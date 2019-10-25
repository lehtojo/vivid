using System;

public class UnresolvedType : Type, Resolvable
{
	private Resolvable Resolvable;

	public UnresolvedType(Context context, string name) : base(context)
	{
		Resolvable = new UnresolvedIdentifier(name);
	}

	public UnresolvedType(Context context, Resolvable resolvable) : base(context)
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