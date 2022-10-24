using System;

public class AccessorNode : Node, IResolvable
{
	public Node Start => First!;
	public Node Offset => Last!;

	public AccessorNode(Node start, Node offset)
	{
		Add(start);
		Add(new ParenthesisNode { offset });
		Instance = NodeType.ACCESSOR;
	}

	public AccessorNode(Node start, Node offset, Position? position)
	{
		Add(start);
		Add(offset);
		Position = position;
		Instance = NodeType.ACCESSOR;
	}

	public int GetStride()
	{
		return GetType().AllocationSize;
	}

	public Format GetFormat()
	{
		return GetType().Format;
	}

	private LinkNode CreateOperatorFunctionCall(Node target, string function, Node parameters)
	{
		return new LinkNode(target, new UnresolvedFunction(function, Position).SetArguments(parameters), Position);
	}

	private Node? TryResolveAsGetterAccessor(Type type)
	{
		// Determine if this node represents a setter
		if (Parent != null && Parent.Instance == NodeType.OPERATOR && Parent.To<OperatorNode>().Operator.Type == OperatorType.ASSIGNMENT && Parent.First == this)
		{
			// Indexed accessor setter is handled elsewhere
			return null;
		}

		// Ensure that the type contains overload for an indexed accessor (getter)
		return !type.IsLocalFunctionDeclared(Operators.INDEXED_ACCESSOR_GETTER_IDENTIFIER) ? null : CreateOperatorFunctionCall(Start, Operators.INDEXED_ACCESSOR_GETTER_IDENTIFIER, Offset);
	}

	public virtual Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Start);
		Resolver.Resolve(context, Offset);

		var type = Start.TryGetType();
		if (type == null) return null;

		return TryResolveAsGetterAccessor(type);
	}

	public override Type? TryGetType()
	{
		return Start.TryGetType()?.GetAccessorType();
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override bool Equals(object? other)
	{
		return other is AccessorNode && base.Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position);
	}

	public override string ToString() => "Accessor";
}