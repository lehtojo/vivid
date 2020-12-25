public class ContextInlineNode : InlineNode, IContext
{
	public Context Context { get; private set; }

	public ContextInlineNode(Context context, Position? position = null) : base(position)
	{
		Context = context;
	}

	public Context GetContext()
	{
		return Context;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}
}