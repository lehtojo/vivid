using System.Collections.Generic;

public class ExtensionFunctionNode : Node, IResolvable
{
	public Type Destination { get; private set; }
	public FunctionToken Descriptor { get; private set; }
	public List<Token> Body { get; private set; }

	public ExtensionFunctionNode(Type destination, FunctionToken descriptor, List<Token> body, Position position)
	{
		Destination = destination;
		Descriptor = descriptor;
		Body = body;
		Position = position;
	}

	public Node? Resolve(Context context)
	{
		if (Destination.IsUnresolved)
		{
			var destination = Resolver.Resolve(context, Destination);

			if (destination == null)
			{
				return null;
			}

			Destination = destination;
		}

		var function = new Function(Destination, AccessModifier.PUBLIC, Descriptor.Name, Body)
		{
			Position = Position
		};

		function.Parameters.AddRange(Descriptor.GetParameters(function));
		Destination.Declare(function);

		return new FunctionDefinitionNode(function, Position);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.EXTENSION_FUNCTION;
	}

	public Status GetStatus()
	{
		return Status.Error(Position!, $"Could not resolve the destination '${Destination}' of the extension function");
	}
}