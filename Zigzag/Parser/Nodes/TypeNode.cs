using System;
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

	public override bool Equals(object? obj)
	{
		return obj is TypeNode node &&
				base.Equals(obj) &&
				EqualityComparer<Type>.Default.Equals(Type, node.Type);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Type.Name);
		return hash.ToHashCode();
	}
}
