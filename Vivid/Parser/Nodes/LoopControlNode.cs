using System;
using System.Collections.Generic;

public class LoopControlNode : Node, IResolvable
{
	public Keyword Instruction { get; private set; }
	public Condition? Condition { get; set; }
	public LoopNode? Loop { get; set; }

	public LoopControlNode(Keyword instruction, Position? position = null)
	{
		Instruction = instruction;
		Position = position;
		Instance = NodeType.LOOP_CONTROL;
	}

	public LoopControlNode(Keyword instruction, Condition? condition, LoopNode? loop, Position? position)
	{
		Instruction = instruction;
		Condition = condition;
		Loop = loop;
		Position = position;
		Instance = NodeType.LOOP_CONTROL;
	}

	public Node? Resolve(Context context)
	{
		// If the loop has been found already, it means this node is resolved
		if (Loop != null) return null;

		// Try to find the parent loop
		Loop = (LoopNode?)FindParent(i => i.Is(NodeType.LOOP));
		if (Loop == null) return null;

		// Continue nodes must execute the action of their parent loops
		if (Instruction != Keywords.CONTINUE) return null;

		// Copy the action node if it is present and it is not empty
		if (Loop.IsForeverLoop || Loop.Action.IsEmpty) return null;
		
		// Execute the action first then the continue
		var result = new InlineNode();
		Loop.Action.ForEach(i => result.Add(i.Clone()));

		result.Add(new LoopControlNode(Instruction, Condition, Loop, Position));

		return result;
	}

	public Status GetStatus()
	{
		if (Loop != null) return Status.OK;
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