using System.Linq;

public class LambdaNode : Node, IResolvable, IType
{
	private Status Status { get; set; } = Status.Error("Could not resolve parameter types of the short function");

	public Lambda Lambda { get; private set; }

	public LambdaNode(Lambda lambda)
	{
		Lambda = lambda;
	}

	public new Type? GetType()
	{
		if (Lambda.Implementations.Any() && Lambda.Implementation.ReturnType != null)
		{
			return new CallDescriptorType(Lambda.Parameters.Select(p => p.Type).ToList(), Lambda.Implementation.ReturnType);
		}

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LAMBDA;
	}

	public Node? Resolve(Context context)
	{
		if (Lambda.Implementations.Any())
		{
			Status = Status.OK;
			return null;
		}

		// Try to resolve all parameter types
		foreach (var parameter in Lambda.Parameters)
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

		if (Lambda.Parameters.Any(p => p.Type == null || p.Type.IsUnresolved))
		{
			return null;
		}

		Status = Status.OK;
		Lambda.Implement(Lambda.Parameters.Select(p => p.Type!));

		return null;
	}

	public Status GetStatus()
	{
		return Status;
	}
}