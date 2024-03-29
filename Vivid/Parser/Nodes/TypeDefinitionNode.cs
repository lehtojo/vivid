using System.Collections.Generic;
using System.Linq;
using System;

public class TypeDefinitionNode : Node, IScope
{
	public Type Type { get; set; }
	public List<Token> Blueprint { get; set; }

	public TypeDefinitionNode(Type type, List<Token> blueprint, Position position)
	{
		this.Instance = NodeType.TYPE_DEFINITION;
		this.Type = type;
		this.Blueprint = blueprint;
		this.Position = position;
	}

	public void Parse()
	{
		// Static types can not be constructed
		if (!Type.IsStatic && !Type.IsPlain) Type.AddRuntimeConfiguration();

		// Create the body of the type
		Parser.Parse(this, Type, new List<Token>(Blueprint));
		Blueprint.Clear();

		// Add all member initializations
		var assignments = FindTop(i => i.Is(Operators.ASSIGN)).ToList();

		// Remove all constant and static variable assignments
		for (var i = assignments.Count - 1; i >= 0; i--)
		{
			var assignment = assignments[i];
			var destination = assignment.Left;
			if (destination.Instance != NodeType.VARIABLE) continue;

			var variable = destination.To<VariableNode>().Variable;
			if (!variable.IsStatic && !variable.IsConstant) continue;

			assignments.RemoveAt(i);
		}

		Type.Initialization = assignments;

		// Add member initialization to the constructors that have been created before loading the member initializations
		foreach (var constructor in Type.Constructors.Overloads)
		{
			constructor.To<Constructor>().AddMemberInitializations();
		}
	}

	public void SetContext(Context context)
	{
		throw new InvalidOperationException("Replacing the context of a type definition node is not allowed");
	}

	public Context GetContext()
	{
		return Type;
	}

	public override bool Equals(object? other)
	{
		return other is TypeDefinitionNode node && Instance == node.Instance && Type == node.To<TypeDefinitionNode>().Type;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Type);
	}

	public override string ToString()
	{
		return "Type Definition " + Type.Name;
	}
}