using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateFunction : Function
{
   private const int HEAD = 0;

   public List<string> TemplateArgumentNames { get; private set; }
   private Dictionary<string, Function> Variants { get; set; } = new Dictionary<string, Function>();

   public TemplateFunction(Context context, int modifiers, string name, List<Token> blueprint, List<string> template_argument_names) : base(context, modifiers, name, blueprint)
   {
      TemplateArgumentNames = template_argument_names;
   }

   private FunctionImplementation? TryGetVariant(Type[] arguments)
   {
      var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      if (Variants.TryGetValue(identifier, out Function? variant))
      {
         return variant.Get(arguments.ToList());
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

   private FunctionImplementation? CreateVariant(Type[] arguments)
   {
      var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      // Copy the blueprint and insert the specified arguments to their places
      var blueprint = Blueprint.Select(t => (Token)t.Clone()).ToList();
      blueprint[HEAD].To<FunctionToken>().Identifier.Value = Name + '_' + string.Join('_', arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

      InsertArguments(blueprint, arguments);

      // Parse the new variant
      var result = Parser.Parse(Parent ?? throw new ApplicationException("Template function didn't have parent context"), blueprint).First;

      if (result == null || !result.Is(NodeType.FUNCTION_DEFINITION_NODE))
      {
         throw new ApplicationException("Tried to parse a new variant from template function but the result wasn't a new function");
      }

      // Register the new variant
      var variant = result.To<FunctionDefinitionNode>().Function;
      Variants.Add(identifier, variant);

      return variant.Get(arguments.Skip(TemplateArgumentNames.Count).ToList());
   }

   public override bool Passes(List<Type> parameters)
   {
      return parameters.Count == (TemplateArgumentNames.Count + Parameters.Count);
   }

   public override FunctionImplementation? Get(List<Type> arguments)
   {
      if (arguments.Count < TemplateArgumentNames.Count)
      {
         throw new ApplicationException("Missing template arguments");
      }

      return TryGetVariant(arguments.ToArray()) ?? CreateVariant(arguments.ToArray());
   }
}