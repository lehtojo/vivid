/// <summary>
/// Allocates or deallocates the stack
/// </summary>
public class StackMemoryInstruction : Instruction
{
	public const string SHARED_ALLOCATE_INSTRUCTION = "sub";
	public const string SHARED_SHRINK_INSTRUCTION = "add";

	public int Bytes { get; private set; }
	public bool Hidden { get; private set; }

	private StackMemoryInstruction(Unit unit, int bytes, bool hidden) : base(unit)
	{
		Bytes = bytes;
		Hidden = hidden;
	}

	public static StackMemoryInstruction Allocate(Unit unit, int bytes, bool hidden)
	{
		return new StackMemoryInstruction(unit, bytes, hidden);
	}

	public static StackMemoryInstruction Shrink(Unit unit, int bytes, bool hidden)
	{
		return new StackMemoryInstruction(unit, -bytes, hidden);
	}

	public override void OnBuild()
	{
		if (Hidden)
		{
			return;
		}

		var stack_pointer = Unit.GetStackPointer();

		if (Assembler.IsArm64)
		{
			if (Bytes < 0)
			{
				Build($"{SHARED_SHRINK_INSTRUCTION} {stack_pointer}, {stack_pointer}, {-Bytes}");
			}
			else if (Bytes > 0)
			{
				Build($"{SHARED_ALLOCATE_INSTRUCTION} {stack_pointer}, {stack_pointer}, {Bytes}");
			}

			return;
		}

		if (Bytes < 0)
		{
			Build($"{SHARED_SHRINK_INSTRUCTION} {stack_pointer}, {-Bytes}");
		}
		else if (Bytes > 0)
		{
			Build($"{SHARED_ALLOCATE_INSTRUCTION} {stack_pointer}, {Bytes}");
		}
	}

	public override int GetStackOffsetChange()
	{
		return Bytes;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.STACK_MEMORY;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}