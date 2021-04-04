using System;
using System.Collections.Generic;
using System.Linq;

public class ExtensionFunctionNode : Node, IResolvable
{
	public Type Destination { get; private set; }
	public FunctionToken Descriptor { get; private set; }
	public List<string> TemplateParameters { get; private set; }
	public List<Token> Body { get; private set; }

	public ExtensionFunctionNode(Type destination, FunctionToken descriptor, List<Token> body, Position position)
	{
		Destination = destination;
		Descriptor = descriptor;
		TemplateParameters = new List<string>();
		Body = body;
		Position = position;
		Instance = NodeType.EXTENSION_FUNCTION;
	}

	public ExtensionFunctionNode(Type destination, FunctionToken descriptor, List<string> template_parameters, List<Token> body, Position position)
	{
		Destination = destination;
		Descriptor = descriptor;
		TemplateParameters = template_parameters;
		Body = body;
		Position = position;
		Instance = NodeType.EXTENSION_FUNCTION;
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

		var function = (Function?)null;

		if (TemplateParameters.Any())
		{
			function = new TemplateFunction(Destination, Modifier.DEFAULT, Descriptor.Name, TemplateParameters) { Position = Position };
			function.Blueprint.AddRange(new[] { Descriptor, (Token)new ContentToken(Body) { Type = ParenthesisType.CURLY_BRACKETS } });
		}
		else
		{
			function = new Function(Destination, Modifier.DEFAULT, Descriptor.Name, Body) { Position = Position };
		}

		function.Parameters.AddRange(Descriptor.GetParameters(function));
			
		Destination.Declare(function);

		return new FunctionDefinitionNode(function, Position);
	}

	public Status GetStatus()
	{
		return Status.Error(Position!, $"Could not resolve the destination '{Destination}' of the extension function");
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Destination, Descriptor);
	}
}