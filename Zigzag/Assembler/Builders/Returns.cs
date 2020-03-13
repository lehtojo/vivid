public static class Returns
{
    public static Quantum<Handle> Build(Unit unit, ReturnNode node)
    {
        return new ReturnInstruction(References.Get(unit, node.First)).Execute(unit);
    }
}