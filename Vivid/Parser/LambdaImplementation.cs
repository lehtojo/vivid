using System.Collections.Generic;
using System.Linq;

public class LambdaImplementation : FunctionImplementation
{
	public List<CapturedVariable> Captures { get; private set; } = new List<CapturedVariable>();
	public Type? Type { get; private set; }
	public Variable? Function { get; private set; }

	public LambdaImplementation(Lambda metadata, List<Parameter> parameters, Type? return_type, Context context)
	   : base(metadata, parameters, return_type, context)
	{
		Prefix = "_";
		Postfix = "_" + Postfix;
	}

	protected override void OnMangle(Mangle mangle)
	{
		mangle += $"_{Name}_";
		mangle += Parameters.Select(p => p.Type!);

		if (ReturnType != global::Types.UNIT)
		{
			mangle += "_r";
			mangle += ReturnType!;
		}
	}

	public void Seal()
	{
		if (Type == null || Type.Variables.Any())
		{
			return;
		}

		Self = new Variable(this, Type, VariableCategory.PARAMETER, Lambda.SELF_POINTER_IDENTIFIER, Modifier.DEFAULT)
		{
			IsSelfPointer = true
		};

		// Declare the function pointer as the first member
		Function = Type.DeclareHidden(global::Types.LINK, VariableCategory.MEMBER);

		// Change all captured variables into member variables so that they are retrieved using the self pointer of this lambda
		Captures.ForEach(c => c.Category = VariableCategory.MEMBER);

		// Remove all captured variables from the current context since they must be moved to the lambda object's context
		Captures.Select(c => c.Name).ForEach(n => Variables.Remove(n));

		// Move all the captured variables to the lambda object's type
		Captures.ForEach(c => Type.Declare(c));

		// Add the self pointer to all of the usages of the captured variables
		var usages = Node!.FindAll(i => i.Is(NodeType.VARIABLE) && Captures.Contains(i.To<VariableNode>().Variable));

		foreach (var usage in usages)
		{
			usage.Replace(new LinkNode(new VariableNode(Self), usage.Clone()));
		}

		// Align the member variables
		Aligner.Align(Type);
	}

	public override bool IsVariableDeclared(string name)
	{
		return IsLocalVariableDeclared(name) || GetVariable(name) != null;
	}

	public override Variable? GetVariable(string name)
	{
		if (IsLocalVariableDeclared(name))
		{
			return base.GetVariable(name);
		}

		// If the variable is declared outside of this implementation, it may need to be captured
		var variable = base.GetVariable(name);

		if (variable == null)
		{
			return null;
		}

		// The variable can be captured only if it is a local variable or a parameter and it is resolved
		if (variable.IsPredictable && variable.IsResolved && !variable.IsConstant)
		{
			var captured = CapturedVariable.Create(this, variable);
			Captures.Add(captured);

			return captured;
		}

		return variable.IsMember || variable.IsConstant ? variable : null;
	}

	/// <summary>
	/// Implements the function using the given blueprint
	/// </summary>
	/// <param name="blueprint">Tokens from which to implement the function</param>
	public override void Implement(List<Token> blueprint)
	{
		Type = new Type(this, string.Empty, Modifier.DEFAULT);

		Type.AddConstructor(Constructor.Empty(Type, Metadata.Position!));
		Type.AddDestructor(Destructor.Empty(Type, Metadata.Position!));

		Node = new ImplementationNode(this, Metadata.Position);

		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
	}

	public override Variable? GetSelfPointer()
	{
		return Self ?? GetVariable(global::Function.SELF_POINTER_IDENTIFIER);
	}
}