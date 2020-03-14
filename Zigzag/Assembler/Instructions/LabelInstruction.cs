using System;

public class LabelInstruction : Instruction
{
    public Label Label { get; private set; }

    public LabelInstruction(Unit unit, Label label) : base(unit)
    {
        Label = label;
    }

    public override void Build()
    {
        Unit.Append($"{Label.GetName()}:");
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.LABEL;
    }

    public override void RedirectTo(Handle handle)
    {
        throw new InvalidOperationException("Tried to redirect result of a label?");
    }

    public override void Weld() {}
}