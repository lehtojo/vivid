using System;

public class IsNode : Node, IResolvable
{
	public Node Object => First!;
	public Type Type { get; private set; }
	public Variable Result => Last!.To<VariableNode>().Variable;

	public bool HasResultVariable => Count() == 2;

	public IsNode(Node source, Type type, Variable? result, Position? position = null)
	{
		Type = type;
		Position = position;
		Instance = NodeType.IS;

		Add(source);

		if (result != null)
		{
			Add(new VariableNode(result, Position));
		}
	}

	public override Type? TryGetType()
	{
		return Primitives.CreateBool();
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

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Type);
	}

	public override string ToString() => "Is";
}
