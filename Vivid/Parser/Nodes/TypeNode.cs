using System;
using System.Collections.Generic;
using System.Linq;

public class TypeNode : Node, IContext
{
	public Type Type { get; private set; }
	public bool IsDefinition { get; set; } = false;

	private List<Token> Body { get; set; }

	public TypeNode(Type type)
	{
		Type = type;
		Body = new List<Token>();
		Instance = NodeType.TYPE;
	}

	public TypeNode(Type type, Position? position)
	{
		Type = type;
		Body = new List<Token>();
		Position = position;
		Instance = NodeType.TYPE;
	}

	public TypeNode(Type type, List<Token> body, Position? position)
	{
		Type = type;
		Body = body;
		Position = position;
		IsDefinition = true;
		Instance = NodeType.TYPE;
	}

	public void Parse()
	{
		Type.AddRuntimeConfiguration();

		Parser.Parse(this, Type, Body);
		Body.Clear();

		// Find all expressions which represent type initialization
		Type.Initialization = FindTop(i => i.Is(Operators.ASSIGN)).Cast<OperatorNode>().ToArray();
	}

	public override Type? TryGetType()
	{
		return Type;
	}

	public override bool Equals(object? other)
	{
		return other is TypeNode node &&
				base.Equals(other) &&
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
