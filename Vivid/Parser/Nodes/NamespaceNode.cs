using System.Collections.Generic;
using System.Linq;
using System;

public class NamespaceNode : Node
{
	public List<Token> Name { get; }
	public List<Token> Blueprint { get; }
	public bool IsParsed { get; private set; } = false;

	public NamespaceNode(List<Token> name, List<Token> blueprint)
	{
		Name = name;
		Blueprint = blueprint;
		Instance = NodeType.NAMESPACE;
	}

	/// <summary>
	/// Defines the actual namespace from the stored tokens.
	/// This does not create the body of the namespace.
	/// </summary>
	private Type CreateNamespace(Context context)
	{
		for (var i = 0; i < Name.Count; i += 2)
		{
			if (!Name[i].Is(TokenType.IDENTIFIER)) throw new ApplicationException("Invalid namespace tokens");

			var name = Name[i].To<IdentifierToken>().Value;
			var type = context.GetType(name);

			// Use the type if it was found and its parent is the current context
			if (type != null && ReferenceEquals(type.Parent, context))
			{
				context = type;
				continue;
			}

			context = new Type(context, name, Modifier.DEFAULT | Modifier.STATIC, Name.First().Position);
		}

		return (Type)context;
	}

	public void Parse(Context context)
	{
		if (IsParsed) return;
		IsParsed = true;

		// Define the actual namespace
		var result = CreateNamespace(context);

		// Create the body of the namespace
		Parser.Parse(this, result, new List<Token>(Blueprint));

		// Apply the static modifier to the parsed functions and variables
		result.Functions.Values.SelectMany(i => i.Overloads).ForEach(i => i.Modifiers |= Modifier.STATIC);
		result.Variables.Values.ForEach(i => i.Modifiers |= Modifier.STATIC);

		// Parse all the subtypes
		FindAll(NodeType.TYPE_DEFINITION).Cast<TypeDefinitionNode>().ForEach(i => i.Parse());

		// Parse all the subnamespaces
		FindAll(NodeType.NAMESPACE).Cast<NamespaceNode>().ForEach(i => i.Parse(result));
	}
}