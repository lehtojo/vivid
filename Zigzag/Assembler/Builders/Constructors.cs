public static class Constructors
{
    public const string FUNCTION_ALLOCATE = "allocate";

    public static void CreateHeader(Unit unit, Type type)
    {
        unit.Self = Calls.Build(unit, FUNCTION_ALLOCATE, new NumberNode(NumberType.INT32, type.Size));
    }
}