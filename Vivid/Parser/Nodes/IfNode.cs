using System;
using System.Collections.Generic;

public class IfNode : Node, IResolvable
{
	public Node? Successor => (Next?.Is(NodeType.ELSE_IF, NodeType.ELSE) ?? false) ? Next : null;
	public Node? Predecessor => (Is(NodeType.ELSE_IF) && (Previous?.Is(NodeType.IF, NodeType.ELSE_IF) ?? false)) ? Previous : null;

	public Node Condition => Common.FindCondition(First!);
	public ScopeNode Body => Last!.To<ScopeNode>();

	public IfNode(Context context, Node condition, Node body, Position? start, Position? end)
	{
		Position = start;
		Instance = NodeType.IF;

		Add(new Node());
		Add(new ScopeNode(context, start, end));

		body.ForEach(i => Body.Add(i));

		First!.Add(condition);
	}

	public IfNode()
	{
		Instance = NodeType.IF;
	}

	/// <summary>
	/// Returns the nodes which are executed during condition step except the actual condition.
	/// NOTE: Use this function only for building since this function returns copies of the executed nodes
	/// </summary>
	public Node GetConditionInitialization()
	{
		// Clone all the nodes under the condition step
		var node = First!.Clone();
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
	public Node GetConditionStep()
	{
		return First!;
	}

	public List<Node> GetSuccessors()
	{
		var successors = new List<Node>();
		var iterator = Successor;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.ELSE_IF))
			{
				successors.Add(iterator);
				iterator = iterator.To<ElseIfNode>().Successor;
			}
			else
			{
				successors.Add(iterator);
				break;
			}
		}

		return successors;
	}

	public List<Node> GetBranches()
	{
		var branches = new List<Node>() { this };

		if (Successor == null)
		{
			return branches;
		}

		if (Successor.Is(NodeType.ELSE_IF))
		{
			branches.AddRange(Successor.To<ElseIfNode>().GetBranches());
		}
		else
		{
			branches.Add(Successor);
		}

		return branches;
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Condition);
		Resolver.Resolve(Body.Context, Body);

		if (Successor != null)
		{
			Resolver.Resolve(context, Successor);
		}

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Body.Context.Identity);
	}
}