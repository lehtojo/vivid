using System.Globalization;

public class Lifetime
{
	public int Start { get; set; } = -1;
	public int End { get; set; } = -1;

	public void Reset()
	{
		Start = -1;
		End = -1;
	}

	/// <summary>
	/// Returns whether this lifetime is active, that is, whether the lifetime has started but not ended from the perspective of the specified position
	/// </summary>
	public bool IsActive(int position)
	{
		return position >= Start && (End == -1 || position <= End);
	}

	/// <summary>
	/// Returns true, if the lifetime is active and is not starting or ending at the specified instruction position, otherwise false
	/// </summary>
	public bool IsOnlyActive(int position)
	{
		return IsActive(position) && Start != position && End != position;
	}

	public override string ToString()
	{
		if (Start == -1 && End == -1) return "static";

		return (Start == -1 ? string.Empty : Start.ToString()) + ".." + (End == -1 ? string.Empty : End.ToString());
	}
}