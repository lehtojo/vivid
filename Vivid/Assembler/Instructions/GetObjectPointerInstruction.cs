/// <summary>
/// Specialized instruction for accessing member variables
/// </summary>
public class GetObjectPointerInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; private set; }
	public AccessMode Mode { get; private set; }

	public GetObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset, AccessMode mode) : base(unit)
	{
		Variable = variable;
		Start = start;
		Offset = offset;
		Mode = mode;

		Result.Metadata.Attach(new VariableAttribute(Variable));
		Result.Format = Variable.Type!.Format;
	}

	public override void OnBuild()
	{
		if (!Trace.IsLoadingRequired(Unit, Result))
		{
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{	
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;

			Memory.MoveToRegister(Unit, Result, Assembler.Size, Variable.GetRegisterFormat().IsDecimal(), Result.GetRecommendation(Unit));
		}
		else
		{
			var address = new Result(ExpressionHandle.CreateMemoryAddress(Start, Offset), Assembler.Format);
			Memory.MoveToRegister(Unit, address, Assembler.Size, false, Result.GetRecommendation(Unit));

			Result.Value = new MemoryHandle(Unit, address, 0);
			Result.Format = Variable.Type!.Format;
		}
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Start };
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_OBJECT_POINTER;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}
}