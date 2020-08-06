using System.Linq;

public class LambdaNode : Node, IResolvable, IType
{
	private Status Status { get; set; } = Status.Error("Couldn't resolve parameter types of the short function");

   public Function Lambda { get; private set; }

	public LambdaNode(Function lambda)
	{
      Lambda = lambda;
	}

	public new Type? GetType()
	{
		return new LambdaType(Lambda.Parameters.Select(p => p.Type).ToList(), Types.UNKNOWN);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LAMBDA_NODE;
	}

   public Node? Resolve(Context context)
   {
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