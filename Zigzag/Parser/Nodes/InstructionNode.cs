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
}