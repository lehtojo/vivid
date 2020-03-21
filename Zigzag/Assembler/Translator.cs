public static class Translator
{
    public static string Translate(Unit unit)
    {
        unit.Simulate(UnitMode.BUILD_MODE, instruction => 
        {
            instruction.Build();
        });

        return unit.Export();
    }
}