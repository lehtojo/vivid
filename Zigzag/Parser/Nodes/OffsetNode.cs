using System;

public class OffsetNode : Node, IType, IResolvable
{
	public Node Start => First!;
   public Node Offset => Last!;

	public OffsetNode(Node start, Node offset)
	{
      Add(start);
      Add(offset);
	}

   private LinkNode CreateOperatorFunctionCall(Node target, string function, Node parameters)
	{
		var parameter_types = Resolver.GetTypes(Offset);

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

	public new Type? GetType()
	{
		return Start.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.OFFSET_NODE;
	}

	public override bool Equals(object? obj)
	{
		return obj is OffsetNode && base.Equals(obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		return hash.ToHashCode();
	}

   public Status GetStatus()
   {
      return Status.OK;
   }
}