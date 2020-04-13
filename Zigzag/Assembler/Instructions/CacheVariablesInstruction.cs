using System;
using System.Linq;

using System.Collections.Generic;

public static class ListPopExtensionClasses
{
    public static T? Pop<T>(this List<T> source) where T : class
    {
        if (source.Count == 0)
        {
            return null;
        }

        var element = source[0];
        source.RemoveAt(0);

        return element;
    }
}

public static class ListPopExtensionStructs
{
    public static T? Pop<T>(this List<T> source) where T : struct
    {
        if (source.Count == 0)
        {
            return null;
        }

        var element = source[0];
        source.RemoveAt(0);

        return element;
    }
}

public class CacheVariablesInstruction : Instruction
{
    private List<VariableUsageInfo> Usages { get; set; }

    public CacheVariablesInstruction(Unit unit, List<VariableUsageInfo> variables) : base(unit)
    {
        Usages = variables;

        foreach (var usage in Usages)
        {
            usage.Reference = References.GetVariable(unit, usage.Variable, AccessMode.READ);
        }
    }

    public override void Build()
    {
        var usages = new List<VariableUsageInfo>(Usages);

        var available = new List<Register>(Unit.NonReservedRegisters);
        var removed = new List<(VariableUsageInfo Info, Register Register)>();

        foreach (var usage in Usages)
        {
            // Try to find a register that contains the current variable
            var register = available.Find(r => r.Handle?.Metadata.Equals(usage.Variable) ?? false);

            if (register != null)
            {  
                usages.Remove(usage);
                available.Remove(register);

                removed.Add((usage, register));
            }
        }

        // Sort the variables based on their number of usages (most used variables first)
        removed.Sort((a, b) => -a.Info.Usages.CompareTo(b.Info.Usages));

        foreach (var usage in usages)
        {
            // Try to get an available register
            var register = available.Pop();

            if (register == null)
            {
                if (removed.Count == 0)
                {
                    // There are no available registers anymore
                    break;
                }

                var next = removed.First();

                // The current variable is only allowed to take over the used register if it will be more used
                if (next.Info.Usages >= usage.Usages)
                {
                    continue;
                }

                register = next.Register;
            }

            // Clear the register safely if it holds something
            Unit.Release(register);

            var destination = new Result(new RegisterHandle(register));
            var source = usage.Reference!;

            var move = new MoveInstruction(Unit, destination, source);
            move.Mode = MoveMode.RELOCATE;

            Unit.Append(move);
        }
    }

    public override Result? GetDestinationDependency()
    {
        Console.WriteLine("Warning: Cache-Variables-Instruction cannot be redirected");
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.CACHE_VARIABLES;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}