
public class GetVariableInstruction : LoadInstruction
{
    public Result? Self { get; private set; }
    public Variable Variable { get; private set; }

    public GetVariableInstruction(Unit unit, Variable variable) : base(unit)
    {
        Variable = variable;
    }

    public GetVariableInstruction(Unit unit, Result? self, Variable variable) : base(unit)
    {
        Self = self;
        Variable = variable;
    }
    
    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_VARIABLE;
    }

    public override Result[] GetHandles()
    {
        if (Self != null)
        {
            return new Result[] { Result, Self };
        }
        else
        {
            return new Result[] { Result  };
        }
    }

    public override void Weld() {}
}