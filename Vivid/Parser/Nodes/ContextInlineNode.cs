using System;

public class ContextInlineNode : InlineNode, IScope
{
	public Context Context { get; private set; }

	public ContextInlineNode(Context context, Position? position = null) : base(position)
	{
		Context = context;
		IsContext = true;
		Instance = NodeType.INLINE;
	}

	public Context GetContext()
	{
		return Context;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Context.Identity);
	}

	public override string ToString() => "Context Inline";
}