using System;
using System.Collections.Generic;
using System.Linq;

public class TypeNode : Node, IScope
{
	public Type Type { get; private set; }
	public bool IsDefinition { get; set; } = false;

	private List<Token> Blueprint { get; set; }

	public TypeNode(Type type)
	{
		Type = type;
		Blueprint = new List<Token>();
		Instance = NodeType.TYPE;
	}

	public TypeNode(Type type, Position? position)
	{
		Type = type;
		Blueprint = new List<Token>();
		Position = position;
		Instance = NodeType.TYPE;
	}

	public TypeNode(Type type, List<Token> body, Position? position)
	{
		Type = type;
		Blueprint = body;
		Position = position;
		IsDefinition = true;
		Instance = NodeType.TYPE;
	}

	public void Parse()
	{
		// Static types can not be constructed
		if (!Type.IsStatic)
		{
			Type.AddRuntimeConfiguration();
		}

		// Create the body of the type
		Parser.Parse(this, Type, new List<Token>(Blueprint));

		// Apply the static modifier to the parsed functions and variables
		if (Type.IsStatic)
		{
			Type.Functions.Values.SelectMany(i => i.Overloads).ForEach(i => i.Modifiers |= Modifier.STATIC);
			Type.Variables.Values.ForEach(i => i.Modifiers |= Modifier.STATIC);
		}

		// Parse all the subtypes
		FindAll(i => i.Is(NodeType.TYPE)).Cast<TypeNode>().Where(i => i.IsDefinition).ForEach(i => i.Parse());

		// Find all expressions which represent type initialization
		var expressions = FindTop(i => i.Is(Operators.ASSIGN)).Cast<OperatorNode>().ToArray();
		Type.Initialization = Type.Initialization.Concat(expressions).ToArray();
	}

	public override Type TryGetType()
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
		hash.Add(Type.Identity);
		return hash.ToHashCode();
	}

	public void SetContext(Context context)
	{
		throw new InvalidOperationException("Replacing the context of a type node is not allowed");
	}

	public Context GetContext()
	{
		return Type;
	}
}
