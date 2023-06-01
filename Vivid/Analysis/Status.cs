public class Status
{
	public static readonly Status OK = new("OK", false);

	public Position? Position { get; } = null;
	public string Message { get; } = string.Empty;
	public bool IsProblematic { get; private set; } = false;

	public Status(string message, bool problematic)
	{
		Message = message;
		IsProblematic = problematic;
	}

	public Status(string message)
	{
		Position = null;
		Message = message;
		IsProblematic = true;
	}

	public Status(string format, params string[] arguments)
	{
		Position = null;
		Message = string.Format(format, arguments);
		IsProblematic = true;
	}

	public Status(Position? position, string message)
	{
		Position = position;
		Message = message;
		IsProblematic = true;
	}

	public override bool Equals(object? other)
	{
		if (other is not Status status) return false;

		if (Message != status.Message) return false;
		return Equals(Position, status.Position);
	}

	public static Status Warning(Position? position, string message)
	{
		return new Status(position, message);
	}
}