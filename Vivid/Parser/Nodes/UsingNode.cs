using System;

public class UsingNode : Node, IResolvable
{
	public bool IsAllocatorResolved { get; set; } = false;

	public UsingNode(Node allocated, Node allocator, Position position)
	{
		Instance = NodeType.USING;
		Position = position;
		Add(allocated);
		Add(allocator);
	}

	public UsingNode(Position position)
	{
		Instance = NodeType.USING;
		Position = position;
	}

	public override Type? TryGetType()
	{
		return Left.TryGetType();
	}

	public void AddAllocatorFunction()
	{
		if (IsAllocatorResolved) return;

		if (!(Left.Instance == NodeType.CONSTRUCTION) && !(Left.Instance == NodeType.LINK && Left.Right.Instance == NodeType.CONSTRUCTION)) return;

		var allocated_type = Left.TryGetType();
		if (allocated_type == null || allocated_type.IsUnresolved) return;

		var allocator_type = Right.TryGetType();
		if (allocator_type == null || allocator_type.IsUnresolved) return;

		// If the allocator is an integer or a link, treat it as an address where the object should be allocated
		if ((allocator_type.IsNumber && allocator_type.Format != Format.DECIMAL) || allocator_type is Link)
		{
			IsAllocatorResolved = true;
			return;
		}

		var allocator_function_name = Parser.STANDARD_ALLOCATOR_FUNCTION;

		if (!allocator_type.IsFunctionDeclared(allocator_function_name) && !allocator_type.IsVirtualFunctionDeclared(allocator_function_name)) return;

		var allocator_object = Right;
		allocator_object.Remove();

		var size = Math.Max(1, allocated_type.ContentSize);
		var arguments = new Node();
		arguments.Add(new NumberNode(Parser.Signed, (long)size, Position));

		var allocator_call = new UnresolvedFunction(allocator_function_name, Position);
		allocator_call.SetArguments(arguments);

		Add(new LinkNode(allocator_object, allocator_call, Position));
		IsAllocatorResolved = true;
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Left);
		Resolver.Resolve(context, Right);

		AddAllocatorFunction();

		return (Node?)null;
	}

	public Status GetStatus()
	{
		// 1. Verify the allocated object is a construction
		if (!(Left.Instance == NodeType.CONSTRUCTION) && !(Left.Instance == NodeType.LINK && Left.Right.Instance == NodeType.CONSTRUCTION))
		{
			return new Status(Position, "Left side must be a construction");
		}

		// 2. Verify the allocator has an allocation function
		var allocator_type = Right.TryGetType();
		if (allocator_type == null || allocator_type.IsUnresolved) return new Status(Position, "Can not resolve the type of the allocator");

		// If the allocator is an integer or a link, treat it as an address where the object should be allocated
		if ((allocator_type.IsNumber && allocator_type.Format != Format.DECIMAL) || allocator_type is Link) return Status.OK;

		if (!allocator_type.IsFunctionDeclared(Parser.STANDARD_ALLOCATOR_FUNCTION) && !allocator_type.IsVirtualFunctionDeclared(Parser.STANDARD_ALLOCATOR_FUNCTION))
		{
			return new Status(Position, "Allocator does not have allocation function: allocate(size: i64): link");
		}

		return Status.OK;
	}

	public override string ToString()
	{
		return "Using";
	}
}