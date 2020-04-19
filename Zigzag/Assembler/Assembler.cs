using System.Text.RegularExpressions;
using System.Text;
using System;

public static class Assembler 
{
    public const string SEPARATOR = "\n\n";

    public static string Build(Function function)
    {
        var builder = new StringBuilder();

        foreach (var implementation in function.Implementations)
        {
            if (implementation.Node != null && !implementation.IsEmpty)
            {
                var unit = new Unit(implementation);

                unit.Execute(UnitMode.APPEND_MODE, () => 
                {
                    using (var scope = new Scope(unit))
                    {
                        unit.Append(new InitializeInstruction(unit));

                        if (function is Constructor constructor)
                        {
                            Constructors.CreateHeader(unit, constructor.GetTypeParent() ?? throw new ApplicationException("Couldn't get constructor owner type"));
                        }

                        Builders.Build(unit, implementation.Node);
                    }
                });

                Oracle.Channel(unit);

                var previous = 0;
                var current = unit.Instructions.Count;

                do
                {
                    previous = current;

                    Oracle.SimulateLifetimes(unit);

                    unit.Simulate(UnitMode.BUILD_MODE, instruction => 
                    {
                        instruction.TryBuild();
                    });

                    current = unit.Instructions.Count;
                }
                while (previous != current);

                builder.Append(Translator.Translate(unit));
                builder.AppendLine();
            }
        }

        if (builder.Length == 0)
        {
            return string.Empty;
        }

        return function.GetFullname() + ":\n" + builder.ToString();
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
                    builder.Append(SEPARATOR);
                }
            }
        }

        foreach (var type in context.Types.Values)
        {
            foreach (var overload in type.Constructors.Overloads)
            {
                builder.Append(Assembler.Build(overload));
                builder.Append(SEPARATOR);
            }

            builder.Append(Build(type));
        }

        return Regex.Replace(builder.ToString(), "\n{3,}", "\n\n");
    }
}