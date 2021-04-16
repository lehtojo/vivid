using System.Collections.Generic;
using System.Linq;

public class InheritancePattern : Pattern
{
	public const int INHERITANT = 0;
	public const int TEMPLATE_ARGUMENTS = 1;
	public const int INHERITOR = 2;

	public const int PRIORITY = 21;

	// Pattern: $type [<$1, $2, ..., $n>] [\n] $type_definition
	public InheritancePattern() : base(TokenType.IDENTIFIER) { }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Remove the consumed token
		state.End--;
		state.Formatted.RemoveAt(0);

		if (!Common.ConsumeType(state)) return false;

		// Consume a line break if one is present
		Consume(state, TokenType.END);

		return Consume(state, out Token? x, TokenType.DYNAMIC) && x is DynamicToken y && y.Node is TypeNode z && z.IsDefinition;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var inheritant_tokens = tokens.Take(tokens.Count - 1).Where(i => !i.Is(TokenType.END)).ToArray();

		var inheritor_node = tokens.Last().To<DynamicToken>().Node.To<TypeNode>();
		var inheritor = inheritor_node.Type;

		if (inheritor.IsTemplateType)
		{
			var template_type = inheritor.To<TemplateType>();

			// If any of the inherited tokens represent a template argument, the inheritant tokens must be added to the template type
			if (inheritant_tokens.Any(i => i is IdentifierToken x && template_type.TemplateArgumentNames.Any(j => x.Value == j)))
			{
				template_type.Inherited.InsertRange(0, inheritant_tokens);
				return inheritor_node;
			}
		}

		var inheritant_type = Common.ReadType(context, new Queue<Token>(inheritant_tokens));

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

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}