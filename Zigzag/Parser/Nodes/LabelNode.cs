public class LabelNode : Node 
{
    public Label Label { get; private set; }

    public LabelNode(Label label) 
	{
        Label = label;
    }
	
    public override NodeType GetNodeType() 
	{
        return NodeType.LABEL_NODE;
    }
}