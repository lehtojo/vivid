using System.Collections.Generic;
using System;

public class PatternState
{
	public List<Token> Tokens { get; set; }
	public List<Token> Formatted { get; set; }

	public int Start { get; set; }
	public int End { get; set; }

	public int Min { get; set; }
	public int Max { get; set; }

	public PatternState(List<Token> tokens, List<Token> formatted, int start, int end, int min, int max)
	{
		Tokens = tokens;
		Formatted = formatted;
		Start = start;
		End = end;
		Min = min;
		Max = max;
	}
}

public abstract class Pattern
{
	private List<int> Path { get; set; }

	public Pattern(params int[] path)
	{
		Path = new List<int>(path);
	}

	public static List<Token> Consume(Context context, PatternState state, List<System.Type> patterns)
	{
		return Parser.Consume(context, state, patterns);
	}

	public static bool Consume(PatternState state, out List<Token> consumed, params int[] path)
	{
		return Parser.TryConsume(state, out consumed, path);
	}

	public static bool Consume(PatternState state, out Token? consumed, int mask)
	{
		return Parser.TryConsume(state, out consumed, mask);
	}

	public static bool Try(Func<PatternState, bool> attempt, PatternState state)
	{
		var tokens = state.Tokens;
		var formatted = state.Formatted;
		var start = state.Start;
		var end = state.End;
		var min = state.Min;
		var max = state.Max;

		state.Tokens = new List<Token>(tokens);
		state.Formatted = new List<Token>(formatted);

		if (attempt(state))
		{
			return true;
		}

		state.Tokens = tokens;
		state.Formatted = formatted;
		state.Start = start;
		state.End = end;
		state.Min = min;
		state.Max = max;

		return false;
	}

	public static bool Try(PatternState state, Func<bool> attempt)
	{
		var tokens = state.Tokens;
		var formatted = state.Formatted;
		var start = state.Start;
		var end = state.End;
		var min = state.Min;
		var max = state.Max;

		state.Tokens = new List<Token>(tokens);
		state.Formatted = new List<Token>(formatted);

		if (attempt())
		{
			return true;
		}

		state.Tokens = tokens;
		state.Formatted = formatted;
		state.Start = start;
		state.End = end;
		state.Min = min;
		state.Max = max;

		return false;
	}

	public abstract int GetPriority(List<Token> tokens);
	public abstract bool Passes(Context context, PatternState state, List<Token> tokens);
	public abstract Node? Build(Context context, List<Token> tokens);

	public virtual int GetStart()
	{
		return 0;
	}

	public virtual int GetEnd()
	{
		return -1;
	}

	public List<int> GetPath()
	{
		return new List<int>(Path);
	}
}