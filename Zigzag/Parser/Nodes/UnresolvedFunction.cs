using System;
using System.Collections.Generic;
using System.Linq;

public class UnresolvedFunction : Node, IResolvable, IType
{
    public string Name { get; }


    /// <summary>
    /// Creates an unresolved function with a function name to look for
    /// </summary>
    /// <param name="name">Function name</param>
    public UnresolvedFunction(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Transfers the specified parameters to this unresolved function
    /// </summary>
    public UnresolvedFunction SetParameters(Node parameters)
    {
        foreach (var parameter in parameters)
        {
            Add(parameter);
        }

        return this;
    }

    /// <summary>
    /// Returns all parameters as pairs containing the parameter type and the node
    /// </summary>
    private List<(Type Type, Node Node)>? GetParameterTypes()
	{
		var types = new List<(Type, Node)>();

        foreach (var parameter in this)
        {
            var type = parameter.TryGetType();

            if (type == Types.UNKNOWN)
            {
                return null;
            }

            types.Add((type, parameter));
        }

		return types;
	}

    /// <summary>
    /// Tries to resolve any typeless short functions inside the parameters of this unresolved function
    /// </summary>
    private void TryResolveShortFunctionParameters(Context environment, List<(Type Type, Node Node)> parameters, List<Type> types)
    {
        // Collect all the parameters which are unresolved
        var unresolved = parameters.Where(p => p.Type.IsUnresolved).ToArray();

        // Ensure all the unresolved parameter types represent lambda types
        if (!unresolved.All(p => p.Type is LambdaType && p.Node.Is(NodeType.LAMBDA_NODE)) ||
            !environment.IsFunctionDeclared(Name))
        {
            return;
        }

        // Find all the functions overloads with the name of this unresolved function
        var functions = environment.GetFunction(Name)!;

        // Find all the function overloads that could accept the currently resolved parameter types
        var candidates = functions.Overloads.Where(f => f.Parameters
            .Select(p => p.Type)
            .Zip(types)
            .All(i => i.First == null || i.Second.IsUnresolved || Resolver.GetSharedType(i.First, i.Second) != null)

        ).ToList();

        Type?[] expected_types;

        // Filter out all candidates where the type of the parameter matching the unresolved lambda type is not a lambda type
        for (var c = candidates.Count - 1; c >= 0; c--)
        {
            var candidate = candidates[c];
            expected_types = candidate.Parameters.Select(p => p.Type).ToArray();

            for (var i = 0; i < expected_types.Length; i++)
            {
                var expected = expected_types[i];
                var actual = types[i];

                // Skip all parameter types which don't represent lambda types
                if (expected == null || !(actual is LambdaType))
                {
                    continue;
                }

                if (!(expected is LambdaType))
                {
                    // Since the actual parameter type is lambda type and the expected is not, the current candidate can be removed
                    candidates.RemoveAt(c);
                    break;
                }
            }
        }

        // Resolve the lambda type only if there's only one option left since the analysis would go too complex
        if (candidates.Count != 1)
        {
            return;
        }

        var match = candidates.First();
        expected_types = match.Parameters.Select(p => p.Type).ToArray();

        for (var i = 0; i < expected_types.Length; i++)
        {
            // Skip all parameter types which don't represent lambda types
            if (!(expected_types[i] is LambdaType expected))
            {
                continue;
            }

            var actual = (LambdaType)types[i];

            // Ensure the parameter types don't conflict
            if (expected.Parameters.Count != actual.Parameters.Count ||
                !expected.Parameters.Zip(actual.Parameters).All(i => i.Second == null || i.Second == i.First))
            {
                return;
            }

            // Since none of the parameters conflicted with the expected parameters types, the expected parameter types can be transfered
            for (var j = 0; j < expected.Parameters.Count; j++)
            {
                parameters[i].Node.To<LambdaNode>().Lambda.Parameters[j].Type = expected.Parameters[j];
            }
        }
    }

    public Node? Solve(Context environment, Context context)
    {
        // Try to solve all the parameters
        var parameter = First;

        while (parameter != null)
        {
            Resolver.Resolve(environment, parameter);
            parameter = parameter.Next;
        }

        // Get parameter types
        var parameters = GetParameterTypes();

        // Parameter types must be known
        if (parameters == null)
        {
            return null;
        }

        var types = parameters.Select(p => p.Type).ToList();
        
        if (types.Any(t => t.IsUnresolved))
        {
            TryResolveShortFunctionParameters(environment, parameters, types);
            return null;
        }

        // Try to find a suitable function by name and parameter types
        var function = Singleton.GetFunctionByName(context, Name, types);
        	
        if (function == null)
        {
            if (context.IsVariableDeclared(Name))
		    {
			    var variable = context.GetVariable(Name)!;

			    if (variable.Type is LambdaType)
			    {
                    var call = new LambdaCallNode(this);

				    return environment == context
					    ? (Node)new LinkNode(new VariableNode(variable), call)
					    : (Node)call;
			    }
		    }

            return null;
        }

        var node = new FunctionNode(function).SetParameters(this);

        if (function.Metadata is Constructor)
        {
            return new ConstructionNode(node);
        }

        return node;
    }

    public new Type? GetType()
    {
        return Types.UNKNOWN;
    }

    public Node? Resolve(Context context)
    {
        return Solve(context, context);
    }

    public override NodeType GetNodeType()
    {
        return NodeType.UNRESOLVED_FUNCTION;
    }

    public Status GetStatus()
    {
        return Status.Error($"Could not resolve function or constructor '{Name + $"({string.Join(", ", ((IEnumerable<Node>)this).Select(p => p.TryGetType()?.ToString() ?? "_"))})"}'");
    }

    public override bool Equals(object? obj)
    {
        return obj is UnresolvedFunction function &&
               base.Equals(obj) &&
               Name == function.Name;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Name);
        return hash.ToHashCode();
    }
}