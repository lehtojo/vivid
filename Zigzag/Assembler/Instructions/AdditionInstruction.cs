using System.Text;

public class AdditionInstruction : DualParameterInstruction
{
    public AdditionInstruction(Quantum<Handle> first, Quantum<Handle> second) : base(first, second) {}

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ADDITION;
    }

    public override void Weld(Unit unit)
    {
        if (First.Value.IsDying(unit))
        {
            Result.SetParent(First);
        }
    }

    public override void Build(Unit unit)
    {
        if (First.Value.IsDying(unit))
        {
            Build(
                unit, "add",
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
                    HandleType.STACK_MEMORY_HANDLE
                )
            );
        }
        else
        {
            var result = new StringBuilder();

            // Form the calculation parameter
            var calculation = Mold(
                unit, result, "[{0}+{1}]",
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
                Memory.GetRegisterFor(unit, Result);
            }

            unit.Append($"lea {Result}, {calculation}");
        }
    }
}