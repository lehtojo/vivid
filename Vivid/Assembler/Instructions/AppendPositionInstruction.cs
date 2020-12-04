using System;
using System.Globalization;

public class AppendPositionInstruction : Instruction
{
	public const string INSTRUCTION = ".loc";

	public new Position Position { get; private set; }

	public static string GetPositionInstruction(Position position, int file)
	{
		var line = position.FriendlyLine.ToString(CultureInfo.InvariantCulture);
		var character = position.FriendlyCharacter.ToString(CultureInfo.InvariantCulture);

		return INSTRUCTION + ' ' + file.ToString(CultureInfo.InvariantCulture) + ' ' + line + ' ' + character;
	}

	public AppendPositionInstruction(Unit unit, Position position) : base(unit)
	{
		Position = position;
	}
	
	public override void OnBuild()
	{
		var line = Position.FriendlyLine.ToString(CultureInfo.InvariantCulture);
		var character = Position.FriendlyCharacter.ToString(CultureInfo.InvariantCulture);

		Build(GetPositionInstruction(Position, 1));
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.APPEND_POSITION;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}

	public override Result? GetDestinationDependency()
	{
		throw new InvalidOperationException("Tried to redirect Append-Position-Instruction");
	}

	public override bool Redirect(Handle handle)
	{
		throw new InvalidOperationException("Tried to redirect Append-Position-Instruction");
	}

	public override string ToString()
	{
		return $"Line: {Position.FriendlyLine}, Character: {Position.FriendlyCharacter}";
	}
}