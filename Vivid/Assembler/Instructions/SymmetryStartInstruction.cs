using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Saves the current locations of the specified variables
/// This instruction works on all architectures
/// </summary>
public class SymmetryStartInstruction : Instruction
{
	public List<Variable> Actives { get; private set; }

	public List<Handle> Handles { get; private set; } = new List<Handle>();
	public List<Variable> Variables { get; private set; } = new List<Variable>();

	public SymmetryStartInstruction(Unit unit, IEnumerable<Variable>? active_variables) : base(unit)
	{
		Actives = active_variables?.ToList() ?? new List<Variable>();
	}

	public override void OnBuild()
	{
		Handles.Clear();
		Variables.Clear();

		var handles = Actives
			.Select(i => Unit.GetCurrentVariableHandle(i) ?? References.GetVariable(Unit, i, AccessMode.READ))
			.ToArray();

		for (var i = 0; i < Actives.Count; i++)
		{
			var handle = handles[i];

			if (handles.Take(i).All(j => !j.Equals(handle)))
			{
				Handles.Add(handle.Value.Finalize());
				Variables.Add(Actives[i]);
			}
		}
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Symmetry-Start-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SYMMETRY_START;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}