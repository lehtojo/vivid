using System.Text;

public class AdditionInstruction : DualParameterInstruction
{
    public AdditionInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ADDITION;
    }
    
    public override void Build()
    {
        if (First.IsDying(Position))
        {
            Build(
                "add",
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER
                ),
                new InstructionParameter(
                    Second,
                    false,
                    HandleType.CONSTANT,
                    HandleType.REGISTER,
                    HandleType.MEMORY_HANDLE
                )
            );
        }
        else
        {
            var result = new StringBuilder();

            // Form the calculation parameter
            var calculation = Format(
                result, "[{0}+{1}]",
                new InstructionParameter(
                    First,
                    false,
                    HandleType.REGISTER,
                    HandleType.CONSTANT
                ),
                new InstructionParameter(
                    Second,
                    false,
                    HandleType.REGISTER,
                    HandleType.CONSTANT
                )
            );

            if (Result.Value.Type != HandleType.REGISTER)
            {
                // Get a new register for the result
                Memory.GetRegisterFor(Unit, Result);
            }

            Unit.Append($"lea {Result}, {calculation}");
        }
    }

    public override Result? GetDestination()
    {
        if (First.IsDying(Position))
        {
            return First;
        }
        else
        {
            return Result;
        }
    }
}