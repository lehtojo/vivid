public class GetObjectPointerInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; private set; }

	public GetObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset) : base(unit)
	{
		Result.Metadata.Attach(new VariableAttribute(variable));

		Variable = variable;
		Start = start;
		Offset = offset;
	}

	public override void OnBuild()
	{
		// Lock the parameters (Maybe new type of lifetime which stores a result its dependent on)
		Memory.Convert(Unit, Start, true, HandleType.CONSTANT, HandleType.REGISTER);
		Result.Value = new MemoryHandle(Unit, Start, Offset);
		Result.Value.Format = Variable.Type!.Format;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result, Start };
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