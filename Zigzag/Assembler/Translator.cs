public static class Translator
{
    public static string Translate(Unit unit)
    {
        unit.Simulate(instruction => 
        {
            instruction.Build();
        });

        return unit.Export();
    }
}