using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateType : Type
{
   private const string TEMPLATE_ARGUMENT_SIZE_ACCESSOR = "size";
   private const string TEMPLATE_ARGUMENT_NAME_ACCESSOR = "name";

   private const int NAME = 0;

   public List<string> TemplateArgumentNames { get; private set; }
   public int TemplateArgumentCount => TemplateArgumentNames.Count;

   private List<Token> Blueprint { get; set; }
   private Dictionary<string, Type> Variants { get; set; } = new Dictionary<string, Type>();

   public static TemplateType? TryGetTemplateType(Context environment, string name, Node parameters)
   {
      if (!environment.IsTemplateTypeDeclared(name)) return null;

      var template_type = (TemplateType) environment.GetType(name)!;

      // Check if the template type has the same amount of arguments as this function has parameters
      return template_type.TemplateArgumentCount == parameters.Count() ? template_type : null;
   }

   public static Type SolveTemplateTypeVariant(Context environment, TemplateType template_type, Node parameters)
   {
      var types = Resolver.GetTypes(parameters);

      // Check if the type could be resolved
      var variant = types == null
         ? new UnresolvedType(environment, new UnresolvedTemplateType(template_type, parameters))
         : template_type[types.ToArray()];

      return variant;
   }

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
      for (var i = 0; i < tokens.Count; i++)
      {
         if (tokens[i].Type == TokenType.IDENTIFIER)
         {
            var j = TemplateArgumentNames.IndexOf(tokens[i].To<IdentifierToken>().Value);

            if (j == -1)
            {
               continue;
            }

            var type = arguments[j];

            // Check if accessor pattern is possible (e.g T.size or T.name)
            if (i + 2 < tokens.Count && tokens[i + 1].Type == TokenType.OPERATOR && tokens[i + 1].To<OperatorToken>().Operator == Operators.DOT && tokens[i + 2].Type == TokenType.IDENTIFIER)
            {
               switch (tokens[i + 2].To<IdentifierToken>().Value)
               {
                  case TEMPLATE_ARGUMENT_SIZE_ACCESSOR: 
                  {
                     tokens.RemoveRange(i, 3);
                     tokens.Insert(i, new NumberToken(type.ReferenceSize));
                     continue;
                  }

                  case TEMPLATE_ARGUMENT_NAME_ACCESSOR:
                  {
                     tokens.RemoveRange(i, 3);
                     tokens.Insert(i, new StringToken(type.Name));
                     continue;
                  }

                  default: break;
               }
            }

            tokens[i].To<IdentifierToken>().Value = type.Name;
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
      blueprint[NAME].To<IdentifierToken>().Value = Name + '(' + string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name)) + ')';

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
      variant.Identifier = Name;
      variant.OnAddDefinition = mangle => 
      {
         mangle.Add(this);

         mangle += 'I';
         mangle += arguments.Take(TemplateArgumentNames.Count);
         mangle += 'E';
      };

      Variants.Add(identifier, variant);

      return variant;
   }

   public Type GetVariant(Type[] arguments)
   {
      if (arguments.Length < TemplateArgumentNames.Count)
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