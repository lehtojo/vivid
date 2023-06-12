using System.Collections.Generic;
using System.Linq;
using System;

public class ReorderInstruction : Instruction
{
	public List<Handle> Destinations { get; }
	public List<Format> Formats { get; }
	public List<Result> Sources { get; }
	public Type? ReturnType { get; }
	public bool Extracted { get; private set; } = false;

	public ReorderInstruction(Unit unit, List<Handle> destinations, List<Result> sources, Type? return_type) : base(unit, InstructionType.REORDER)
	{
		Dependencies = null;
		Destinations = destinations;
		Formats = Destinations.Select(i => i.Format).ToList();
		Sources = sources;
		ReturnType = return_type;
	}

	/// <summary>
	/// Evacuates variables that are located at the overflow zone of the stack
	/// </summary>
	private void EvacuateOverflowZone(Type type)
	{
		var overflow = Math.Max(Common.ComputeReturnOverflow(Unit, type), Settings.IsTargetWindows ? Calls.SHADOW_SPACE_SIZE : 0);

		foreach (var iterator in Unit.Scope!.Variables)
		{
			// Find all memory handles
			var value = iterator.Value;
			var instance = value.Value.Instance;
			if (instance != HandleInstanceType.STACK_MEMORY && instance != HandleInstanceType.TEMPORARY_MEMORY && instance != HandleInstanceType.MEMORY) continue;

			var memory = value.Value.To<MemoryHandle>();

			// Ensure the memory address represents a stack address
			var start = memory.GetStart();
			if (start == null || start != Unit.GetStackPointer()) continue;

			// Ensure the memory address overlaps with the overflow
			var offset = memory.GetAbsoluteOffset();
			if (offset < 0 || offset >= overflow) continue;

			var variable = iterator.Key;

			// Try to get an available non-volatile register
			var destination = (Handle?)null;
			var destination_format = Settings.Format;
			var register = Memory.GetNextRegister(Unit, variable.Type!.Format.IsDecimal(), Trace.For(Unit, value));

			// Use the non-volatile register, if one was found
			if (register != null)
			{
				destination = new RegisterHandle(register);
				destination_format = variable.Type!.GetRegisterFormat();
			}
			else
			{
				// Since there are no non-volatile registers available, the value must be relocated to safe stack location
				destination = References.CreateVariableHandle(Unit, variable);
				destination_format = variable.Type!.Format;
			}

			Unit.Add(new MoveInstruction(Unit, new Result(destination, destination_format), value)
			{
				Description = "Evacuate overflow",
				Type = MoveType.RELOCATE
			});
		}
	}

	public override void OnBuild()
	{
		if (ReturnType != null) EvacuateOverflowZone(ReturnType);

		var instructions = new List<Instruction>();

		for (var i = 0; i < Destinations.Count; i++)
		{
			var source = Sources[i];
			var destination = new Result(Destinations[i], Formats[i]);

			instructions.Add(new MoveInstruction(Unit, destination, source) { IsDestinationProtected = true });
		}

		Memory.Align(Unit, instructions.Cast<MoveInstruction>().ToList());
		Extracted = true;
	}

	public override Result[] GetDependencies()
	{
		if (Extracted) return Array.Empty<Result>();
		return Sources.ToArray();
	}
}