using System.Collections.Generic;
using System.Linq;
using System;

public class MergeScopeInstruction : Instruction
{
    //private List<Variable> Variables { get; set; }
    //private List<Result> Loads { get; set; } = new List<Result>();

    public MergeScopeInstruction(Unit unit, IEnumerable<Variable> variables) : base(unit)
    {
        //Variables = variables.ToList();
    }

    private Result GetDestinationHandle(Variable variable)
    {
        return Unit.Scope!.Outer?.GetCurrentVariableHandle(Unit, variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
    }

    private bool IsUsedLater(Variable variable)
    {
        return Unit.Scope!.Outer?.IsUsedLater(variable) ?? false;
    }

    public void Append()
    {
        /*foreach (var variable in Variables)
        {
            Loads.Add(References.GetVariable(Unit, variable, AccessMode.READ));
        }*/
    }

    public override void Build() 
    {
        var moves = new List<MoveInstruction>();

        foreach (var variable in Scope!.ActiveVariables)
        {
            var source = Unit.GetCurrentVariableHandle(variable) ?? throw new ApplicationException("Couldn't get the current handle for an active variable");
            var destination = GetDestinationHandle(variable);
            
            // When the destination is a memory handle, it most likely means it won't be used later
            if (destination.Value.Type == HandleType.MEMORY && !IsUsedLater(variable))
            {
                continue;
            }

            moves.Add(new MoveInstruction(Unit, destination, source));
        }

        /*for (var i = 0; i < Loads.Count; i++)
        {
            var source = Loads[i];
            var destination = GetDestinationHandle(Variables[i]);
            
            // When the destination is a memory handle, it most likely means it won't be used later
            if (destination.Value.Type == HandleType.MEMORY && !IsUsedLater(Variables[i]))
            {
                continue;
            }

            moves.Add(new MoveInstruction(Unit, destination, source));
        }*/

        var remove_list = new List<MoveInstruction>();

        foreach (var a in moves)
        {
            foreach (var b in moves)
            {
                if (a == b) continue;
                
                if (a.First.Value.Equals(b.Second.Value) &&
                    a.Second.Value.Equals(b.First.Value))
                {
                    /// TODO: Implement XCHG
                    throw new NotImplementedException("Implement exchange instruction");
                    
                    //remove_list.Add(a);
                    //remove_list.Add(b);
                    //break;
                }
            }
        }

        moves.RemoveAll(m => remove_list.Contains(m));

        moves.Sort((a, b) => a.First.Value.Equals(b.Second.Value) ? 1 : 0);
        moves.ForEach(move => Unit.Append(move));
    }

    public override Result? GetDestinationDependency()
    {
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MERGE_SCOPE;
    }

    public override Result[] GetResultReferences()
    {
        //return Loads.ToArray();
        return new Result[] { Result };
    }
}