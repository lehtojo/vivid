using System.Collections.Generic;
using System.Linq;
using System;

public static class Translator
{
	private static List<Register> GetAllUsedNonVolatileRegisters(Unit unit)
	{
		return unit.Instructions.SelectMany(i => i.Parameters).Where(p => p.IsRegister && !p.Value!.To<RegisterHandle>().Register.IsVolatile).Select(p => p.Value!.To<RegisterHandle>().Register).Distinct().ToList();
	}

	private static IEnumerable<Variable> GetAllSavedLocalVariables(Unit unit)
	{
		return unit.Instructions.SelectMany(i => i.Parameters.Select(p => p.Value ?? throw new ApplicationException("Instruction parameter was not assigned")))
				.Where(h => h is VariableMemoryHandle v && v.Variable.IsLocal).Select(h => ((VariableMemoryHandle)h).Variable).Distinct();
	}

	public static string Translate(Unit unit)
	{
		var registers = GetAllUsedNonVolatileRegisters(unit);
		var local_variables = GetAllSavedLocalVariables(unit);
		var required_local_memory = local_variables.Sum(v => v.Type!.ReferenceSize);
		var local_variables_top = 0;

		unit.Execute(UnitPhase.BUILD_MODE, () => 
		{
			if (unit.Instructions.Last().Type != InstructionType.RETURN)
			{
				unit.Append(new ReturnInstruction(unit, null, Types.UNKNOWN));
			}
		});

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (i is InitializeInstruction instruction)
			{
				instruction.Build(registers, required_local_memory);
				local_variables_top = instruction.LocalVariablesTop;
			}
		});

		registers.Reverse();

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (i is ReturnInstruction instruction)
			{
				instruction.Build(registers, local_variables_top);
			}
		});

		// Align all used local variables
		Aligner.AlignLocalVariables(local_variables, local_variables_top);

		unit.Simulate(UnitPhase.BUILD_MODE, instruction => 
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
			registers.ForEach(r => Console.WriteLine(r[Assembler.Size]));
		}

		Console.WriteLine("\n");

		return unit.Export();
	}
}