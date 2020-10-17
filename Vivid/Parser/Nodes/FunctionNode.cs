using System.Collections.Generic;
using System;

public class FunctionNode : Node, IType
{
	public FunctionImplementation Function { get; private set; }
	public List<Token> Tokens { get; private set; }

	public Node Parameters => this;
	public Node Body => Last!;

	public FunctionNode(FunctionImplementation function)
	{
		Function = function;
		Function.References.Add(this);
		Tokens = new List<Token>();
	}

	public FunctionNode SetParameters(Node parameters)
	{
		var parameter = parameters.First;

		if (Function.Parameters.Count != parameters.Count())
		{
			if (Function.Parameters.Count > parameters.Count())
			{
				throw new ApplicationException("Tried to build a function call with too few arguments");
			}

			for (var i = 0; i < parameters.Count() - Function.Parameters.Count; i++)
			{
				parameter = parameter!.Next;
			}
		}

		while (parameter != null)
		{
			var next = parameter.Next;
			Add(parameter);
			parameter = next;
		}

		return this;
	}

	public new Type? GetType()
	{
		return Function.ReturnType;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.FUNCTION;
	}
}
