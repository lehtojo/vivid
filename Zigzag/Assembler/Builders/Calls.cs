public static class Calls
{
    public static Result Build(Unit unit, FunctionNode node)
    {
        return Build(unit, null, node.Parameters, node.Function!);
    }

    public static Result Build(Unit unit, Result self, FunctionNode node)
    {
        return Build(unit, self, node.Parameters, node.Function!);
    }
    
    public static Result Build(Unit unit, Node? parameters, FunctionImplementation implementation)
    {
        return Build(unit, null, parameters, implementation);
    }

    public static Result Build(Unit unit, Result? self, Node? parameters, FunctionImplementation implementation)
    {
        var metadata = implementation.Metadata!;
        var parameter = parameters;

        if (self != null)
        {
            unit.Append(new PassParameterInstruction(unit, self));
        }

        while (parameter != null)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PassParameterInstruction(unit, handle));
            
            parameter = parameter.Next;
        }

        return new CallInstruction(unit, metadata.GetFullname()).Execute();
    }

    public static Result Build(Unit unit, string function, params Node[] parameters)
    {
        foreach (var parameter in parameters)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PassParameterInstruction(unit, handle));
        }
        
        return new CallInstruction(unit, function).Execute();
    }
}