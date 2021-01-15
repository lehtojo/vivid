using System.Collections.Generic;
using System.Linq;

public class LoopNode : Node, IResolvable, IContext
{
	public Context Context { get; private set; }

	public Node Steps => First!;
	public ContextNode Body => Last!.To<ContextNode>();

	public Node Initialization => Steps.First!;
	public Node Condition => Initialization!.Next!.Last!;
	public Node Action => Steps.Last!;

	public Label? Start { get; set; } = null;
	public Label? Exit { get; set; } = null;
	public string? Identifier { get; set; }

	public bool IsForeverLoop => First == Last;

	public LoopNode(Context context, Node? steps, ContextNode body, Position? position = null)
	{
		Context = context;
		Position = position;

		if (steps != null)
		{
			Add(steps);
		}

		Add(body);
	}

	public IEnumerable<Node> GetConditionInitialization()
	{
		return Initialization.Next!.Where(i => i != Condition).ToArray();
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

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public Context GetContext()
	{
		return Context;
	}
}
