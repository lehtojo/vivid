using System;

public class FunctionNode : Node
{
	public FunctionImplementation Function { get; private set; }
	public Node Parameters => this;

	public FunctionNode(FunctionImplementation function, Position? position = null)
	{
		Function = function;
		Function.Usages.Add(this);
		Position = position;
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
