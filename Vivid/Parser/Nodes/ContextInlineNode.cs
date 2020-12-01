public class ContextInlineNode : InlineNode, IContext
{
	public Context Context { get; private set; }

	public ContextInlineNode(Context parent)
	{
		Context = new Context(parent);
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