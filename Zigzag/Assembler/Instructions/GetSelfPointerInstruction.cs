using System;

public class GetSelfPointerInstruction : LoadInstruction
{
    public GetSelfPointerInstruction(Unit unit) : base(unit) 
    {
        Result.EntangleTo(unit.Self ?? throw new ApplicationException("Tried to get self pointer in a non-member function"));
    }

    public override void Build()
    {
        if (Result.Value.Type != HandleType.REGISTER)
        {
            Memory.MoveToRegister(Unit, Result);
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_SELF_POINTER;
    }

    public override Result? GetDestination()
    {
        return null;   
    }
}