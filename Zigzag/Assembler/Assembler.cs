using System.Text;
using System;

public static class Assembler 
{
    public const string BasePointer = "rbp";
    public const string StackPointer = "rsp";

    public static string Build(Function function)
    {
        var builder = new StringBuilder(function.GetFullname() + ":\n");

        foreach (var implementation in function.Implementations)
        {
            if (implementation.Node != null && !implementation.IsEmpty)
            {
                var unit = new Unit(implementation);

                if (function is Constructor constructor)
                {
                    Constructors.CreateHeader(unit, constructor.GetTypeParent() ?? throw new ApplicationException("Couldn't get constructor owner type"));
                }
                else if (function.IsMember)
                {
                    unit.Self = new Result(MemoryHandle.FromStack(unit, 8));
                }
                
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

        foreach (var type in context.Types.Values)
        {
            foreach (var overload in type.Constructors.Overloads)
            {
                builder.Append(Assembler.Build(overload));
                builder.AppendLine();
            }

            builder.Append(Build(type));
        }

        return builder.ToString();
    }
}