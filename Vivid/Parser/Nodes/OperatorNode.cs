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

	private Type? GetClassicType()
	{
		var left = Left.TryGetType();

		if (Operator is not ClassicOperator operation)
		{
			throw new InvalidOperationException("Operator was being processed as a classical operator but it was not one");
		}

		if (!operation.IsShared)
		{
			return left;
		}

		var right = Right.TryGetType();

		// Return the left type only if it represents a link and it is modified with a number type
		if (left is Link && right is Number && right != Types.DECIMAL && (Operator == Operators.ADD || Operator == Operators.SUBTRACT || Operator == Operators.MULTIPLY))
		{
			return left;
		}

		return Resolver.GetSharedType(left, right);
	}

	private Type? GetActionType()
	{
		return Left.TryGetType();
	}

	public override Type? TryGetType()
	{
		return Operator.Type switch
		{
			OperatorType.CLASSIC => GetClassicType(),
			OperatorType.COMPARISON => Types.BOOL,
			OperatorType.ACTION => GetActionType(),
			OperatorType.LOGIC => Types.BOOL,
			_ => throw new Exception("Independent operator should not be processed here")
		};
	}

	private LinkNode CreateOperatorFunctionCall(Node target, string function, Node parameters)
	{
		var parameter_types = Resolver.GetTypes(Right);

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

		return new LinkNode(target, new FunctionNode(operator_function, Position).SetParameters(parameters), Position);
	}

	private Node? TryResolveAsIndexedSetter()
	{
		if (!Equals(Operator, Operators.ASSIGN))
		{
			return null;
		}

		// Since the left node represents an indexed accessor its first node must represent the target object
		var target = Left.First!;
		var type = target.TryGetType();

		if (Equals(type, Types.UNKNOWN))
		{
			return null;
		}

		if (!type.IsLocalFunctionDeclared(Type.INDEXED_ACCESSOR_SETTER_IDENTIFIER))
		{
			return null;
		}

		// Since the left node represents an indexed accessor its last node must represent the 'indices'
		var parameters = Left.Last!;

		// Since the current node is the assign-operator the right node must represent the assigned value which should be the last parameter
		parameters.Add(Right);

		return CreateOperatorFunctionCall(target, Type.INDEXED_ACCESSOR_SETTER_IDENTIFIER, parameters);
	}

	public virtual Node? Resolve(Context context)
	{
		// First resolve any problems in the other nodes
		Resolver.Resolve(context, Left);
		Resolver.Resolve(context, Right);

		// Check if the left node represents an indexed accessor and if it is being assigned a value
		if (Operator.Type == OperatorType.ACTION && Left.Is(NodeType.OFFSET))
		{
			var result = TryResolveAsIndexedSetter();

			if (result != null)
			{
				return result;
			}
		}

		var type = Left.TryGetType();

		if (Equals(type, Types.UNKNOWN))
		{
			return null;
		}

		if (!type.IsOperatorOverloaded(Operator))
		{
			return null;
		}

		// Retrieve the function name corresponding to the operator of this node
		var operator_function_name = Type.OPERATOR_OVERLOAD_FUNCTIONS[Operator];

		// Construct the parameters
		var parameters = new Node { Right };

		return CreateOperatorFunctionCall(Left, operator_function_name, parameters);
	}

	private Status GetActionStatus(Type left, Type right)
	{
		// Assign operator is a special operator since it is a little bit unsafe
		if (Operator == Operators.ASSIGN)
		{
			return Status.OK;
		}

		return GetClassicStatus(left, right);
	}

	private Status GetClassicStatus(Type left, Type right)
	{
		if (left is not Number)
		{
			// Allow operations such as comparing whether an object is a null pointer or not
			if (right is not Number)
			{
				return Status.Error(Left.Position, $"Type '{left}' does not have an operator overload for operator '{Operator.Identifier}' with argument type '{right}'");
			}
			
			return Status.OK;
		}

		return right is Number ? Status.OK : Status.Error("Could not resolve the type of the operation");
	}

	private Status GetLogicStatus(Type left, Type right)
	{
		if (left != Types.BOOL)
		{
			return Status.Error(Left.Position, $"The type of the operand must be '{Types.BOOL.Identifier}' since its parent is a logical operator");
		}

		if (right != Types.BOOL)
		{
			return Status.Error(Right.Position, $"The type of the operand must be '{Types.BOOL.Identifier}' since its parent is a logical operator");
		}

		return Status.OK;
	}

	public virtual Status GetStatus()
	{
		var left = Left.TryGetType();
		var right = Right.TryGetType();

		if (left == Types.UNKNOWN || right == Types.UNKNOWN)
		{
			return Status.Error(Position, "Could not resolve the type of the operation");
		}

		return Operator.Type switch
		{
			OperatorType.ACTION => GetActionStatus(left, right),
			OperatorType.CLASSIC => GetClassicStatus(left, right),
			OperatorType.COMPARISON => GetClassicStatus(left, right),
			OperatorType.LOGIC => GetLogicStatus(left, right),
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
}
