public class SubtractionInstruction : DualParameterInstruction
{
    public SubtractionInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Weld() 
    {
        //Result.SetParent(First);
    }
    
    public override void Build()
    {
        Build(
            "sub", 
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

    public override void RedirectTo(Handle handle)
    {
        First.Set(handle, true);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.SUBTRACT;
    }
}