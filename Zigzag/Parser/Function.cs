using System.Collections.Generic;
using System.Linq;

public class Function : Context
{
	public string Identifier => Index != -1 ? $"function_{Name}_{Index}" : $"function_{Name}";

	public int Modifiers { get; set; }
	public int Index { get; set; } = -1;

	public new List<Variable> Variables => base.Variables.Values.Concat(Parameters).ToList();
	public List<Variable> Parameters { get; private set; } = new List<Variable>();
	public List<Type> ParameterTypes => Parameters.Select(p => p.Type).ToList();
	public List<Variable> Locals => base.Variables.Values.ToList();
	public int LocalMemorySize => Variables.Where(v => v.Category == VariableCategory.LOCAL).Select(v => v.Type.Size).Sum();
	public List<Node> References { get; private set; } = new List<Node>();
	public Type ReturnType { get; set; }

	public bool IsStatic => Flag.Has(Modifiers, AccessModifier.STATIC);
	public new bool IsGlobal => GetTypeParent() == null;
	public bool IsMember => !IsGlobal;
	
	public Function(Context context, string name, int modifiers, Type result)
	{
		Name = name;
		Modifiers = modifiers;
		ReturnType = result;

		Link(context);
		context.Declare(this);
	}
	
	public Function(Context context, int modifiers)
	{
		Modifiers = modifiers;
		Link(context);
	}
	
	public override bool IsLocalVariableDeclared(string name)
	{
		return Parameters.Any(p => p.Name == name) || base.IsLocalVariableDeclared(name);
	}

	public override bool IsVariableDeclared(string name)
	{
		return Parameters.Any(p => p.Name == name) || base.IsVariableDeclared(name);
	}
	
	public override Variable GetVariable(string name)
	{
		if (Parameters.Any(p => p.Name == name))
		{
			return Parameters.Find(p => p.Name == name);
		}

		return base.GetVariable(name);
	}
	
	public Function SetParameters(Node node)
	{
		VariableNode parameter = (VariableNode)node.First;

		while (parameter != null)
		{
			Variable variable = parameter.Variable;
			variable.Category = VariableCategory.PARAMETER;

			Parameters.Add(variable);

			parameter = (VariableNode)parameter.Next;
		}

		return this;
	}

	public Function SetParameters(params Variable[] variables)
	{
		foreach (Variable parameter in variables)
		{
			parameter.Category = VariableCategory.PARAMETER;
			Parameters.Add(parameter);
		}

		return this;
	}
}