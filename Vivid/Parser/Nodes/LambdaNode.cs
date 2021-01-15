using System.Linq;
using System;

public class LambdaNode : Node, IResolvable
{
	private Status Status { get; set; }

	public Function Function { get; private set; }
	public FunctionImplementation? Implementation { get; set; }

	public LambdaNode(Function lambda, Position position)
	{
		Function = lambda;
		Position = position;
		Status = Status.Error(Position, "Could not resolve parameter types of the short function");
	}

	public LambdaNode(FunctionImplementation implementation, Position position)
	{
		Implementation = implementation;
		Function = implementation.Metadata ?? throw new ApplicationException("Missing function implementation metadata");
		Position = position;
		Status = Status.OK;
	}

	public CallDescriptorType GetIncompleteType()
	{
		return new CallDescriptorType(Function.Parameters.Select(p => p.Type).ToList(), Implementation?.ReturnType);
	}

	public override Type? TryGetType()
	{
		if (Implementation != null && Implementation.ReturnType != null)
		{
			return new CallDescriptorType(Function.Parameters.Select(p => p.Type).ToList(), Implementation.ReturnType);
		}

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LAMBDA;
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
			if (parameter.Type == null)
			{
				continue;
			}

			if (parameter.Type.IsUnresolved)
			{
				// Try to resolve the parameter type
				var type = parameter.Type.To<UnresolvedType>().TryResolveType(context);

				if (type != Types.UNKNOWN)
				{
					parameter.Type = type;
				}
			}
		}

		if (Function.Parameters.Any(p => p.Type == null || p.Type.IsUnresolved))
		{
			return null;
		}

		Status = Status.OK;
		Implementation = Function.Implement(Function.Parameters.Select(p => p.Type!));

		return null;
	}

	public Status GetStatus()
	{
		return Status;
	}
}