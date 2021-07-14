using System.Collections.Generic;
using System.Linq;
using System;

public class ReorderInstruction : Instruction
{
	public List<Handle> Destinations { get; }
	public List<Format> Formats { get; }
	public List<Result> Sources { get; }
	public bool Extracted { get; private set; } = false;

	public ReorderInstruction(Unit unit, List<Handle> destinations, List<Result> sources) : base(unit, InstructionType.REORDER)
	{
		Dependencies = null;
		Destinations = destinations;
		Formats = Destinations.Select(i => i.Format).ToList();
		Sources = sources;
	}

	public override void OnBuild()
	{
		var instructions = new List<Instruction>();

		for (var i = 0; i < Destinations.Count; i++)
		{
			var source = Sources[i];
			var destination = new Result(Destinations[i], Formats[i]);

			instructions.Add(new MoveInstruction(Unit, destination, source) { IsSafe = true });
		}

		instructions = Memory.Align(Unit, instructions.Cast<MoveInstruction>().ToList());
		
		Extracted = true;
		Unit.Append(instructions);
	}

	public override Result[] GetResultReferences()
	{
		if (Extracted) return Array.Empty<Result>();
		return Sources.ToArray();
	}
}