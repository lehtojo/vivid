public class MoveInstruction : DualParameterInstruction
{
    public MoveInstruction(Quantum<Handle> first, Quantum<Handle> second) : base(first, second) 
    {
        Result = first;
    }

    public override void Weld(Unit unit) {}

    public override void Build(Unit unit)
    {
        if (First.Value.Type == HandleType.STACK_MEMORY_HANDLE)
        {
            Build(
                unit, "mov",
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.STACK_MEMORY_HANDLE
                ),
                new InstructionParameter(
                    Second,
                    false,
                    HandleType.CONSTANT,
                    HandleType.REGISTER
                )
            );
        }
        else
        {
            Build(
                unit, "mov",
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.STACK_MEMORY_HANDLE
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

        if (First.Value is RegisterHandle handle)
        {
            handle.Register.Value = Second;
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MOVE;
    }
}