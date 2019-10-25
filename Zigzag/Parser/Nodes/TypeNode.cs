using System.Collections.Generic;

public class TypeNode : Node, Contextable
{
	public Type Type { get; private set; }
	private List<Token> Body { get; set; }

	public TypeNode(Type type) : this(type, new List<Token>()) { }

	public TypeNode(Type type, List<Token> body)
	{
		Type = type;
		Body = body;
	}

	public void Parse()
	{
		Parser.Parse(this, Type, Body, Parser.MEMBERS);
		Body.Clear();
	}

	public Type GetContext()
	{
		return Type;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.TYPE_NODE;
	}
}
