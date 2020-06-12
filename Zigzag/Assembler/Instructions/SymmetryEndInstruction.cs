using System;
using System.Collections.Generic;
using System.Linq;

public class SymmetryEndInstruction : Instruction
{
	public SymmetryStartInstruction Start  { get; private set; }
	private List<Result> Handles { get; set; } = new List<Result>();

	public SymmetryEndInstruction(Unit unit, SymmetryStartInstruction start) : base(unit) 
	{
		Start = start;
	}

	public void Append()
	{
		foreach (var variable in Start.ActiveVariables)
		{
			Handles.Add(References.GetVariable(Unit, variable, AccessMode.READ));
		}
	}

	public override void OnBuild()
	{
		var moves = new List<MoveInstruction>();

		for (var i = 0; i < Handles.Count; i++)
		{
			var source = Handles[i];
			var destination = new Result(Start.Handles[i], source.Format);

			moves.Add(new MoveInstruction(Unit, destination, source)
			{
				IsSafe = true,
				Description = "Relocate the source in order to make the loop symmetric",
			});
		}

		Unit.Append(Memory.Relocate(Unit, moves), true);
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Symmetry-End-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SYMMETRY_END;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result }.Concat(Handles).ToArray();
	}
}