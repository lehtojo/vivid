using System;
using System.Collections.Generic;

public class TypeNode : Node, IType, IContext
{
	public Type Type { get; private set; }
	public bool IsDefinition { get; private set; } = false;

	private List<Token> Body { get; set; }

	public TypeNode(Type type)
	{
		Type = type;
		Body = new List<Token>();
	}

	public TypeNode(Type type, List<Token> body)
	{
		Type = type;
		Body = body;
		IsDefinition = true;
	}

	public void Parse()
	{
		Parser.Parse(this, Type, Body);
		Body.Clear();

		// Find all expressions which represent type initialization
		var expressions = FindChildren(n => !n.Is(NodeType.FUNCTION_DEFINITION_NODE) && !n.Is(NodeType.VARIABLE_NODE));

		// Pack all expressions under an initialization node
		var initialization = new Node();
		expressions.ForEach(e => initialization.Add(e));

		// Save the initialization for other constructors
		Type.Initialization = initialization;
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
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Type.Name);
		return hash.ToHashCode();
	}

	public void SetContext(Context context)
	{
		throw new InvalidOperationException("Replacing context of a type node is not allowed");
	}

   public Context GetContext()
   {
		return Type;
   }
}
