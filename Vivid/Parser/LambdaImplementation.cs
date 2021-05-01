using System.Collections.Generic;
using System.Linq;

public class LambdaImplementation : FunctionImplementation
{
	public List<CapturedVariable> Captures { get; private set; } = new List<CapturedVariable>();
	public Type? Type { get; private set; }
	public Variable? Function { get; private set; }

	public LambdaImplementation(Lambda metadata, List<Parameter> parameters, Type? return_type, Context context) : base(metadata, parameters, return_type, context) {}

	public override void OnMangle(Mangle mangle)
	{
		Parent!.OnMangle(mangle);

		mangle += $"_{Name}_";
		mangle += Parameters.Select(i => i.Type!);

		if (!Primitives.IsPrimitive(ReturnType, Primitives.UNIT))
		{
			mangle += Mangle.PARAMETERS_END;
			mangle += Mangle.START_RETURN_TYPE_COMMAND;
			mangle += ReturnType!;
		}
	}

	public void Seal()
	{
		// 1. If the type is not created, it means that this lambda is not used, therefore this lambda can be skipped
		// 2. If the function is already created, this lambda is sealed
		if (Type == null || Function != null)
		{
			return;
		}

		Self = new Variable(this, Type, VariableCategory.PARAMETER, Lambda.SELF_POINTER_IDENTIFIER, Modifier.DEFAULT)
		{
			IsSelfPointer = true
		};

		// Declare the function pointer as the first member
		Function = Type.DeclareHidden(new Link(), VariableCategory.MEMBER);

		// Change all captured variables into member variables so that they are retrieved using the self pointer of this lambda
		Captures.ForEach(i => i.Category = VariableCategory.MEMBER);

		// Remove all captured variables from the current context since they must be moved to the lambda object's context
		Captures.Select(i => i.Name).ForEach(i => Variables.Remove(i));

		// Move all the captured variables to the lambda object's type
		Captures.ForEach(i => Type.Declare(i));

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
	public override void Implement(List<Token> blueprint)
	{
		Type = new Type(GetRoot(), Identity, Modifier.DEFAULT);
		Type.AddRuntimeConfiguration();

		Type.AddConstructor(Constructor.Empty(Type, Metadata.Start, Metadata.End));
		Type.AddDestructor(Destructor.Empty(Type, Metadata.Start, Metadata.End));

		Node = new ScopeNode(this, Metadata.Start, Metadata.End);

		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
	}

	public override Variable? GetSelfPointer()
	{
		return Self ?? GetVariable(global::Function.SELF_POINTER_IDENTIFIER);
	}
}