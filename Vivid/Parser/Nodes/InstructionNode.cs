using System;
using System.Collections.Generic;

public class InstructionNode : Node
{
	public Keyword Instruction { get; private set; }

	public InstructionNode(Keyword instruction)
	{
		Instruction = instruction;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INSTRUCTION;
	}

	public override bool Equals(object? other)
	{
		return other is InstructionNode node &&
			   base.Equals(other) &&
			   EqualityComparer<Keyword>.Default.Equals(Instruction, node.Instruction);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Instruction);
	}
}