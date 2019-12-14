using System.Collections.Generic;

public class FunctionNode : Node, IType
{
	public FunctionImplementation Function { get; private set; }
	public List<Token> Tokens { get; private set; }

	public bool IsDefinition { get; private set; } = false;

	public Node Parameters => First;
	public Node Body => Last;

	public FunctionNode(FunctionImplementation function)
	{
		Function = function;
		Function.References.Add(this);
		Tokens = new List<Token>();
	}

	public FunctionNode SetParameters(Node parameters)
	{
		var parameter = parameters.First;

		while (parameter != null)
		{
			var next = parameter.Next;
			Add(parameter);
			parameter = next;
		}

		return this;
	}

	public Type GetType()
	{
		return Function.ReturnType;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.FUNCTION_NODE;
	}
}
