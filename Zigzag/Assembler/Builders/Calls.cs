public static class Calls
{
    public static Quantum<Handle> Build(Unit unit, FunctionNode node)
    {
        var parameter = node.Parameters;

        while (parameter != null)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PassParameterInstruction(handle));
            
            parameter = parameter.Next;
        }

        return new CallInstruction(node.Function.Metadata.GetFullname()).Execute(unit);
    }
}