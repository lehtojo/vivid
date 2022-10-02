/// <summary>
/// This instruction appends the specified source file position to the generated assembly for debug information
/// This instruction is works on all architectures
/// </summary>
public class DebugBreakInstruction : Instruction
{
	public const string INSTRUCTION = ".loc";

	public Position Position { get; private set; }

	public static string GetPositionInstruction(Position position)
	{
		return INSTRUCTION + " 1 " + position.FriendlyLine.ToString() + ' ' + position.FriendlyCharacter.ToString();
	}

	public DebugBreakInstruction(Unit unit, Position position) : base(unit, InstructionType.DEBUG_BREAK)
	{
		Position = position;
		Operation = GetPositionInstruction(Position);
		State = InstructionState.BUILT;
		Description = ToString();
	}

	public override string ToString()
	{
		return $"Line: {Position.FriendlyLine}, Character: {Position.FriendlyCharacter}";
	}
}