using System;
using System.Collections.Generic;
using System.Linq;

public class ExtensionFunctionNode : Node, IResolvable
{
	public Type Destination { get; private set; }
	public FunctionToken Descriptor { get; private set; }
	public List<string> TemplateParameters { get; private set; }
	public List<Token> ReturnTypeTokens { get; private set; }
	public List<Token> Body { get; private set; }
	public Position? Start => Position;
	public Position? End { get; private set; }

	public ExtensionFunctionNode(Type destination, FunctionToken descriptor, List<Token> return_type_tokens, List<Token> body, Position? start, Position? end)
	{
		Destination = destination;
		Descriptor = descriptor;
		TemplateParameters = new List<string>();
		ReturnTypeTokens = return_type_tokens;
		Body = body;
		Instance = NodeType.EXTENSION_FUNCTION;
		Position = start;
		End = end;
	}

	public ExtensionFunctionNode(Type destination, FunctionToken descriptor, List<string> template_parameters, List<Token> return_type_tokens, List<Token> body, Position? start, Position? end)
	{
		Destination = destination;
		Descriptor = descriptor;
		TemplateParameters = template_parameters;
		ReturnTypeTokens = return_type_tokens;
		Body = body;
		Instance = NodeType.EXTENSION_FUNCTION;
		Position = start;
		End = end;
	}

	public Node? Resolve(Context context)
	{
		if (Destination.IsUnresolved)
		{
			var destination = Resolver.Resolve(context, Destination);
			if (destination == null) return null;

			Destination = destination;
		}

		var function = (Function?)null;

		if (TemplateParameters.Any())
		{
			function = new TemplateFunction(Destination, Modifier.DEFAULT, Descriptor.Name, TemplateParameters, Descriptor.Parameters.Tokens, Start, End);
			function.To<TemplateFunction>().Initialize();
			function.Blueprint.Add(Descriptor);
			function.Blueprint.AddRange(ReturnTypeTokens);
			function.Blueprint.Add(new ParenthesisToken(Body) { Opening = ParenthesisType.CURLY_BRACKETS });
		}
		else
		{
			// Read the explicit return type if it is specified
			var return_type = (Type?)null;

			if (ReturnTypeTokens.Any())
			{
				return_type = Common.ReadType(context, ReturnTypeTokens, 1);
				if (return_type == null) return null;
			}

			function = new Function(Destination, Modifier.DEFAULT, Descriptor.Name, Body, Start, End);
			function.Parameters.AddRange(Descriptor.GetParameters(function));

			// Set the explicit return type if it is specified
			function.ReturnType = return_type;
		}

		// If the destination is a namespace, mark the function as a static function
		if (Destination.IsStatic)
		{
			function.Modifiers |= Modifier.STATIC;
		}

		Destination.Declare(function);

		return new FunctionDefinitionNode(function, Position);
	}

	public Status GetStatus()
	{
		return Status.Error(Position!, $"Can not resolve the destination '{Destination}' of the extension function");
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Destination, Descriptor);
	}
}