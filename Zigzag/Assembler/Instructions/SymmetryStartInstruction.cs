using System;
using System.Collections.Generic;
using System.Linq;

public class SymmetryStartInstruction : Instruction
{
    public List<Variable> NonLocalVariables { get; private set; }
    public List<Handle> Handles { get; private set; } = new List<Handle>();

    public SymmetryStartInstruction(Unit unit, IEnumerable<Variable>? non_local_variables) : base(unit)
    {
        NonLocalVariables = non_local_variables?.ToList() ?? new List<Variable>();
    }

    public override void Build()
    {
        Handles.Clear();

        foreach (var variable in NonLocalVariables)
        {
            var current_handle = Unit.GetCurrentVariableHandle(variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
            Handles.Add(current_handle.Value);
        }
    }

    public override Result? GetDestinationDependency()
    {
        throw new ApplicationException("Tried to redirect Loop-Connect-Start-Instruction");
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