using System.Collections.Generic;
using System.Text;

public class InitializeInstruction : Instruction
{
    public int StackMemoryChange { get; private set; }
    public int LocalVariablesTop { get; private set; }

    public InitializeInstruction(Unit unit) : base(unit) {}

    public override void OnBuild() {}

    public void Build(List<Register> save_registers, int required_local_memory)
    {
        var builder = new StringBuilder();
        var start = Unit.StackOffset;

        foreach (var register in save_registers)
        {
            builder.AppendLine($"push {register}");
            Unit.StackOffset += Assembler.Size.Bytes;
        }

        // Local variables in memory start now
        LocalVariablesTop = Unit.StackOffset;

        if (required_local_memory > 0)
        {
            builder.Append($"sub {Unit.GetStackPointer()}, {required_local_memory}");
            Unit.StackOffset += required_local_memory;
        }
        else if (save_registers.Count > 0)
        {
            // Remove the last line ending
            builder.Remove(builder.Length - 1, 1);
        }

        StackMemoryChange = Unit.StackOffset - start;

        Build(builder.ToString());
    }

    public override int GetStackOffsetChange()
    {
        return StackMemoryChange;
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