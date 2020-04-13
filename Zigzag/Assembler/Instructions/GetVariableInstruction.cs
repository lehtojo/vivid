
public class GetVariableInstruction : LoadInstruction
{
    public Result? Self { get; private set; }
    public Variable Variable { get; private set; }

    public GetVariableInstruction(Unit unit, Result? self, Variable variable, AccessMode mode) : base(unit, mode)
    {
        Self = self;
        Variable = variable;
        
        SetSource(References.CreateVariableHandle(unit, Self, variable));
    }
    
    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_VARIABLE;
    }

    public override Result[] GetResultReferences()
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
}