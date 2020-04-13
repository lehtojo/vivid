using System;

public class GetSelfPointerInstruction : LoadInstruction
{
    public GetSelfPointerInstruction(Unit unit) : base(unit, AccessMode.READ) 
    {
        Source.Join(unit.Self ?? throw new ApplicationException("Tried to get self pointer in a non-member function"));
    }

    public override void Build()
    {
        if (Result.Empty)
        {
            Result.Value = new RegisterHandle(Unit.GetNextRegister());
        }

        var move = new MoveInstruction(Unit, Result, Source);
        move.Mode = MoveMode.LOAD;

        Unit.Append(move);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_SELF_POINTER;
    }
}