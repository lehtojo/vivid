using System;
using System.Collections.Generic;
using System.Linq;

public class SymmetryStartInstruction : Instruction
{
    public List<Variable> ActiveVariables { get; private set; }
    public List<Handle> Handles { get; private set; } = new List<Handle>();

    public SymmetryStartInstruction(Unit unit, IEnumerable<Variable>? active_variables) : base(unit)
    {
        ActiveVariables = active_variables?.ToList() ?? new List<Variable>();
    }

    public override void OnBuild()
    {
        Handles.Clear();

        foreach (var variable in ActiveVariables)
        {
            var current_handle = Unit.GetCurrentVariableHandle(variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
            Handles.Add(current_handle.Value);
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
        return new Result[] { Result };
    }
}