using System;
using System.Collections.Generic;
using System.Linq;

public static class Translator
{
	public static Dictionary<SourceFile, List<Instruction>> Output { get; set; } = new Dictionary<SourceFile, List<Instruction>>();
	public static int TotalInstructions { get; set; } = 0;

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
			.Distinct(new ReferenceEqualityComparer<Variable>())
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
			var copies = constant_data_section_handles.Where(i => i.Equals(current)).ToList();

			var identifier = unit.GetNextConstantIdentifier(current.Value);
			copies.ForEach(i => i.Identifier = identifier);
			copies.ForEach(i => constant_data_section_handles.Remove(i));
		}
	}

	public static string Translate(Unit unit, List<ConstantDataSectionHandle> constants)
	{
		TotalInstructions += unit.Instructions.Count;
		
		// Take only the instructions which are actual assembly instructions
		var instructions = unit.Instructions.Where(i => !i.IsAbstract).ToList();

		// If optimization is enabled, try to optimize the generated code on instruction level
		if (Analysis.IsInstructionAnalysisEnabled)
		{
			InstructionAnalysis.Optimize(unit, instructions);
		}

		var registers = GetAllUsedNonVolatileRegisters(unit);
		var local_variables = GetAllSavedLocalVariables(unit);
		var temporary_handles = GetAllTemporaryMemoryHandles(unit);
		var inline_handles = GetAllInlineHandles(unit);
		var constant_handles = GetAllConstantDataSectionHandles(unit);

		// Determine how much additional memory must be allocated at the start based on the generated code
		var required_local_memory = local_variables.Sum(i => i.Type!.AllocationSize) + temporary_handles.Sum(i => i.Size.Bytes) + inline_handles.Distinct().Sum(i => i.Bytes);
		var local_memory_top = 0;

		// Append a return instruction at the end if there is no return instruction present
		if (!instructions.Any() || !instructions.Last().Is(InstructionType.RETURN))
		{
			if (Assembler.IsDebuggingEnabled && unit.Function.Metadata.End != null)
			{
				instructions.Add(new AppendPositionInstruction(unit, unit.Function.Metadata.End));
			}

			instructions.Add(new ReturnInstruction(unit, null, null));
		}

		// If debug information is being generated, append a debug information label at the end
		if (Assembler.IsDebuggingEnabled)
		{
			var end = new LabelInstruction(unit, new Label(Debug.GetEnd(unit.Function).Name));
			end.OnBuild();

			instructions.Add(end);

			// Find sequential position instructions and separate them using NOP-instructions
			for (var i = instructions.Count - 2; i >= 0; i--)
			{
				if (!instructions[i].Is(InstructionType.APPEND_POSITION) || !instructions[i + 1].Is(InstructionType.APPEND_POSITION)) continue;
				instructions.Insert(i + 1, new NoOperationInstruction(unit));
			}
		}

		// Build all initialization instructions
		foreach (var instruction in instructions)
		{
			if (!instruction.Is(InstructionType.INITIALIZE)) continue;

			var initialization = instruction.To<InitializeInstruction>();

			initialization.Build(registers, required_local_memory);
			local_memory_top = initialization.LocalMemoryTop;
		}

		// Reverse the saved registers since they must be recovered from stack when returning from the function so they must be in the reversed order
		registers.Reverse();

		// Build all return instructions
		foreach (var instruction in instructions)
		{
			if (!instruction.Is(InstructionType.RETURN)) continue;

			// Save the local memory size for later use
			unit.Function.SizeOfLocals = unit.StackOffset - local_memory_top;
			unit.Function.SizeOfLocalMemory = unit.Function.SizeOfLocals + registers.Count * Assembler.Size.Bytes;

			instruction.To<ReturnInstruction>().Build(registers, local_memory_top);
		}

		// Reverse the register list to its original order
		registers.Reverse();

		// If optimization is enabled, finish the instructions
		if (Analysis.IsInstructionAnalysisEnabled)
		{
			InstructionAnalysis.Finish(unit, instructions, registers, required_local_memory);
		}

		// Align all used local variables
		Aligner.AlignLocalMemory(local_variables, temporary_handles.ToList(), inline_handles, local_memory_top);

		AllocateConstantDataHandles(unit, new List<ConstantDataSectionHandle>(constant_handles));

		// Translate all instructions
		instructions.ForEach(i => i.Translate());

		// Remove duplicates
		constants.AddRange(constant_handles.Distinct());

		if (Output.ContainsKey(unit.Function.Metadata.Start!.File!))
		{
			Output[unit.Function.Metadata.Start!.File!].AddRange(instructions);
		}
		else
		{
			Output[unit.Function.Metadata.Start!.File!] = instructions;
		}

		return unit.Export();
	}
}