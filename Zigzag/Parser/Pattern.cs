using System.Collections.Generic;
using System;

public class PatternState
{
	public List<Token> Tokens { get; }

	public int Start { get; }
	public int End { get; set; }

	public int Min { get; }
	public int Max { get; }

	public PatternState(List<Token> tokens, int start, int end, int min, int max)
	{
		Tokens = tokens;
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

	public static bool TryConsume(PatternState state, out List<Token> consumed, params int[] path)
	{
		return Parser.TryConsume(state, out consumed, path);
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

public enum ConsumptionStatus
{
	DEFAULT,
	CONSUMING,
	CONSUMED
}

public class ConsumptionState
{
	private List<System.Type> _Whitelist { get; set; } = new List<System.Type>();
	public List<System.Type> Whitelist => new List<System.Type>(_Whitelist);

	public ConsumptionStatus Status { get; set; } = ConsumptionStatus.DEFAULT;
	public bool IsConsumed => Status == ConsumptionStatus.CONSUMED;
	public bool IsConsuming => Status == ConsumptionStatus.CONSUMING;

	public void Consume(List<System.Type> whitelist)
	{
		Status = ConsumptionStatus.CONSUMING;
		_Whitelist = whitelist;

		if (_Whitelist.Exists(p => !p.IsSubclassOf(typeof(Pattern))))
		{
			throw new ArgumentException("Tried to whitelist a non-pattern type");
		}
	}
}

public abstract class ConsumingPattern : Pattern
{
	public ConsumingPattern(params int[] path) : base(path) {}

	public abstract Node? Build(Context context, List<Token> tokens, ConsumptionState state);

	public sealed override Node? Build(Context context, List<Token> tokens)
	{
		throw new ApplicationException("Standard build method was executed on a consuming pattern");
	}
} 