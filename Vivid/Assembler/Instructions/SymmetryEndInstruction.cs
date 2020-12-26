using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ensures that the current locations of values match the specified start state
/// This instruction works on all architectures
/// </summary>
public class SymmetryEndInstruction : Instruction
{
	public SymmetryStartInstruction Start { get; private set; }
	private List<Result> Sources { get; set; } = new List<Result>();

	public SymmetryEndInstruction(Unit unit, SymmetryStartInstruction start) : base(unit)
	{
		Start = start;
	}

	public void Append()
	{
		Sources.Clear();
		Sources.AddRange(Start.Actives.Select(i => References.GetVariable(Unit, i, AccessMode.READ)));
	}

	public override void OnBuild()
	{
		var moves = new List<MoveInstruction>();

		for (var i = 0; i < Start.Variables.Count; i++)
		{
			var source = Sources[Start.Actives.IndexOf(Start.Variables[i])];
			var destination = new Result(Start.Handles[i], source.Format);

			// There are situation where the variable is constant in the outer scope and it's loaded into a register in the current scope so it should not be relocated
			if (destination.IsConstant)
			{
				continue;
			}

			moves.Add(new MoveInstruction(Unit, destination, source)
			{
				IsSafe = true,
				Description = "Relocate the source in order to make the loop symmetric",
				Type = MoveType.RELOCATE
			});
		}

		Unit.Append(Memory.Align(Unit, moves), true);
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
		return new[] { Result }.Concat(Sources).ToArray();
	}
}