using System;
using System.Collections.Generic;

public class FunctionNode : Node
{
	public FunctionImplementation Function { get; private set; }
	public List<Token> Tokens { get; private set; }

	public Node Parameters => this;
	public Node Body => Last!;

	public FunctionNode(FunctionImplementation function, Position? position = null)
	{
		Function = function;
		Function.References.Add(this);
		Position = position;
		Tokens = new List<Token>();
		Instance = NodeType.FUNCTION;
	}

	public FunctionNode SetArguments(Node arguments)
	{
		foreach (var argument in arguments) Add(argument);
		return this;
	}

	public override Type? TryGetType()
	{
		return Function.ReturnType;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Function.Identity);
	}

	public override string ToString() => $"Call {Function}";
}
