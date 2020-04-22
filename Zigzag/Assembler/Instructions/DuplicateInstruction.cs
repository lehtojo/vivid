
public class DuplicateInstruction : DualParameterInstruction
{
    public DuplicateInstruction(Unit unit, Result value) : base(unit, new Result(), value) {}

    public override void Build()
    {
        if (Result.Empty)
        {
            Result.Value = new RegisterHandle(Unit.GetNextRegister());
        }

        var move = new MoveInstruction(Unit, Result, Second);
        move.Type = MoveType.LOAD;

        if (Second.Metadata.PrimaryAttribute != null)
        {
            Result.Metadata.Attach(Second.Metadata.PrimaryAttribute);
        }

        Unit.Append(move);
    }

    public override Result? GetDestinationDependency()
    {
        return Result;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.DUPLICATE;
    }
}