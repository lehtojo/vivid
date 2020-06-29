using System;
using System.Collections.Generic;

public class OperatorNode : Node, IType, IResolvable
{
	public Operator Operator { get; }

	public Node Left => First!;
	public Node Right => Last!;

	public OperatorNode(Operator operation)
	{
		Operator = operation;
	}

	public OperatorNode(Operator operation, Node left, Node right)
	{
		Operator = operation;
		SetOperands(left, right);
	}

	public OperatorNode SetOperands(Node left, Node right)
	{
		Add(left);
		Add(right);

		return this;
	}

	private Type? GetClassicType()
	{
		Type? left;

		if (Left is IType a)
		{
			var type = a.GetType();

			if (!(Operator is ClassicOperator operation))
			{
				throw new InvalidOperationException("Invalid operator given");
			}

			if (!operation.IsShared)
			{
				return type;
			}

			left = type;
		}
		else
		{
			return Types.UNKNOWN;
		}

		Type? right;

		if (Right is IType b)
		{
			right = b.GetType();
		}
		else
		{
			return Types.UNKNOWN;
		}

		return Resolver.GetSharedType(left, right);
	}

	private Type? GetActionType()
	{
		if (Left is IType type)
		{
			return type.GetType();
		}

		return Types.UNKNOWN;
	}

	public virtual new Type? GetType()
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

	public override NodeType GetNodeType()
	{
		return NodeType.OPERATOR_NODE;
	}

	public override bool Equals(object? obj)
	{
		return obj is OperatorNode node &&
				base.Equals(obj) &&
				EqualityComparer<Operator>.Default.Equals(Operator, node.Operator);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Operator);
		return hash.ToHashCode();
	}

	private LinkNode CreateOperatorFunctionCall(Node target, string function, Node parameters)
	{
		var parameter_types = Resolver.GetTypes(Right);

		// If the parameter type list is null, it means that one or more of the parameters could not be resolved
		if (parameter_types == null)
		{
			return new LinkNode(
				target,
				new UnresolvedFunction(function)
					.SetParameters(parameters)
			);
		}
		
		var operator_functions = target.GetType().GetFunction(function) ?? 
		                         throw new InvalidOperationException("Tried to create an operator function call but the function didn't exist");
		
		var operator_function = operator_functions[parameter_types];

		if (operator_function == null)
		{
			return new LinkNode(
				target,
				new UnresolvedFunction(function)
					.SetParameters(parameters)
			);
		}
				
		return new LinkNode(
			target,
			new FunctionNode(operator_function)
				.SetParameters(parameters)
		);
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
		
		if (!type.IsLocalFunctionDeclared(Type.INDEXED_ACCESSOR_SETTER_IDENTIFIER)) return null;
		
		// Since the left node represents an indexed accessor its last node must represent the 'indices'
		var parameters = Left.Last!;

		// Since the current node is the assign-operator the right node must represent the assigned value which should be the last parameter
		parameters.Add(Right);

		return CreateOperatorFunctionCall(target, Type.INDEXED_ACCESSOR_SETTER_IDENTIFIER, parameters);
	}

	private Node? TryResolveAsIndexedGetter(Type type)
	{
		// Determine if this node represents a setter
		if (Parent != null && Parent.Is(NodeType.OPERATOR_NODE) &&
		    Parent.To<OperatorNode>().Operator.Type == OperatorType.ACTION)
		{
			// Indexed accessor setter is handled elsewhere
			return null;
		}
		
		// Ensure that the type contains overload for an indexed accessor (getter)
		return !type.IsLocalFunctionDeclared(Type.INDEXED_ACCESSOR_GETTER_IDENTIFIER) ? null : CreateOperatorFunctionCall(Left, Type.INDEXED_ACCESSOR_GETTER_IDENTIFIER, Right);
	}

	public virtual Node? Resolve(Context context)
	{
		// First resolve any problems in the other nodes
		Resolver.Resolve(context, Left);
		Resolver.Resolve(context, Right);
		
		// Check if the left node represents an indexed accessor and if it's being assigned a value
		if (Operator.Type == OperatorType.ACTION && Left.Is(NodeType.OPERATOR_NODE) &&
		    Equals(Left.To<OperatorNode>().Operator, Operators.COLON))
		{
			return TryResolveAsIndexedSetter();
		}
		
		var type = Left.TryGetType();

		if (Equals(type, Types.UNKNOWN))
		{
			return null;
		}

		// Check if this node represents an indexed accessor
		if (Equals(Operator, Operators.COLON))
		{
			return TryResolveAsIndexedGetter(type);
		}

		if (!type.IsOperatorOverloaded(Operator)) return null;
		
		// Retrieve the function name corresponding to the operator of this node
		var operator_function_name = Type.OPERATOR_OVERLOAD_FUNCTIONS[Operator];
		
		// Construct the parameters
		var parameters = new Node();
		parameters.Add(Right);

		return CreateOperatorFunctionCall(Left, operator_function_name, parameters);
	}

	public virtual Status GetStatus()
	{
		return Status.OK;
	}
}
