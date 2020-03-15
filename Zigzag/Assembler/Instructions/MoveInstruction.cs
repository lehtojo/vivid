public class MoveInstruction : DualParameterInstruction
{
    public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Weld() 
    {
        //Result.SetParent(First);
    }

    public override void Build()
    {
        if (First.Value.Type == HandleType.MEMORY_HANDLE)
        {
            Build(
                "mov",
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.MEMORY_HANDLE
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
                "mov",
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.MEMORY_HANDLE
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

        if (First.Value is RegisterHandle handle)
        {
            handle.Register.Value = Second;
        }
    }

    public override void RedirectTo(Handle handle)
    {
        First.Set(handle, true);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MOVE;
    }
}