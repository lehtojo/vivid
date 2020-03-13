using System.Text;

public static class Assembler 
{
    public const string BasePointer = "rbp";
    public const string StackPointer = "rsp";

    public static string Build(Function function)
    {
        var builder = new StringBuilder();

        foreach (var implementation in function.Implementations)
        {
            if (implementation.Node != null)
            {
                var unit = new Unit();
                
                Builders.Build(unit, implementation.Node);
                Oracle.Channel(unit);

                builder.Append(Translator.Translate(unit));
            }
            
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static string Build(Context context)
    {
        var builder = new StringBuilder();

        foreach (var function in context.Functions.Values)
        {
            foreach (var overload in function.Overloads)
            {
                if (!Flag.Has(overload.Modifiers, AccessModifier.EXTERNAL))
                {
                    builder.Append(Assembler.Build(overload));
                    builder.AppendLine();
                }
            }
        }

        return builder.ToString();
    }
}