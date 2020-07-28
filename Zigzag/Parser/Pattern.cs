using System.Collections.Generic;
using System;

public abstract class Pattern
{
	private List<int> Path { get; set; }

	public Pattern(params int[] path)
	{
		Path = new List<int>(path);
	}

	public abstract int GetPriority(List<Token> tokens);
	public abstract bool Passes(Context context, List<Token> tokens);
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
			throw new ArgumentException("Tried to blacklist a non-pattern type");
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