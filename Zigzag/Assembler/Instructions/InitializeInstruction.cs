using System.Collections.Generic;
using System.Text;

public class InitializeInstruction : Instruction
{
    public InitializeInstruction(Unit unit) : base(unit) {}

    public override void Build() {}

    public void Build(List<Register> save_registers, int required_local_memory)
    {
        var builder = new StringBuilder();

        foreach (var register in save_registers)
        {
            builder.AppendLine($"push {register}");
        }

        if (required_local_memory > 0)
        {
            builder.Append($"sub {Unit.GetStackPointer()}, {required_local_memory}");
        }
        else if (save_registers.Count > 0)
        {
            // Remove the last line ending
            builder.Remove(builder.Length - 1, 1);
        }

        Build(builder.ToString());
    }

    public override Result? GetDestinationDependency()
    {
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.INITIALIZE;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}