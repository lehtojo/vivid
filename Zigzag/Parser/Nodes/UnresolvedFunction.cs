using System;
using System.Collections.Generic;

public class UnresolvedFunction : Node, IResolvable
{
	public string Name { get; private set; }
	public Node Parameters => First;


	/// <summary>
	/// Creates an unresolved function with a function name to look for
	/// </summary>
	/// <param name="name">Function name</param>
	public UnresolvedFunction(string name)
	{
		Name = name;
	}

	public UnresolvedFunction SetParameters(Node parameters)
	{
		var parameter = parameters.First;

		while (parameter != null)
		{
			var next = parameter.Next;
			Add(parameter);
			parameter = next;
		}

		return this;
	}

	public Node Solve(Context environment, Context context)
	{
		if (Parameters != null)
		{
			var parameter = First;

			while (parameter != null)
			{
				var resolved = Resolver.Resolve(environment, parameter, new List<string>());

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

		// Get parameter types
		var types = Resolver.GetTypes(this);

		// Parameter types must be known
		if (types == null)
		{
			throw new Exception($"Couldn't resolve function parameters '{Name}'");
		}

		// Try to find a suitable function by name and parameter types
		var function = Singleton.GetFunctionByName(context, Name, types);

		if (function == null)
		{
			throw new Exception($"Couldn't resolve function or constructor '{Name}'");
		}

		var node = new FunctionNode(function).SetParameters(this);

		if (function.Metadata is Constructor)
		{
			return new ConstructionNode(node);
		}

		return node;
	}

	public Node Resolve(Context context)
	{
		return Solve(context, context);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.UNRESOLVED_FUNCTION;
	}

	public Status GetStatus()
	{
		return Status.Error($"Couldn't resolve function or constructor '{Name}'");
	}
}