public class CompilesNode : Node, IType
{
    public new Type GetType()
    {
        return Types.BOOL;
    }

    public override NodeType GetNodeType()
    {
        return NodeType.COMPILES;
    }
}