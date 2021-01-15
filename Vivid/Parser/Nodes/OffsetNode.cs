using System;

public class OffsetNode : Node, IResolvable
{
	public Node Start => First!;
	public Node Offset => Last!;

	public int? Stride { get; set; }
	public Format? Format { get; set; }

	public static OffsetNode CreateConstantOffset(Node start, long offset, int stride, Format format)
	{
		return new OffsetNode(start, new ContentNode { new NumberNode(Parser.Format, offset) })
		{
			Stride = stride,
			Format = format
		};
	}

	public OffsetNode(Node start, Node offset)
	{
		Add(start);
		Add(offset);
	}

	public OffsetNode(Node start, Node offset, Position? position)
	{
		Add(start);
		Add(offset);
		Position = position;
	}

	public int GetStride()
	{
		if (Stride != null)
		{
			return (int)Stride;
		}

		var type = GetType();

		return Equals(type, Types.LINK) ? 1 : type.ReferenceSize;
	}

	public Format GetFormat()
	{
		if (Format != null)
		{
			return (Format)Format;
		}

		var type = GetType();

		return Equals(type, Types.LINK) ? global::Format.UINT8 : type.Format;
	}

	private LinkNode CreateOperatorFunctionCall(Node target, string function, Node parameters)
	{
		var parameter_types = Resolver.GetTypes(Offset);

		// If the parameter type list is null, it means that one or more of the parameters could not be resolved
		if (parameter_types == null)
		{
			return new LinkNode(target, new UnresolvedFunction(function, Position).SetParameters(parameters), Position);
		}

		var operator_functions = target.GetType().GetFunction(function) ?? throw new InvalidOperationException("Tried to create an operator function call but the function did not exist");
		var operator_function = operator_functions.GetImplementation(parameter_types);

		if (operator_function == null)
		{
			return new LinkNode(target, new UnresolvedFunction(function, Position).SetParameters(parameters), Position);
		}

		return new LinkNode(target, new FunctionNode(operator_function, Position).SetParameters(parameters),Position);
	}

	private Node? TryResolveAsIndexedGetter(Type type)
	{
		// Determine if this node represents a setter
		if (Parent != null && Parent.Is(NodeType.OPERATOR) && Parent.To<OperatorNode>().Operator.Type == OperatorType.ACTION && Parent.First == this)
		{
			// Indexed accessor setter is handled elsewhere
			return null;
		}

		// Ensure that the type contains overload for an indexed accessor (getter)
		return !type.IsLocalFunctionDeclared(Type.INDEXED_ACCESSOR_GETTER_IDENTIFIER) ? null : CreateOperatorFunctionCall(Start, Type.INDEXED_ACCESSOR_GETTER_IDENTIFIER, Offset);
	}

	public virtual Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Start);
		Resolver.Resolve(context, Offset);

		var type = Start.TryGetType();

		if (type == Types.UNKNOWN)
		{
			return null;
		}

		return TryResolveAsIndexedGetter(type);
	}

	public override Type? TryGetType()
	{
		return Start.TryGetType()?.GetOffsetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.OFFSET;
	}

	public override bool Equals(object? other)
	{
		return other is OffsetNode && base.Equals(other);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		return hash.ToHashCode();
	}

	public Status GetStatus()
	{
		return TryGetType() == null ? Status.Error(Position, "Could not resolve the type of the accessor") : Status.OK;
	}
}