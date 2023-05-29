using System;
using System.Collections.Generic;

public class OperatorNode : Node, IResolvable
{
	public Operator Operator { get; }

	public OperatorNode(Operator operation)
	{
		Operator = operation;
		Instance = NodeType.OPERATOR;
	}

	public OperatorNode(Operator operation, Position? position)
	{
		Operator = operation;
		Position = position;
		Instance = NodeType.OPERATOR;
	}

	public OperatorNode SetOperands(Node left, Node right)
	{
		Add(left);
		Add(right);

		return this;
	}

	/// <summary>
	/// Returns whether the user is attempting to modify a memory address
	/// </summary>
	private bool IsAddressModification(Type? left_type, Type? right_type)
	{
		if (left_type == null || right_type == null) return false;

		// The right operand must be an integer type
		if (!right_type.IsNumber || right_type.Format == Format.DECIMAL) return false;

		// Allow links and array types as left operands
		return Primitives.IsPrimitive(left_type, Primitives.LINK) || left_type is ArrayType;
	}

	private Type? GetClassicType()
	{
		var left = Left.TryGetType();
		var right = Right.TryGetType();

		if (IsAddressModification(left, right)) return left;

		return Resolver.GetSharedType(left, right);
	}

	private Type? GetAssignmentType()
	{
		return Left.TryGetType();
	}

	public override Type? TryGetType()
	{
		return Operator.Type switch
		{
			OperatorType.CLASSIC => GetClassicType(),
			OperatorType.COMPARISON => Primitives.CreateBool(),
			OperatorType.ASSIGNMENT => GetAssignmentType(),
			OperatorType.LOGICAL => Primitives.CreateBool(),
			_ => throw new Exception("Independent operator should not be processed here")
		};
	}

	private LinkNode CreateOperatorFunctionCall(Node target, string function, Node arguments)
	{
		return new LinkNode(target, new UnresolvedFunction(function, Position).SetArguments(arguments), Position);
	}

	private Node? TryResolveAsSetterAccessor()
	{
		if (!Equals(Operator, Operators.ASSIGN)) return null;

		// Since the left node represents an accessor its first node must represent the target object
		var target = Left.First!;
		var type = target.TryGetType();

		if (type == null || !type.IsLocalFunctionDeclared(Operators.INDEXED_ACCESSOR_SETTER_IDENTIFIER)) return null;

		// Since the left node represents an accessor its last node must represent its arguments
		var arguments = Left.Last!;

		// Since the current node is the assign-operator the right node must represent the assigned value which should be the last parameter
		arguments.Add(Right);

		return CreateOperatorFunctionCall(target, Operators.INDEXED_ACCESSOR_SETTER_IDENTIFIER, arguments);
	}

	public virtual Node? Resolve(Context context)
	{
		// First resolve any problems in the other nodes
		Resolver.Resolve(context, Left);
		Resolver.Resolve(context, Right);

		// Process implicit conversions
		ImplicitConvertor.Process(context, this);

		// Check if the left node represents an indexed accessor and if it is being assigned a value
		if (Operator.Type == OperatorType.ASSIGNMENT && Left.Is(NodeType.ACCESSOR))
		{
			var result = TryResolveAsSetterAccessor();
			if (result != null) return result;
		}

		// Try to resolve this operator node as an operator overload function call
		var type = Left.TryGetType();
		if (type == null) return null;

		if (!type.IsOperatorOverloaded(Operator)) return null;

		// Retrieve the function name corresponding to the operator of this node
		var overload = Operators.Overloads[Operator];
		var arguments = new Node { Right };

		return CreateOperatorFunctionCall(Left, overload, arguments);
	}

	private Status GetActionStatus(Type left, Type right)
	{
		if (Operator == Operators.ASSIGN) return Status.OK;
		return GetClassicStatus(left, right);
	}

	private Status GetClassicStatus(Type left, Type right)
	{
		if (left is not Number)
		{
			// Allow operations such as comparing whether an object is none or not
			if (right is not Number && Operator != Operators.EQUALS && Operator != Operators.NOT_EQUALS && Operator != Operators.ABSOLUTE_EQUALS && Operator != Operators.ABSOLUTE_NOT_EQUALS)
			{
				return Status.Error(Left.Position, $"Type '{left}' does not have an operator overload for operator '{Operator.Identifier}' with argument type '{right}'");
			}

			return Status.OK;
		}

		return right is Number ? Status.OK : Status.Error(Position, "Can not resolve the type of the operation");
	}

	private Status GetLogicStatus(Type left, Type right)
	{
		if (!Primitives.IsPrimitive(left, Primitives.BOOL)) return Status.Error(Left.Position, "Operand must be a bool");
		if (!Primitives.IsPrimitive(right, Primitives.BOOL)) return Status.Error(Right.Position, "Operand must be a bool");
		return Status.OK;
	}

	public virtual Status GetStatus()
	{
		var left = Left.TryGetType();
		var right = Right.TryGetType();

		if (left == null || right == null)
		{
			return Status.Error(Position, "Can not resolve the type of the operation");
		}

		return Operator.Type switch
		{
			OperatorType.ASSIGNMENT => GetActionStatus(left, right),
			OperatorType.CLASSIC => GetClassicStatus(left, right),
			OperatorType.COMPARISON => GetClassicStatus(left, right),
			OperatorType.LOGICAL => GetLogicStatus(left, right),
			_ => Status.OK
		};
	}

	public override bool Equals(object? other)
	{
		return other is OperatorNode node &&
				base.Equals(other) &&
				EqualityComparer<Operator>.Default.Equals(Operator, node.Operator);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Operator.Identifier);
	}

	public override string ToString() => $"Operator {Operator.Identifier}";
}
