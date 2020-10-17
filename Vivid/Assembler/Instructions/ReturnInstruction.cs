using System.Collections.Generic;
using System.Text;
using System;

public class ReturnInstruction : Instruction
{
	private const string RETURN = "ret";

	public Register ReturnRegister => ReturnType == Types.DECIMAL ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();
	private Handle ReturnRegisterHandle => new RegisterHandle(ReturnRegister);

	public Result? Object { get; private set; }
	public Type? ReturnType { get; private set; }

	public int StackMemoryChange { get; private set; }

	public ReturnInstruction(Unit unit, Result? value, Type? return_type) : base(unit)
	{
		Object = value;
		ReturnType = return_type;
		Result.Format = ReturnType?.GetRegisterFormat() ?? Assembler.Format;
	}

	/// <summary>
	/// Returns whether the return value is in the wanted return register
	/// </summary>
	private bool IsValueInReturnRegister()
	{
		return Object!.Value.Is(HandleType.REGISTER) && Object!.Value.To<RegisterHandle>().Register == ReturnRegister;
	}

	public override void OnBuild()
	{
		// Ensure that if there's a value to return it's in a return register
		if (Object == null || IsValueInReturnRegister())
		{
			return;
		}

		Unit.Append(new MoveInstruction(Unit, new Result(ReturnRegisterHandle, ReturnType!.GetRegisterFormat()), Object));
	}

	public void Build(List<Register> recover_registers, int local_variables_top)
	{
		var builder = new StringBuilder();
		var start = Unit.StackOffset;
		var allocated_local_memory = start - local_variables_top;

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
		return Object != null ? new[] { Result, Object } : new[] { Result };
	}
}