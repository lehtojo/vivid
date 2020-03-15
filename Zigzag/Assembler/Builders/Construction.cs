public static class Construction
{
    public static Result Build(Unit unit, ConstructionNode node)
    {
        return Calls.Build(unit, node.Parameters, node.GetConstructor()!);
    }
}