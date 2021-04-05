using System.Collections.Generic;
using System.Linq;

public class TypeInspectionPattern : Pattern
{
	public const string SIZE_INSPECTION_IDENTIFIER = "sizeof";
	public const string NAME_INSPECTION_IDENTIFIER = "nameof";

	public const int PRIORITY = 18;

	// Pattern: sizeof(...)/nameof(...)
	public TypeInspectionPattern() : base(TokenType.FUNCTION) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var descriptor = tokens.First().To<FunctionToken>();

		if (descriptor.Name != SIZE_INSPECTION_IDENTIFIER && descriptor.Name != NAME_INSPECTION_IDENTIFIER)
		{
			return false;
		}

		return descriptor.GetParsedParameters(context).Count() == 1;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var descriptor = tokens.First().To<FunctionToken>();
		var parameter = descriptor.GetParsedParameters(context).First();

		if (descriptor.Name == NAME_INSPECTION_IDENTIFIER)
		{
			var type = parameter.TryGetType();

			if (type != Types.UNKNOWN)
			{
				return new StringNode(type.ToString(), descriptor.Position);
			}

			return new InspectionNode(InspectionType.NAME, parameter);
		}

		return new InspectionNode(InspectionType.SIZE, parameter);
	}
}