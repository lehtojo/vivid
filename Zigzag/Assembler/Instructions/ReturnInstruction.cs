using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ReturnInstruction : Instruction
{
    private const string RETURN = "ret";

    public Result? Object { get; private set; }
    public int StackMemoryChange { get; private set; }

    public ReturnInstruction(Unit unit, Result? value) : base(unit)
    {
        Object = value;
    }

    private bool IsObjectInReturnRegister()
    {
        return Object!.Value is RegisterHandle handle && handle.Register.IsReturnRegister;
    }

    private Result GetReturnRegister()
    {
        return new Result(new RegisterHandle(Unit.GetStandardReturnRegister()));
    }
    
    public override void Build()
    {
        // Ensure that if there's a value to return it's in a return register
        if (Object != null && !IsObjectInReturnRegister())
        {
            Unit.Append(new MoveInstruction(Unit, GetReturnRegister(), Object));
        }
    }

    public void Build(List<Register> recover_registers, int allocated_local_memory)
    {
        var builder = new StringBuilder();
        var start = Unit.StackOffset;

        if (allocated_local_memory > 0)
        {
            builder.AppendLine($"add {Unit.GetStackPointer()}, {allocated_local_memory}");
            Unit.StackOffset -= allocated_local_memory;
        }

        foreach (var register in recover_registers)
        {
            builder.AppendLine($"pop {register}");
            Unit.StackOffset -= Assembler.Size.Bytes;
        }

        builder.Append(RETURN);

        StackMemoryChange = Unit.StackOffset - start;

        Build(builder.ToString());
    }

    public override int GetStackOffsetChange()
    {
        return StackMemoryChange;
    }

    public override Result? GetDestinationDependency()
    {
        return Object;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.RETURN;
    }

    public override Result[] GetResultReferences()
    {
        return Object != null ? new Result[] { Result, Object } : new Result[] { Result };
    }
}