using System.Collections.Generic;

public class FunctionNode : Node, Contextable
{
	public Function Function { get; private set; }
	public List<Token> Tokens { get; private set; }

	public Node Parameters => First;
	public Node Body => Last;

	public FunctionNode(Function function) : this(function, new List<Token>()) { }

	public FunctionNode(Function function, List<Token> body)
	{
		Function = function;
		Function.References.Add(this);
		Tokens = body;
	}

	public void Parse()
	{
		Node node = Parser.Parse(Function, Tokens, Parser.MIN_PRIORITY, Parser.MEMBERS - 1);
		Add(node);

		Tokens.Clear();
	}

	public FunctionNode SetParameters(Node parameters)
	{
		Node parameter = parameters.First;

		while (parameter != null)
		{
			Node next = parameter.Next;
			Add(parameter);
			parameter = next;
		}

		return this;
	}

	public Type GetContext()
	{
		return Function.ReturnType;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.FUNCTION_NODE;
	}
}
