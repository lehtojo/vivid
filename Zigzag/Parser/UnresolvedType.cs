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
		var resolved = Resolvable.Resolve(context);

		if (resolved is IType type)
		{
			return new TypeNode(type.GetType());
		}

		throw new Exception("Couldn't resolve type");
	}

	public Status GetStatus()
	{
		return Resolvable.GetStatus();
	}
}