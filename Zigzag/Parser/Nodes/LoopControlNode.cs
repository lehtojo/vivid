using System;
using System.Collections.Generic;

public class LoopControlNode : Node
{
	public Keyword Instruction { get; private set; }
    public LoopNode? Loop => (LoopNode?)FindParent(p => p.Is(NodeType.LOOP_NODE));

	public LoopControlNode(Keyword instruction)
	{
		Instruction = instruction;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP_CONTROL;
	}

    public override bool Equals(object? obj)
    {
        return obj is LoopControlNode node &&
               base.Equals(obj) &&
               EqualityComparer<Keyword>.Default.Equals(Instruction, node.Instruction);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Instruction);
        return hash.ToHashCode();
    }
}