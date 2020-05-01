using System;
using System.Collections.Generic;

public class SymmetryEndInstruction : Instruction
{
    public SymmetryStartInstruction Start  { get; private set; }
    private List<Result> Loads { get; set; } = new List<Result>();

    public SymmetryEndInstruction(Unit unit, SymmetryStartInstruction start) : base(unit) 
    {
        Start = start;
    }

    public void Append()
    {
        foreach (var variable in Start.ActiveVariables)
        {
            var source = References.GetVariable(Unit, variable, AccessMode.READ);
            Loads.Add(source);
        }
    }

    public override void OnBuild()
    {
        var moves = new List<DualParameterInstruction>();

        for (var i = 0; i < Loads.Count; i++)
        {
            var source = Loads[i];
            var destination = new Result(Start.Handles[i]);

            moves.Add(new MoveInstruction(Unit, destination, source));
        }

        var remove_list = new List<DualParameterInstruction>();
        var exchanges = new List<ExchangeInstruction>();

        foreach (var a in moves)
        {
            foreach (var b in moves)
            {
                if (a == b) continue;
                
                if (a.First.Value.Equals(b.Second.Value) &&
                    a.Second.Value.Equals(b.First.Value))
                {
                    exchanges.Add(new ExchangeInstruction(Unit, a.First, a.Second));

                    remove_list.Add(a);
                    remove_list.Add(b);
                    break;
                }
            }
        }

        moves.AddRange(exchanges);
        moves.RemoveAll(m => remove_list.Contains(m));

        moves.Sort((a, b) => a.First.Value.Equals(b.Second.Value) ? 1 : 0);
        moves.ForEach(move => Unit.Append(move));
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
        return new Result[] { Result };
    }
}