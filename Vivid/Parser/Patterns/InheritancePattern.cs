using System.Collections.Generic;
using System.Linq;

public class InheritancePattern : Pattern
{
	public const int INHERITANT = 0;
	public const int TEMPLATE_ARGUMENTS = 1;
	public const int INHERITOR = 2;

	// NOTE: There can not be an optional line break since function import return types can be consumed accidentally for example
	// Pattern: $type [<$1, $2, ..., $n>] $type_definition
	public InheritancePattern() : base(TokenType.IDENTIFIER) { Priority = 21; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Remove the consumed token
		state.End--;
		state.Tokens.RemoveAt(0);

		if (!Common.ConsumeType(state)) return false;

		// Require the next token to represent a type definition
		var next = state.Peek();
		if (next == null || next.Type != TokenType.DYNAMIC)return false;

		var node = next.To<DynamicToken>().Node;
		if (node.Instance != NodeType.TYPE_DEFINITION) return false;

		state.Consume();
		return true;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var inheritant_tokens = tokens.Take(tokens.Count - 1).ToList();

		var inheritor_node = tokens.Last().To<DynamicToken>().Node.To<TypeDefinitionNode>();
		var inheritor = inheritor_node.Type;

		if (inheritor.IsTemplateType)
		{
			var template_type = inheritor.To<TemplateType>();

			// If any of the inherited tokens represent a template argument, the inheritant tokens must be added to the template type
			/// NOTE: Inherited types, which are not dependent on template arguments, can be added as a supertype directly
			if (inheritant_tokens.Any(i => i.Type == TokenType.IDENTIFIER && template_type.TemplateParameters.Contains(i.To<IdentifierToken>().Value)))
			{
				template_type.Inherited.InsertRange(0, inheritant_tokens);
				return inheritor_node;
			}
		}

		var inheritant_type = Common.ReadType(context, inheritant_tokens);

		if (inheritant_type == null)
		{
			throw Errors.Get(inheritant_tokens.First().Position, "Can not resolve the inherited type");
		}

		if (!inheritor.IsInheritingAllowed(inheritant_type))
		{
			throw Errors.Get(inheritant_tokens.First().Position, "Can not inherit the type since it would have caused a cyclic inheritance");
		}

		inheritor.Supertypes.Insert(0, inheritant_type);
		return inheritor_node;
	}
}