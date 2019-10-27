using System;

public class UnresolvedType : Type, IResolvable
{
	private IResolvable Resolvable;

	public UnresolvedType(Context context, string name) : base(context)
	{
		Resolvable = new UnresolvedIdentifier(name);
	}

	public UnresolvedType(Context context, IResolvable resolvable) : base(context)
	{
		Resolvable = resolvable;
	}

	public Node Resolve(Context context)
	{
		Node resolved = Resolvable.Resolve(context);

		if (resolved is Contextable contextable)
		{
			return new TypeNode(contextable.GetContext());
		}

		throw new Exception("Couldn't resolve type");
	}
}