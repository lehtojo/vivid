using System;
using System.Collections.Generic;

public class PatternState
{
	public List<Token> Tokens { get; }
	public List<Token> Formatted { get; }

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

	public PatternState(List<Token> tokens)
	{
		Tokens = tokens;
		Formatted = new List<Token>();
		Start = 0;
		End = 0;
		Min = Parser.MIN_PRIORITY;
		Max = Parser.MAX_PRIORITY;
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

	public static bool Consume(PatternState state, int mask)
	{
		return Parser.TryConsume(state, out Token? _, mask);
	}

	public static bool Consume(PatternState state)
	{
		return Parser.TryConsume(state);
	}

	public static Token? Peek(PatternState state)
	{
		return state.Tokens.Count > state.End ? state.Tokens[state.End] : null;
	}

	public static bool Try(Func<PatternState, bool> attempt, PatternState state)
	{
		var tokens = new List<Token>(state.Tokens);
		var formatted = new List<Token>(state.Formatted);
		var start = state.Start;
		var end = state.End;
		var min = state.Min;
		var max = state.Max;

		if (attempt(state))
		{
			return true;
		}

		state.Tokens.Clear();
		state.Formatted.Clear();

		state.Tokens.AddRange(tokens);
		state.Formatted.AddRange(formatted);

		state.Start = start;
		state.End = end;
		state.Min = min;
		state.Max = max;

		return false;
	}

	public static bool Try(PatternState state, Func<bool> attempt)
	{
		var tokens = new List<Token>(state.Tokens);
		var formatted = new List<Token>(state.Formatted);
		var start = state.Start;
		var end = state.End;
		var min = state.Min;
		var max = state.Max;

		if (attempt())
		{
			return true;
		}

		state.Tokens.Clear();
		state.Formatted.Clear();

		state.Tokens.AddRange(tokens);
		state.Formatted.AddRange(formatted);

		state.Start = start;
		state.End = end;
		state.Min = min;
		state.Max = max;

		return false;
	}

	public abstract int GetPriority(List<Token> tokens);
	public abstract bool Passes(Context context, PatternState state, List<Token> tokens);
	public abstract Node? Build(Context context, PatternState state, List<Token> tokens);

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