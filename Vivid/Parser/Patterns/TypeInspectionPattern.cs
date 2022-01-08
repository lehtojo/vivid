using System.Collections.Generic;
using System.Linq;

public class TypeInspectionPattern : Pattern
{
	public const string SIZE_INSPECTION_IDENTIFIER = "sizeof";
	public const string CAPACITY_INSPECTION_IDENTIFIER = "capacityof";
	public const string NAME_INSPECTION_IDENTIFIER = "nameof";

	public const int PRIORITY = 18;

	// Pattern: sizeof(...)/capacityof(...)/nameof(...)
	public TypeInspectionPattern() : base(TokenType.FUNCTION) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var descriptor = tokens.First().To<FunctionToken>();

		if (descriptor.Name != SIZE_INSPECTION_IDENTIFIER && descriptor.Name != CAPACITY_INSPECTION_IDENTIFIER && descriptor.Name != NAME_INSPECTION_IDENTIFIER) return false;

		// Create a temporary state which in order to check whether the parameters contains a type
		state = new PatternState(descriptor.Parameters.Tokens);

		return Common.ConsumeType(state) && state.End == state.Tokens.Count;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var descriptor = tokens.First().To<FunctionToken>();
		var type = Common.ReadType(context, new Queue<Token>(descriptor.Parameters.Tokens));

		if (type == null) throw Errors.Get(descriptor.Position, "Can not resolve the inspected type");

		if (descriptor.Name == NAME_INSPECTION_IDENTIFIER)
		{
			if (!type.IsUnresolved) return new StringNode(type.ToString(), descriptor.Position);

			return new InspectionNode(InspectionType.NAME, new TypeNode(type));
		}

		if (descriptor.Name == CAPACITY_INSPECTION_IDENTIFIER)
		{
			return new InspectionNode(InspectionType.CAPACITY, new TypeNode(type));
		}

		return new InspectionNode(InspectionType.SIZE, new TypeNode(type));
	}
}