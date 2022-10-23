using System.Collections.Generic;

public abstract class Pattern
{
	public List<int> Path { get; set; } = new();
	public int Priority { get; set; }
	public long Id { get; set; }
	public bool IsConsumable { get; set; } = true;

	public Pattern(params int[] path)
	{
		Path = new List<int>(path);
	}

	public abstract bool Passes(Context context, ParserState state, List<Token> tokens, int priority);
	public abstract Node? Build(Context context, ParserState state, List<Token> tokens);
}