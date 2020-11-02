public class IsNode : Node, IResolvable, IType
{
	public Node Object => First!;
	public Type Type { get; private set; }
	public Variable Result => Last!.To<VariableNode>().Variable;

	public bool HasResultVariable => Count() == 2;

	public IsNode(Node source, Type type, Variable? result)
	{
		Add(source);

		if (result != null)
		{
			Add(new VariableNode(result));
		}

		Type = type;
	}

	public new Type GetType()
	{
		return Types.BOOL;
	}

	public Node? Resolve(Context context)
	{
		// Try to resolve the object
		Resolver.Resolve(context, Object);

		// Try to resolve the type
		Type = Resolver.Resolve(context, Type) ?? Type;

		return null;
	}

	public Status GetStatus()
	{
		if (Type is IResolvable x)
		{
			var status = x.GetStatus();

			if (status.IsProblematic)
			{
				return status;
			}
		}

		return Object is IResolvable y ? y.GetStatus() : Status.OK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IS;
	}
}
