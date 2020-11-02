/// <summary>
/// Specialized instruction for accessing member variables
/// </summary>
public class GetObjectPointerInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; private set; }

	public GetObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset) : base(unit)
	{
		Variable = variable;
		Start = start;
		Offset = offset;

		Result.Metadata.Attach(new VariableAttribute(Variable));
		Result.Format = Variable.Type!.Format;
	}

	public override void OnBuild()
	{
		Result.Value = new MemoryHandle(Unit, Start, Offset);
		Result.Format = Variable.Type!.Format;
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