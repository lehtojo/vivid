using System;

public class UnresolvedFunction : Node, IResolvable
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
        var types = Resolver.GetTypes(this);

        // Parameter types must be known
        if (types == null)
        {
            return null;
        }

        // Try to find a suitable function by name and parameter types
        var function = Singleton.GetFunctionByName(context, Name, types);

        if (function == null)
        {
            return null;
        }

        var node = new FunctionNode(function).SetParameters(this);

        if (function.Metadata is Constructor)
        {
            return new ConstructionNode(node);
        }

        return node;
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
        return Status.Error($"Couldn't resolve function or constructor '{Name}'");
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