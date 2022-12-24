using System.Collections.Generic;
using System.Linq;

public class TypeInspectionPattern : Pattern
{
	public const string SIZE_INSPECTION_IDENTIFIER = "sizeof";
	public const string STRIDE_INSPECTION_IDENTIFIER = "strideof";
	public const string NAME_INSPECTION_IDENTIFIER = "nameof";

	// Pattern: sizeof(...)/strideof(...)/nameof(...)
	public TypeInspectionPattern() : base(TokenType.FUNCTION)
	{
		Priority = 18;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		var descriptor = tokens.First().To<FunctionToken>();

		if (descriptor.Name != SIZE_INSPECTION_IDENTIFIER && descriptor.Name != STRIDE_INSPECTION_IDENTIFIER && descriptor.Name != NAME_INSPECTION_IDENTIFIER) return false;

		// Create a temporary state which in order to check whether the parameters contains a type
		state = new ParserState(descriptor.Parameters.Tokens);

		return Common.ConsumeType(state) && state.End == state.All.Count;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var descriptor = tokens.First().To<FunctionToken>();
		var type = Common.ReadType(context, descriptor.Parameters.Tokens);

		if (type == null) throw Errors.Get(descriptor.Position, "Can not resolve the inspected type");

		if (descriptor.Name == NAME_INSPECTION_IDENTIFIER)
		{
			if (!type.IsUnresolved) return new StringNode(type.ToString(), descriptor.Position);

			return new InspectionNode(InspectionType.NAME, new TypeNode(type));
		}

		if (descriptor.Name == STRIDE_INSPECTION_IDENTIFIER)
		{
			return new InspectionNode(InspectionType.STRIDE, new TypeNode(type));
		}

		return new InspectionNode(InspectionType.SIZE, new TypeNode(type));
	}
}