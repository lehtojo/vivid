public class UnresolvedType : Type, IResolvable
{
	private IResolvable Resolvable { get; }

	public UnresolvedType(Context context, string name) : base(context)
	{
		Resolvable = new UnresolvedIdentifier(name);
	}

	public UnresolvedType(Context context, IResolvable resolvable) : base(context)
	{
		Resolvable = resolvable;
	}

	public Node? Resolve(Context context)
	{
		if (Resolvable.Resolve(context) is IType resolved && resolved.GetType() != null)
		{
			return new TypeNode(resolved.GetType()!);
		}

		return null;
	}

	public Type? TryResolveType(Context context)
	{
		return Resolve(context)?.GetType();
	}

	public Status GetStatus()
	{
		return Resolvable.GetStatus();
	}
}