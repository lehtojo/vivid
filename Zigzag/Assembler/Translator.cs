using System.Collections.Generic;
using System.Linq;
using System;

public static class Translator
{
    private static List<Register> GetAllUsedNonVolatileRegisters(Unit unit)
    {
        return unit.NonVolatileRegisters.Where(r => r.IsUsed).ToList();
    }

    public static string Translate(Unit unit)
    {
        var registers = GetAllUsedNonVolatileRegisters(unit);

        unit.Execute(UnitMode.BUILD_MODE, () => 
        {
            if (unit.Instructions.Last().Type != InstructionType.RETURN)
            {
                unit.Append(new ReturnInstruction(unit, null));
            }
        });

        unit.Simulate(UnitMode.READ_ONLY_MODE, i =>
        {
            if (i is InitializeInstruction instruction)
            {
                instruction.Build(registers, 0);
            }
        });

        registers.Reverse();

        unit.Simulate(UnitMode.READ_ONLY_MODE, i =>
        {
            if (i is ReturnInstruction instruction)
            {
                instruction.Build(registers, 0);
            }
        });

        unit.Simulate(UnitMode.BUILD_MODE, instruction => 
        {
            instruction.Translate();
        });

        var metadata = unit.Function.Metadata!;

        if (metadata.IsConstructor)
        {
            Console.WriteLine("Constructor: " + metadata.GetTypeParent()!.Name);
        }
        else 
        {
            Console.WriteLine("Function: " + metadata.Name);
        }

        if (registers.Count == 0)
        {
            Console.WriteLine("Non-volatile registers were not used");
        }
        else
        {
            registers.ForEach(r => Console.WriteLine(r.Name));
        }

        Console.WriteLine("\n");

        return unit.Export();
    }
}