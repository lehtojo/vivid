using System.Collections.Generic;
using System.Linq;
using System;

public static class Translator
{
	private static List<Register> GetAllUsedNonVolatileRegisters(Unit unit)
	{
		return unit.Instructions.SelectMany(i => i.Parameters).Where(p => p.IsRegister && !p.Value!.To<RegisterHandle>().Register.IsVolatile).Select(p => p.Value!.To<RegisterHandle>().Register).Distinct().ToList();
	}

	private static IEnumerable<Handle> GetAllHandles(Unit unit)
	{
		return unit.Instructions.SelectMany(i => i.Parameters.Select(p => p.Value ?? throw new ApplicationException("Instruction parameter was not assigned")));
	}

	private static List<Variable> GetAllSavedLocalVariables(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(h => h is StackVariableHandle v && v.Variable.IsPredictable && v.Variable.LocalAlignment == null)
			.Select(h => h.To<StackVariableHandle>().Variable)
			.Distinct()
			.ToList();
	}

	private static List<TemporaryMemoryHandle> GetAllTemporaryMemoryHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(h => h is TemporaryMemoryHandle)
			.Select(h => h.To<TemporaryMemoryHandle>())
			.ToList();
	}

	private static List<ConstantDataSectionHandle> GetAllConstantDataSectionHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(h => h is ConstantDataSectionHandle)
			.Select(h => h.To<ConstantDataSectionHandle>())
			.ToList();
	}

	private static void AllocateConstantDataHandles(Unit unit, List<ConstantDataSectionHandle> constant_data_section_handles)
	{
		while (constant_data_section_handles.Count > 0)
		{
			var current = constant_data_section_handles.First();
			var copies = constant_data_section_handles.Where(c => c.Equals(current)).ToList();

			var identifier = unit.GetNextConstantIdentifier(current.Value);
			copies.ForEach(c => c.Identifier = identifier);
			copies.ForEach(c => constant_data_section_handles.Remove(c));
		}
	}

	public static string Translate(Unit unit, out List<ConstantDataSectionHandle> constants)
	{
		var registers = GetAllUsedNonVolatileRegisters(unit);
		var local_variables = GetAllSavedLocalVariables(unit);
		var temporary_handles = GetAllTemporaryMemoryHandles(unit);
		constants = GetAllConstantDataSectionHandles(unit).ToList();

		var required_local_memory = local_variables.Sum(i => i.Type!.ReferenceSize) + temporary_handles.Sum(i => i.Size.Bytes);
		var local_memory_top = 0;

		unit.Execute(UnitPhase.BUILD_MODE, () =>
		{
			if (unit.Instructions.Last().Type != InstructionType.RETURN)
			{
				unit.Append(new ReturnInstruction(unit, null, Types.UNKNOWN));
			}
		});

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (!(i is InitializeInstruction instruction)) return;

			instruction.Build(registers, required_local_memory);
			local_memory_top = instruction.LocalMemoryTop;
		});

		registers.Reverse();

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (i is ReturnInstruction instruction)
			{
				instruction.Build(registers, local_memory_top);
			}
		});

		// Align all used local variables
		Aligner.AlignLocalMemory(local_variables, temporary_handles.ToList(), local_memory_top);

		AllocateConstantDataHandles(unit, new List<ConstantDataSectionHandle>(constants));

		unit.Simulate(UnitPhase.BUILD_MODE, instruction =>
		{
			instruction.Translate();
		});

		// Remove duplicates
		constants = constants.Distinct().ToList();

		return unit.Export();
	}
}