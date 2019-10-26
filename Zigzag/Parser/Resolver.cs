using System;
using System.Collections.Generic;

public class Resolver
{

	/**
    * Tries to resolve any unresolved nodes in a node tree
    * @param context Context to use when resolving
    * @param node Node tree
    * @param errors Output list for errors
    * @return Returns a resolved node tree on success, otherwise null
    */
	public static Node Resolve(Context context, Node node, List<Exception> errors)
	{
		if (node is Resolvable resolvable)
		{
			try
			{
				Node resolved = resolvable.Resolve(context);
				return resolved ?? node;
			}
			catch (Exception e)
			{
				errors.Add(e);
			}

			return null;
		}
		else
		{
			Node iterator = node.First;

			while (iterator != null)
			{
				Node resolved;

				if (iterator.GetNodeType() == NodeType.TYPE_NODE)
				{
					TypeNode type = (TypeNode)iterator;
					resolved = Resolver.Resolve(type.Type, iterator, errors);
					Resolver.ResolveVariables(type.Type, errors);
				}
				else if (iterator.GetNodeType() == NodeType.FUNCTION_NODE)
				{
					FunctionNode function = (FunctionNode)iterator;
					resolved = Resolver.Resolve(function.Function, iterator, errors);
					Resolver.ResolveVariables(function.Function, errors);
				}
				else
				{
					resolved = Resolver.Resolve(context, iterator, errors);
				}

				if (resolved != null)
				{
					iterator.Replace(resolved);
				}

				iterator = iterator.Next;
			}

			return node;
		}
	}

	private static Type GetSharedNumber(Number a, Number b)
	{
		return a.Bits > b.Bits ? a : b;
	}

	/**
     * Returns the shared type between the given types
     * @return Success: Shared type between the given types, Failure: null
     */
	public static Type GetSharedType(Type a, Type b)
	{
		if (a == b)
		{
			return a;
		}
		else if (a == Types.UNKNOWN || b == Types.UNKNOWN)
		{
			return Types.UNKNOWN;
		}

		if (a is Number && b is Number)
		{
			return GetSharedNumber((Number)a, (Number)b);
		}

		foreach (Type type in a.Supertypes)
		{
			if (b.Supertypes.Contains(type))
			{
				return type;
			}
		}

		return Types.UNKNOWN;
	}

	/**
     * Returns the shared type between the given types
     * @param types List of types to solve
     * @return Success: Shared type between the given types, Failure: null
     */
	public static Type GetSharedType(List<Type> types)
	{
		if (types.Count == 0)
		{
			return Types.UNKNOWN;
		}
		else if (types.Count == 1)
		{
			return types[0];
		}

		Type current = types[0];

		for (int i = 1; i < types.Count; i++)
		{
			if (current == null)
			{
				break;
			}

			current = Resolver.GetSharedType(current, types[i]);
		}

		return current;
	}

	/**
     * Returns all child node types
     * @return Types of children nodes
     */
	public static List<Type> GetTypes(Node node)
	{
		List<Type> types = new List<Type>();
		Node iterator = node.First;

		while (iterator != null)
		{
			if (iterator is Contextable contextable)
			{
				Context context;

				try
				{
					context = contextable.GetContext();
				}
				catch
				{
					return null;
				}

				if (context == null || !context.IsType)
				{
					return null;
				}
				else
				{
					Type type = (Type)context;

					if (type is UnresolvedType)
					{
						return null;
					}

					types.Add(type);
				}
			}
			else
			{
				return null;
			}

			iterator = iterator.Next;
		}

		return types;
	}

	public static void ResolveVariables(Variable variable)
	{
		List<Type> types = new List<Type>();

		foreach (Node reference in variable.References)
		{
			Node parent = reference.Parent;

			if (parent != null)
			{
				if (parent.GetNodeType() == NodeType.OPERATOR_NODE)
				{
					OperatorNode @operator = (OperatorNode)parent;

					// Try to resolve type via contextable right side of the assign operator
					if (@operator.Operator == Operators.ASSIGN &&
						@operator.Right is Contextable)
					{

						try
						{
							Contextable contextable = (Contextable)@operator.Right;
							Context context = contextable.GetContext();

							// Verify the type is resolved
							if (context != Types.UNKNOWN && context.IsType)
							{
								types.Add((Type)context);
							}
						}
						catch
						{
							continue;
						}
					}
				}
			}
		}

		Type shared = Resolver.GetSharedType(types);

		if (shared != Types.UNKNOWN)
		{
			variable.Type = shared;
			return;
		}

		throw new Exception($"Couldn't resolve type of variable '{variable.Name}'");
	}

	public static void ResolveVariables(Context context, List<Exception> errors)
	{
		foreach (Variable variable in context.Variables.Values)
		{
			if (variable.Type == Types.UNKNOWN)
			{
				try
				{
					ResolveVariables(variable);
				}
				catch (Exception e)
				{
					errors.Add(e);
				}
			}
		}
	}
}