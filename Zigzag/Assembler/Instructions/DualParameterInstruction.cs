public abstract class DualParameterInstruction : Instruction
{
    public Quantum<Handle> First { get; private set; }
    public Quantum<Handle> Second { get; private set; }

    public DualParameterInstruction(Quantum<Handle> first, Quantum<Handle> second)
    {
        First = first;
        Second = second;
    }

    public override Handle[] GetHandles()
    {
        return new Handle[] { Result.Value, First.Value, Second.Value };
    }
}