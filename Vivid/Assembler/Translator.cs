using System;
using System.Collections.Generic;
using System.Linq;

public static class Translator
{
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
		var handles = unit.Instructions.SelectMany(i => i.Parameters.Select(i => i.Value ?? throw new ApplicationException("Instruction parameter was not assigned")));

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

	private static List<StackAllocationHandle> GetAllInlineHandles(Unit unit)
	{
		return GetAllHandles(unit)
			.Where(i => i.Is(HandleInstanceType.STACK_ALLOCATION))
			.Cast<StackAllocationHandle>()
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

			var identifier = unit.GetNextConstant();
			copies.ForEach(i => i.Identifier = identifier);
			copies.ForEach(i => constant_data_section_handles.Remove(i));
		}
	}

	/// <summary>
	/// Finds sequential debug position instructions and separates them using NOP-instructions
	/// </summary>
	private static void SeparateDebugPositions(Unit unit, List<Instruction> instructions)
	{
		for (var i = 0; i < instructions.Count;)
		{
			// Find the next debug position instruction
			if (instructions[i].Type != InstructionType.DEBUG_BREAK)
			{
				i++;
				continue;
			}

			// Find the next hardware instruction or debug position instruction
			var j = i + 1;

			for (; j < instructions.Count; j++)
			{
				var instruction = instructions[j];

				if (instruction.Type == InstructionType.LABEL) continue;
				if (instruction.Type == InstructionType.DEBUG_BREAK || !instruction.IsAbstract) break;
			}

			// We need to insert a NOP-instruction in the following cases:
			// - We reached the end of the instruction list
			// - We found a debug position instruction
			// In the above cases, there are no hardware instructions where the debugger could stop after the debug position instruction.
			if (j == instructions.Count || instructions[j].Type == InstructionType.DEBUG_BREAK)
			{
				instructions.Insert(i + 1, new NoOperationInstruction(unit));
				j++; // Update the index, because we inserted a new instruction
			}

			i = j;
		}
	}

	public static void Translate(AssemblyBuilder builder, Unit unit)
	{
		TotalInstructions += unit.Instructions.Count;
		
		// Take only the instructions which are actual assembly instructions
		var instructions = unit.Instructions.Where(i => !i.IsAbstract).ToList();

		// If optimization is enabled, try to optimize the generated code on instruction level
		if (Analysis.IsInstructionAnalysisEnabled) InstructionAnalysis.Optimize(unit, instructions);

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
				instructions.Add(new DebugBreakInstruction(unit, unit.Function.Metadata.End));
			}

			instructions.Add(new ReturnInstruction(unit, null, null));
		}

		// If debug information is being generated, append a debug information label at the end
		if (Assembler.IsDebuggingEnabled)
		{
			var end = new LabelInstruction(unit, new Label(Debug.GetEnd(unit.Function).Name));
			end.OnBuild();

			instructions.Add(end);

			SeparateDebugPositions(unit, instructions);
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
		Aligner.AlignLocalMemory(unit.Function, local_variables, temporary_handles.ToList(), inline_handles, local_memory_top);

		AllocateConstantDataHandles(unit, new List<ConstantDataSectionHandle>(constant_handles));

		var file = unit.Function.Metadata.Start!.File!;

		if (Assembler.IsAssemblyOutputEnabled || Assembler.IsLegacyAssemblyEnabled)
		{
			// Convert all instructions into textual assembly
			instructions.ForEach(i => i.Finish());

			builder.Write(unit.ToString());

			// Add a directive, which tells the assembler to finish debugging information regarding the current function
			if (Assembler.IsDebuggingEnabled) builder.WriteLine(Assembler.DebugFunctionEndDirective);
		}

		builder.Add(file, instructions);

		// Add a directive, which tells the assembler to finish debugging information regarding the current function
		builder.Add(file, new Instruction(unit, InstructionType.DEBUG_END));

		// Export the generated constants as well
		builder.Add(file, constant_handles.Distinct().ToList());
	}
}