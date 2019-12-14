using System.Collections.Generic;

public abstract class Pattern
{
	private List<int> Path { get; set; }

	public Pattern(params int[] path)
	{
		Path = new List<int>(path);
	}

	public abstract int GetPriority(List<Token> tokens);
	public abstract bool Passes(Context context, List<Token> tokens);
	public abstract Node Build(Context context, List<Token> tokens);

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
