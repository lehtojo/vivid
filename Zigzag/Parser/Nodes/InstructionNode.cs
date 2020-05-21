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
		return NodeType.INSTRUCTION_NODE;
	}

	public override bool Equals(object? obj)
	{
		return obj is InstructionNode node &&
			   base.Equals(obj) &&
			   EqualityComparer<Keyword>.Default.Equals(Instruction, node.Instruction);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Instruction);
	}
}