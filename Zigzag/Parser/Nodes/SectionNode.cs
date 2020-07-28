public class SectionNode : Node
{
   public int Modifiers { get; private set; }

   public SectionNode(int modifiers)
   {
      Modifiers = modifiers;
   }

   public override NodeType GetNodeType()
   {
      return NodeType.SECTION_NODE;
   }
}