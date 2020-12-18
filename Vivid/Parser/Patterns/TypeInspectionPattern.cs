using System.Collections.Generic;
using System.Linq;

public class TypeInspectionPattern : Pattern
{
	public const string SIZE_INSPECTION_IDENTIFIER = "sizeof";
	public const string NAME_INSPECTION_IDENTIFIER = "nameof";

	public const string SIZE_INSPECTION_RUNTIME_IDENTIFIER = "internal_sizeof";
	public const string NAME_INSPECTION_RUNTIME_IDENTIFIER = "internal_nameof";

	public const int PRIORITY = 18;

	public TypeInspectionPattern() : base(TokenType.FUNCTION) {}

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
		var parameters = descriptor.GetParsedParameters(context);
		var parameter = parameters.First();

		if (parameters.Find(i => !i.Is(NodeType.CONTENT) && !i.Is(NodeType.TYPE)) == null)
		{
			var type = parameter.TryGetType();

			if (type == Types.UNKNOWN)
			{
				throw Errors.Get(descriptor.Position, "Could not resolve the specified type");
			}

			if (descriptor.Name == NAME_INSPECTION_IDENTIFIER)
			{
				return new StringNode(type.Name, descriptor.Position);
			}

			return new SizeNode(type);
		}

		descriptor.Identifier.Value = descriptor.Name switch
		{
			SIZE_INSPECTION_IDENTIFIER => SIZE_INSPECTION_RUNTIME_IDENTIFIER,
			NAME_INSPECTION_IDENTIFIER => NAME_INSPECTION_RUNTIME_IDENTIFIER,
			_ => throw Errors.Get(descriptor.Position, $"Unknown type inspection command '{descriptor.Name}'")
		};

		return Singleton.GetFunction(context, context, descriptor);
	}
}