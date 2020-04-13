using System.Collections.Generic;
using System.Linq;
using System;

public class MergeScopeInstruction : Instruction
{
    private List<Variable> Variables { get; set; }
    private List<Result> Loads { get; set; } = new List<Result>();

    public MergeScopeInstruction(Unit unit, IEnumerable<Variable> variables) : base(unit)
    {
        Variables = variables.ToList();
    }

    private Result GetDestinationHandle(Variable variable)
    {
        return Unit.Scope!.Outer?.GetCurrentVariableHandle(Unit, variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
    }

    public void Append()
    {
        foreach (var variable in Variables)
        {
            var source = References.GetVariable(Unit, variable, AccessMode.READ);
            Loads.Add(source);
        }
    }

    public override void Build() 
    {
        var moves = new List<MoveInstruction>();

        for (var i = 0; i < Loads.Count; i++)
        {
            var source = Loads[i];
            var destination = GetDestinationHandle(Variables[i]);

            moves.Add(new MoveInstruction(Unit, destination, source));
        }

        var remove_list = new List<MoveInstruction>();

        foreach (var a in moves)
        {
            foreach (var b in moves)
            {
                if (a == b) continue;
                
                if (a.First.Value.Equals(b.Second.Value) &&
                    a.Second.Value.Equals(b.First.Value))
                {
                    // Append XCHG
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
        return Loads.ToArray();
    }
}