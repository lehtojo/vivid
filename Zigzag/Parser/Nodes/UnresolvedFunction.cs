using System.Collections.Generic;
using System;

public class UnresolvedFunction : Node, Resolvable
{
	public string Value { get; private set; }
	public Node Parameters => First;

	public UnresolvedFunction(string value)
	{
		Value = value;
	}

	public UnresolvedFunction SetParameters(Node parameters)
	{
		Node parameter = parameters.First;

		while (parameter != null)
		{
			Node next = parameter.Next;
			Add(parameter);
			parameter = next;
		}

		return this;
	}

	public Node Solve(Context environment, Context context)
	{
		Node node = Parameters;

		if (node != null)
		{
			Node parameter = First;

			while (parameter != null)
			{
				Node resolved = Resolver.Resolve(environment, parameter, new List<Exception>());

				if (resolved != null)
				{
					parameter.Replace(resolved);
					parameter = resolved.Next;
				}
				else
				{
					parameter = parameter.Next;
				}
			}
		}

		List<Type> parameters = Resolver.GetTypes(this);

		if (parameters == null)
		{
			throw new Exception($"Couldn't resolve function parameters '{Value}'");
		}

		Function function = Singleton.GetFunctionByName(context, Value, parameters);

		if (function == null)
		{
			throw new Exception($"Couldn't resolve function '{Value}'");
		}

		return new FunctionNode(function).SetParameters(this);
	}

	public Node Resolve(Context context)
	{
		return Solve(context, context);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.UNRESOLVED_FUNCTION;
	}
}