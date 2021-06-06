using System;
using System.Collections.Generic;

public class LoopControlNode : Node, IResolvable
{
	public Keyword Instruction { get; private set; }
	public Condition? Condition { get; set; }
	public LoopNode? Loop => (LoopNode?)FindParent(NodeType.LOOP);
	public bool Finished { get; set; } = false;

	public LoopControlNode(Keyword instruction, Position? position = null)
	{
		Instruction = instruction;
		Position = position;
		Instance = NodeType.LOOP_CONTROL;

		if (Instruction != Keywords.CONTINUE) { Finished = true; }
	}

	public LoopControlNode(Keyword instruction, Condition? condition, Position? position, bool finished = false)
	{
		Instruction = instruction;
		Condition = condition;
		Finished = finished;
		Position = position;
		Instance = NodeType.LOOP_CONTROL;
	}

	public Node? Resolve(Context context)
	{
		if (Finished) return null;

		// Try to find the parent loop
		var loop = Loop;
		if (loop == null) return null;

		// Continue nodes must execute the action of their parent loops
		if (Instruction != Keywords.CONTINUE) return null;

		// Copy the action node if it is present and it is not empty
		if (loop.IsForeverLoop || loop.Action.IsEmpty)
		{
			Finished = true;
			return null;
		}
		
		// Execute the action first then the continue
		var result = new InlineNode();
		loop.Action.ForEach(i => result.Add(i.Clone()));

		result.Add(new LoopControlNode(Instruction, Condition, Position, true));

		return result;
	}

	public Status GetStatus()
	{
		if (Finished && Loop != null) return Status.OK;
		return Status.Error($"Keyword '{Instruction.Identifier}' must be used inside a loop");
	}

	public override bool Equals(object? other)
	{
		return other is LoopControlNode node &&
				base.Equals(other) &&
				EqualityComparer<Keyword>.Default.Equals(Instruction, node.Instruction);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Instruction);
		return hash.ToHashCode();
	}
}