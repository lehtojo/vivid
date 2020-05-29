using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateType : Type
{
   private const int NAME = 0;

   public List<string> TemplateArgumentNames { get; private set; }

   private List<Token> Blueprint { get; set; }
   private Dictionary<string, Type> Variants { get; set; } = new Dictionary<string, Type>();

   public TemplateType(Context context, string name, int modifiers, List<Token> blueprint, List<string> template_argument_names) : base(context, name, modifiers)
   {
      Blueprint = blueprint;
      TemplateArgumentNames = template_argument_names;
   }

   private Type? TryGetVariant(Type[] arguments)
   {
      var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      if (Variants.TryGetValue(identifier, out Type? variant))
      {
         return variant;
      }

      return null;
   }

   private void InsertArguments(List<Token> tokens, Type[] arguments)
   {
      for (var i = 0; i < tokens.Count(); i++)
      {
         if (tokens[i].Type == TokenType.IDENTIFIER)
         {
            var j = TemplateArgumentNames.IndexOf(tokens[i].To<IdentifierToken>().Value);

            if (j == -1)
            {
               continue;
            }

            tokens[i].To<IdentifierToken>().Value = arguments[j].Name;
         }
         else if (tokens[i].Type == TokenType.CONTENT)
         {
            var content = tokens[i].To<ContentToken>();

            // Go through all the sections inside the content token
            for (var section = 0; section < content.SectionCount; section++)
            {
               InsertArguments(content.GetTokens(section), arguments);
            }
         }
      }
   }

   private Type CreateVariant(Type[] arguments)
   {
      var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      // Copy the blueprint and insert the specified arguments to their places
      var blueprint = Blueprint.Select(t => (Token)t.Clone()).ToList();
      blueprint[NAME].To<IdentifierToken>().Value = Name + '_' + string.Join('_', arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      InsertArguments(blueprint, arguments);

      // Parse the new variant
      var result = Parser.Parse(Parent ?? throw new ApplicationException("Template type didn't have parent context"), blueprint).First;

      if (result == null || !result.Is(NodeType.TYPE_NODE))
      {
         throw new ApplicationException("Tried to parse a new variant from template type but the result wasn't a new type");
      }

      // Parse the body of the type
      result.To<TypeNode>().Parse();

      // Register the new variant
      var variant = result.To<TypeNode>().Type;
      Variants.Add(identifier, variant);

      return variant;
   }

   public Type GetVariant(Type[] arguments)
   {
      if (arguments.Count() < TemplateArgumentNames.Count)
      {
         throw new ApplicationException("Missing template arguments");
      }

      return TryGetVariant(arguments) ?? CreateVariant(arguments);
   }

   public Type this[Type[] arguments]
   {
      get => GetVariant(arguments);
   }
}