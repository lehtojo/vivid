using System;
using System.Collections.Generic;

public class ParenthesisType
{
	public static readonly ParenthesisType PARENTHESIS = new('(', ')');
	public static readonly ParenthesisType BRACKETS = new('[', ']');
	public static readonly ParenthesisType CURLY_BRACKETS = new('{', '}');

	private static Dictionary<char, ParenthesisType> Map { get; } = new Dictionary<char, ParenthesisType>();

	static ParenthesisType()
	{
		Map.Add(PARENTHESIS.Opening, PARENTHESIS);
		Map.Add(BRACKETS.Opening, BRACKETS);
		Map.Add(CURLY_BRACKETS.Opening, CURLY_BRACKETS);
	}

	public char Opening { get; private set; }
	public char Closing { get; private set; }

	private ParenthesisType(char opening, char closing)
	{
		Opening = opening;
		Closing = closing;
	}

	public static ParenthesisType Get(char opening)
	{
		return Map[opening];
	}

	public static bool Has(char opening)
	{
		return Map.ContainsKey(opening);
	}

	public override bool Equals(object? other)
	{
		return other is ParenthesisType type &&
				base.Equals(other) &&
				Opening == type.Opening &&
				Closing == type.Closing;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Opening, Closing);
	}
}