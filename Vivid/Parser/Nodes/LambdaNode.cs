using System;
using System.Linq;

public class LambdaNode : Node, IResolvable
{
	private Status Status { get; set; }

	public Function Function { get; private set; }
	public FunctionImplementation? Implementation { get; set; }

	public LambdaNode(Function lambda, Position position)
	{
		Function = lambda;
		Position = position;
		Status = new Status(Position, "Can not resolve parameter types of this lambda");
		Instance = NodeType.LAMBDA;
	}

	public LambdaNode(FunctionImplementation implementation, Position position)
	{
		Implementation = implementation;
		Function = implementation.Metadata ?? throw new ApplicationException("Missing function implementation metadata");
		Position = position;
		Status = Status.OK;
		Instance = NodeType.LAMBDA;
	}

	public FunctionType GetIncompleteType()
	{
		return new FunctionType(Function.Parameters.Select(i => i.Type).ToList(), Implementation?.ReturnType, Position);
	}

	public override Type? TryGetType()
	{
		if (Implementation != null && Implementation.ReturnType != null)
		{
			return new FunctionType(Function.Parameters.Select(i => i.Type).ToList(), Implementation.ReturnType, Position);
		}

		return null;
	}

	public Node? Resolve(Context context)
	{
		if (Implementation != null)
		{
			Status = Status.OK;
			return null;
		}

		// Try to resolve all parameter types
		foreach (var parameter in Function.Parameters)
		{
			if (parameter.Type == null) continue;

			if (parameter.Type.IsUnresolved)
			{
				// Try to resolve the parameter type
				var type = parameter.Type.To<UnresolvedType>().ResolveOrNull(context);

				if (type != null) { parameter.Type = type; }
			}
		}

		// Before continuing, ensure all parameters are resolved
		if (Function.Parameters.Any(i => i.Type == null || i.Type.IsUnresolved)) return null;

		Status = Status.OK;
		Implementation = Function.Implement(Function.Parameters.Select(i => i.Type!));

		return null;
	}

	public Status GetStatus()
	{
		return Status;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Function.Identity);
	}
}