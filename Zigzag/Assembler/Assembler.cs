using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System;

public static class Assembler
{
    private const string ALLOCATE_FUNCTION_IDENTIFIER = "allocate";

    public static Function? AllocationFunction { get; private set; }
    public static Size Size { get; set; } = Size.QWORD;
    public static Format Format => Size.ToFormat();
    public static OSPlatform Target { get; } = OSPlatform.Windows;
    public static bool IsTargetWindows => Target == OSPlatform.Windows;
    public static bool IsTargetLinux => Target == OSPlatform.Linux;
    public static bool IsTargetX86 => Size.Bits == 32;
    public static bool IsTargetX64 => Size.Bits == 64;

    private const string TEXT_SECTION = "section .text";

    private const string LINUX_TEXT_SECTION_HEADER = "global _start" + "\n" +
                                                     "_start:" + "\n" +
                                                     "call function_run" + "\n" +
                                                     "mov rax, 60" + "\n" +
                                                     "xor rdi, rdi" + "\n" +
                                                     "syscall" + SEPARATOR;

    private const string WINDOWS_TEXT_SECTION_HEADER = "global function_run";

    private const string DATA_SECTION = "section .data";
    private const string SEPARATOR = "\n\n";

    private static string GetExternalFunctions(Context context)
    {
        var builder = new StringBuilder();

        foreach (var overload in from function in context.Functions.Values from overload in function.Overloads where overload.IsImported select overload)
        {
            builder.AppendLine($"extern {overload.GetFullname()}");
        }

        builder.Append(SEPARATOR);

        return builder.ToString();
    }

    private static string GetText(Function function, out List<ConstantDataSectionHandle> out_constants)
    {
        var builder = new StringBuilder();
        var constants = new List<ConstantDataSectionHandle>();

        foreach (var implementation in function.Implementations.Where(implementation => implementation.Node != null && !implementation.IsEmpty))
        {
            // Export this function if necessary
            if (function.IsExported)
            {
                builder.AppendLine($"global {function.GetFullname()}");

                if (IsTargetWindows)
                {
                    builder.AppendLine($"export {function.GetFullname()}");
                }
            }

            // Append the function label
            builder.AppendLine(function.GetFullname() + ':');

            var unit = new Unit(implementation);

            unit.Execute(UnitPhase.APPEND_MODE, () =>
            {
                // Create the most outer scope where all instructions will be placed
                using var scope = new Scope(unit);

                // Initialize this function
                unit.Append(new InitializeInstruction(unit));

                // Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
                var variables = unit.Function.Parameters;

                if (unit.Function.Metadata!.IsMember && !unit.Function.Metadata!.IsConstructor)
                {
                    variables.Add(unit.Function.GetVariable(Function.THIS_POINTER_IDENTIFIER) ??
                                  throw new ApplicationException("This pointer was missing in member function"));
                }

                unit.Append(new RequireVariablesInstruction(unit, variables));

                if (function is Constructor constructor)
                {
                    Constructors.CreateHeader(unit,
                        constructor.GetTypeParent() ??
                        throw new ApplicationException("Couldn't get constructor owner type"));
                }

                Builders.Build(unit, implementation.Node!);
            });

            // Sprinkle a little intelligence into the output code
            Oracle.Channel(unit);

            var previous = 0;
            var current = unit.Instructions.Count;

            do
            {
                previous = current;

                Oracle.SimulateLifetimes(unit);

                unit.Simulate(UnitPhase.BUILD_MODE, instruction => { instruction.Build(); });

                current = unit.Instructions.Count;
                    
            } while (previous != current);

            builder.Append(Translator.Translate(unit, out List<ConstantDataSectionHandle> constant_handles));
            builder.AppendLine();

            constants.AddRange(constant_handles);
        }

        out_constants = constants;

        return builder.Length == 0 ? string.Empty : builder.ToString();
    }

    private static string GetText(Context context, out List<ConstantDataSectionHandle> constants)
    {
        var builder = new StringBuilder();

        constants = new List<ConstantDataSectionHandle>();

        foreach (var overload in from function in context.Functions.Values from overload in function.Overloads where !overload.IsImported select overload)
        {
            builder.Append(GetText(overload, out List<ConstantDataSectionHandle> function_constants));
            builder.Append(SEPARATOR);

            constants.AddRange(function_constants);
        }

        foreach (var type in context.Types.Values)
        {
            foreach (var overload in type.Constructors.Overloads)
            {
                builder.Append(GetText(overload, out List<ConstantDataSectionHandle> constructor_constants));
                builder.Append(SEPARATOR);

                constants.AddRange(constructor_constants);
            }

            builder.Append(GetText(type, out List<ConstantDataSectionHandle> type_constants));

            constants.AddRange(type_constants);
        }

        return Regex.Replace(builder.ToString().Replace("\r\n", "\n"), "\n{3,}", "\n\n");
    }

    private static string GetStaticVariables(Type type)
    {
        var builder = new StringBuilder();

        foreach (var variable in type.Variables.Values)
        {
            if (!variable.IsStatic) continue;
            
            var name = variable.GetStaticName();
            var allocator = Size.FromBytes(variable.Type!.ReferenceSize).Allocator;

            builder.AppendLine($"{name} {allocator} 0");
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
        return implementation.Node?.FindAll(n => n is StringNode).Select(n => (StringNode) n) ?? new List<StringNode>();
    }

    private static IEnumerable<StringNode> GetStringNodes(Context context)
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

    private static string GetConstantData(List<ConstantDataSectionHandle> constants)
    {
        var builder = new StringBuilder();

        foreach (var constant in constants)
        {
            var name = constant.Identifier;
            var allocator = constant.Size.Allocator;
            var text = constant.Value.ToString()!.Replace(',', '.');

            // Add decimal part to the value if necessary
            if (constant.Format.IsDecimal() && !text.Contains('.'))
            {
                text += ".0";
            }

            builder.AppendLine($"{name} {allocator} {text}");
        }

        return builder.ToString();
    }

    public static string Assemble(Context context)
    {
        AllocationFunction = context.GetFunction(ALLOCATE_FUNCTION_IDENTIFIER)?.Overloads[0] ??
                             throw new ApplicationException("Allocation function was missing");

        var builder = new StringBuilder();

        builder.AppendLine(TEXT_SECTION);
        builder.AppendLine(IsTargetWindows ? WINDOWS_TEXT_SECTION_HEADER : LINUX_TEXT_SECTION_HEADER);
        builder.Append(GetExternalFunctions(context));
        builder.Append(GetText(context, out List<ConstantDataSectionHandle> constants));
        builder.Append(SEPARATOR);

        builder.AppendLine(DATA_SECTION);
        builder.Append(GetData(context));
        builder.Append(GetConstantData(constants));
        builder.Append(SEPARATOR);

        return Regex.Replace(builder.Replace("\r\n", "\n").ToString(), "\n{3,}", "\n\n");
    }
}