using System.Collections.Generic;
using System.Linq;
using System;

public static class Translator
{
	private static List<Register> GetAllUsedNonVolatileRegisters(Unit unit)
	{
		return unit.Instructions.SelectMany(i => i.Parameters).Where(p => p.IsAnyRegister && !p.Value!.To<RegisterHandle>().Register.IsVolatile).Select(p => p.Value!.To<RegisterHandle>().Register).Distinct().ToList();
	}

	private static IEnumerable<Handle> GetAllHandles(Result[] results)
	{
		return results.Select(i => i.Value).Concat(results.SelectMany(i => GetAllHandles(i.Value.GetInnerResults())));
	}

	private static IEnumerable<Handle> GetAllHandles(Unit unit)
	{
		var handles = unit.Instructions.SelectMany(i => i.Parameters.Select(p => p.Value ?? throw new ApplicationException("Instruction parameter was not assigned")));
		
		return handles.Concat(handles.SelectMany(i => GetAllHandles(i.GetInnerResults())));
	}

	private static List<Variable> GetAllSavedLocalVariables(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(i => i.Is(HandleInstanceType.STACK_VARIABLE))
			.Cast<StackVariableHandle>()
			.Where(i => i.Variable.IsPredictable && i.Variable.LocalAlignment == null)
			.Select(i => i.Variable)
			.Distinct()
			.ToList();
	}

	private static List<TemporaryMemoryHandle> GetAllTemporaryMemoryHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(i => i.Is(HandleInstanceType.TEMPORARY_MEMORY))
			.Cast<TemporaryMemoryHandle>()
			.ToList();
	}

	private static List<InlineHandle> GetAllInlineHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(i => i.Is(HandleInstanceType.INLINE))
			.Cast<InlineHandle>()
			.ToList();
	}

	private static List<ConstantDataSectionHandle> GetAllConstantDataSectionHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(i => i.Is(HandleInstanceType.CONSTANT_DATA_SECTION))
			.Cast<ConstantDataSectionHandle>()
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

	public static string Translate(Unit unit, List<ConstantDataSectionHandle> constants)
	{
		// If optimization is enabled, try to optimize the generated code on instruction level
		if (Analysis.IsInstructionAnalysisEnabled)
		{
			InstructionAnalysis.Optimize(unit);
		}
		
		var registers = GetAllUsedNonVolatileRegisters(unit);
		var local_variables = GetAllSavedLocalVariables(unit);
		var temporary_handles = GetAllTemporaryMemoryHandles(unit);
		var inline_handles = GetAllInlineHandles(unit);
		var constant_handles = GetAllConstantDataSectionHandles(unit);

		// When debugging mode is enabled, the base pointer is reserved for saving the value of the stack pointer in the start
		if (Assembler.IsDebuggingEnabled)
		{
			registers.Add(unit.GetBasePointer());
		}

		// Determine how much additional memory must be allocated at the start based on the generated code
		var required_local_memory = local_variables.Sum(i => i.Type!.ReferenceSize) + temporary_handles.Sum(i => i.Size.Bytes) + inline_handles.Distinct().Sum(i => i.Bytes);
		var local_memory_top = 0;

		unit.Execute(UnitPhase.BUILD_MODE, () =>
		{
			// Append a return instruction at the end if there's no return instruction present
			if (!unit.Instructions.Last().Is(InstructionType.RETURN))
			{
				unit.Append(new ReturnInstruction(unit, null, Types.UNKNOWN));
			}

			// If debug information is being generated, append a debug information label at the end
			if (Assembler.IsDebuggingEnabled)
			{
				unit.Append(new LabelInstruction(unit, new Label(Debug.GetEnd(unit.Function).Name)));
			}
		});

		// Build all initialization instructions
		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (!i.Is(InstructionType.INITIALIZE)) return;

			var initialization = i.To<InitializeInstruction>();

			initialization.Build(registers, required_local_memory);
			local_memory_top = initialization.LocalMemoryTop;
		});
		
		// Reverse the saved registers since they must be recovered from stack when returning from the function so they must be in the reversed order
		registers.Reverse();

		// Build all return instructions
		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (!i.Is(InstructionType.RETURN)) return;

			i.To<ReturnInstruction>().Build(registers, local_memory_top);
		});

		// Align all used local variables
		Aligner.AlignLocalMemory(local_variables, temporary_handles.ToList(), inline_handles, local_memory_top);

		AllocateConstantDataHandles(unit, new List<ConstantDataSectionHandle>(constant_handles));

		// Translate all instructions
		unit.Simulate(UnitPhase.BUILD_MODE, instruction =>
		{
			instruction.Translate();
		});

		// Remove duplicates
		constants.AddRange(constant_handles.Distinct());

		return unit.Export();
	}
}