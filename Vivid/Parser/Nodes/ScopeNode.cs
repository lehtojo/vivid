using System;

public class ScopeNode : Node, IResolvable, IScope
{
	public Context Context { get; private set; }
	public Position? End { get; }

	public ScopeNode(Context context, Position? start, Position? end)
	{
		Context = context;
		Instance = NodeType.SCOPE;
		Position = start;
		End = end;
	}

	public Context GetContext()
	{
		return Context;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public Node? Resolve(Context context)
	{
		foreach (var iterator in this)
		{
			Resolver.Resolve(Context, iterator);
		}

		return null;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public override bool Equals(object? other)
	{
		return other is ScopeNode node && Context.Identity == node.Context.Identity;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Context.Identity);
	}
}