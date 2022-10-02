using System.Collections.Generic;

public abstract class DualParameterInstruction : Instruction
{
	public Result First { get; private set; }
	public Result Second { get; private set; }
	public bool Unsigned { get; private set; }

	public DualParameterInstruction(Unit unit, Result first, Result second, Format format, InstructionType type) : base(unit, type)
	{
		First = first;
		Second = second;
		Unsigned = format.IsUnsigned();
		Result.Format = format;

		Dependencies = new List<Result> { Result, First, Second };
	}

	public override Result[] GetDependencies()
	{
		return new[] { Result, First, Second };
	}
}