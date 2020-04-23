using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

public static class Assembler 
{
    private const string AllocateFunctionIdentifier = "allocate";

    public static Function? AllocationFunction { get; private set; }
    public static Size Size { get; private set; } = Size.QWORD;
    
    public const string TEXT_SECTION = "section .text";
    public const string TEXT_SECTION_HEADER =   "global _start" + "\n" +
                                                "_start:" + "\n" +
                                                "call function_run" + "\n" +
                                                "mov rax, 60" + "\n" +
                                                "xor rdi, rdi" + "\n" +
                                                "syscall" + SEPARATOR;   
    public const string DATA_SECTION = "section .data";
    public const string SEPARATOR = "\n\n";

    private static string GetExternalFunctions(Context context)
    {
        var builder = new StringBuilder();

        foreach (var function in context.Functions.Values)
        {
            foreach (var overload in function.Overloads)
            {
                if (overload.IsExternal)
                {
                    builder.AppendLine($"extern {overload.GetFullname()}");
                }
            }
        }

        builder.Append(SEPARATOR);

        return builder.ToString();
    }

    private static string GetText(Function function)
    {
        var builder = new StringBuilder();

        foreach (var implementation in function.Implementations)
        {
            if (implementation.Node != null && !implementation.IsEmpty)
            {
                var unit = new Unit(implementation);

                unit.Execute(UnitPhase.APPEND_MODE, () => 
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

                    unit.Simulate(UnitPhase.BUILD_MODE, instruction => 
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

    private static string GetText(Context context)
    {
        var builder = new StringBuilder();

        foreach (var function in context.Functions.Values)
        {
            foreach (var overload in function.Overloads)
            {
                if (!Flag.Has(overload.Modifiers, AccessModifier.EXTERNAL))
                {
                    builder.Append(Assembler.GetText(overload));
                    builder.Append(SEPARATOR);
                }
            }
        }

        foreach (var type in context.Types.Values)
        {
            foreach (var overload in type.Constructors.Overloads)
            {
                builder.Append(Assembler.GetText(overload));
                builder.Append(SEPARATOR);
            }

            builder.Append(GetText(type));
        }

        return Regex.Replace(builder.ToString(), "\n{3,}", "\n\n");
    }

    private static string GetStaticVariables(Type type)
    {
        var builder = new StringBuilder();
        
        foreach (var variable in type.Variables.Values)
        {
            if (variable.IsStatic)
            {
                var name = variable.GetStaticName();
                var allocator = Size.FromBytes(variable.Type!.ReferenceSize).Allocator;

                builder.AppendLine($"{name} {allocator} 0");
            }
        }

        foreach (var subtype in type.Supertypes)
        {
            builder.Append(SEPARATOR);
            builder.AppendLine(GetStaticVariables(subtype));
        }

        return builder.ToString();
    }

    private static IEnumerable<StringNode> GetFunctionStringNodes(FunctionImplementation implementation)
    {
        return implementation.Node?.FindAll(n => n is StringNode).Select(n => (StringNode)n) ?? new List<StringNode>();
    }

    public static IEnumerable<StringNode> GetStringNodes(Context context)
    {
        var nodes = new List<StringNode>();

        foreach (var implementation in context.GetImplementedFunctions())
        {
            nodes.AddRange(GetFunctionStringNodes(implementation));
        }

        foreach (var type in context.Types.Values)
        {
            nodes.AddRange(GetStringNodes(type));
        }

        return nodes;
    }

    private static string GetData(Context context)
    {
        var builder = new StringBuilder();

        foreach (var type in context.Types.Values)
        {
            builder.AppendLine(GetStaticVariables(type));
            builder.Append(SEPARATOR);
        }

        var nodes = GetStringNodes(context);

        foreach (var node in nodes)
        {
            var name = node.GetIdentifier(null);
            var allocator = Size.BYTE.Allocator;
            var text = node.Text;

            builder.AppendLine($"{name} {allocator} '{text}', 0");
        }

        return builder.ToString();
    }

    public static string Assemble(Context context, int bits)
    {
        Size = Size.FromBytes(bits / 8);
        AllocationFunction = context.GetFunction(AllocateFunctionIdentifier)?.Overloads[0] ?? throw new ApplicationException("Allocation function was missing");
        
        var builder = new StringBuilder();

        builder.AppendLine(TEXT_SECTION);
        builder.AppendLine(TEXT_SECTION_HEADER);
        builder.Append(GetExternalFunctions(context));
        builder.Append(GetText(context));
        builder.Append(SEPARATOR);

        builder.AppendLine(DATA_SECTION);
        builder.Append(GetData(context));
        builder.Append(SEPARATOR);

        return Regex.Replace(builder.ToString(), "\n{3,}", "\n\n");
    }
}