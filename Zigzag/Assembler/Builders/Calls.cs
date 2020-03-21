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

    private static bool IsSelfCall(FunctionImplementation current, FunctionImplementation other)
    {
        return current.IsMember && other.IsMember && current.GetTypeParent() == other.GetTypeParent();
    }

    public static Result Build(Unit unit, Result? self, Node? parameters, FunctionImplementation implementation)
    {
        var metadata = implementation.Metadata!;
        var parameter = parameters?.Last;

        while (parameter != null)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PassParameterInstruction(unit, handle));
            
            parameter = parameter.Previous;
        }

        if (self == null && IsSelfCall(unit.Function, implementation))
        {
            self = new GetSelfPointerInstruction(unit).Execute();
        }

        if (self != null)
        {
            unit.Append(new PassParameterInstruction(unit, self));
        }

        return new CallInstruction(unit, metadata.GetFullname()).Execute();
    }

    public static Result Build(Unit unit, string function, params Node[] parameters)
    {
        for (int i = parameters.Length - 1; i >= 0 ; i--)
        {
            var handle = References.Get(unit, parameters[i]);
            unit.Append(new PassParameterInstruction(unit, handle));
        }
        
        return new CallInstruction(unit, function).Execute();
    }
}