using System;
using System.Collections.Generic;

public class CommandNode : Node, IResolvable
{
	public Keyword Instruction { get; private set; }
	public LoopNode? Container => (LoopNode?)FindParent(NodeType.LOOP);
	public bool Finished { get; set; } = false;

	public CommandNode(Keyword instruction, Position? position = null)
	{
		Instruction = instruction;
		Position = position;
		Instance = NodeType.COMMAND;

		if (Instruction != Keywords.CONTINUE) { Finished = true; }
	}

	public CommandNode(Keyword instruction, Position? position, bool finished = false)
	{
		Instruction = instruction;
		Finished = finished;
		Position = position;
		Instance = NodeType.COMMAND;
	}

	public Node? Resolve(Context context)
	{
		if (Finished) return null;

		// Try to find the parent loop
		var loop = Container;
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

		result.Add(new CommandNode(Instruction, Position, true));

		return result;
	}

	public Status GetStatus()
	{
		if (Finished && Container != null) return Status.OK;
		return new Status(Position, $"Keyword '{Instruction.Identifier}' must be used inside a loop");
	}

	public override bool Equals(object? other)
	{
		return other is CommandNode node &&
				base.Equals(other) &&
				EqualityComparer<Keyword>.Default.Equals(Instruction, node.Instruction);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Instruction);
	}
}