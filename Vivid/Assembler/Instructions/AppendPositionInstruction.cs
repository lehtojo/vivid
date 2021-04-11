using System.Globalization;

/// <summary>
/// This instruction appends the specified source file position to the generated assembly for debug information
/// This instruction is works on all architectures
/// </summary>
public class AppendPositionInstruction : Instruction
{
	public const string INSTRUCTION = ".loc";

	public new Position Position { get; private set; }

	public static string GetPositionInstruction(Position position)
	{
		var line = position.FriendlyLine.ToString(CultureInfo.InvariantCulture);
		var character = position.FriendlyCharacter.ToString(CultureInfo.InvariantCulture);

		return INSTRUCTION + " 1 " + line + ' ' + character;
	}

	public AppendPositionInstruction(Unit unit, Position position) : base(unit, InstructionType.APPEND_POSITION)
	{
		Position = position;
		Operation = GetPositionInstruction(Position);
		IsBuilt = true;
	}

	public override string ToString()
	{
		return $"Line: {Position.FriendlyLine}, Character: {Position.FriendlyCharacter}";
	}
}