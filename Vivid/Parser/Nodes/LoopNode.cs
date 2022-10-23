using System;

public class LoopNode : Node, IResolvable, IScope
{
	public Context Context { get; private set; }

	public Node Steps => First!;
	public ScopeNode Body => Last!.To<ScopeNode>();

	public Node Initialization => Steps.First!;
	public Node Condition => Common.FindCondition(Initialization.Next!);
	public Node Action => Steps.Last!;

	public Label? Start { get; set; } = null;
	public Label? Exit { get; set; } = null;

	public bool IsForeverLoop => First == Last;

	public LoopNode(Context context, Node? steps, ScopeNode body, Position? position = null)
	{
		Context = context;
		Position = position;
		Instance = NodeType.LOOP;

		if (steps != null)
		{
			Add(steps);
		}

		Add(body);
	}

	/// <summary>
	/// Returns the nodes which are executed during condition step except the actual condition.
	/// NOTE: Use this function only for building since this function returns copies of the executed nodes
	/// </summary>
	public Node GetConditionInitialization()
	{
		// Clone all the nodes under the condition step
		var node = Initialization.Next!.Clone();
		node.Parent = this;

		// Remove the condition from the initialization since it is built separately
		if (!Common.FindCondition(node).Remove())
		{
			throw new ApplicationException("Could not remove the condition from the condition step initialization");
		}

		return node;
	}

	/// <summary>
	/// Returns all the nodes which are executed during the condition step
	/// </summary>
	public Node GetConditionContainer()
	{
		return Initialization.Next!;
	}

	public Node? Resolve(Context context)
	{
		if (!IsForeverLoop)
		{
			Resolver.Resolve(Context, Initialization);
			Resolver.Resolve(Context, Condition);
			Resolver.Resolve(Context, Action);
		}

		Resolver.Resolve(Body.Context, Body);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public Context GetContext()
	{
		return Context;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Context.Identity, Body.Context.Identity, IsForeverLoop);
	}

	public override string ToString() => "Loop";
}
