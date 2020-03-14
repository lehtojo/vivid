using System.Collections.Generic;

public class TypeNode : Node, IType
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
		Parser.Parse(this, Type, Body);
		Body.Clear();
	}

	public new Type? GetType()
	{
		return Type;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.TYPE_NODE;
	}
}
