public static class Calls
{
    public static Result Build(Unit unit, FunctionNode node)
    {
        var parameter = (Node?)node.Parameters;

        while (parameter != null)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PassParameterInstruction(unit, handle));
            
            parameter = parameter.Next;
        }

        return new CallInstruction(unit, node.Function.Metadata!.GetFullname()).Execute();
    }
}