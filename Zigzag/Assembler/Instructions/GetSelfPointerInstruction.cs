using System;

public class GetSelfPointerInstruction : LoadInstruction
{
    public GetSelfPointerInstruction(Unit unit) : base(unit, AccessMode.READ) 
    {
        Source.EntangleTo(unit.Self ?? throw new ApplicationException("Tried to get self pointer in a non-member function"));
    }

    public override void Build()
    {
        if (Result.Value.Type == HandleType.NONE)
        {
            Memory.Convert(Unit, Source, true, HandleType.REGISTER);
            Result.EntangleTo(Source);
        }
        else
        {
            Unit.Build(new MoveInstruction(Unit, Result, Source));
            Source.EntangleTo(Result);
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_SELF_POINTER;
    }
}