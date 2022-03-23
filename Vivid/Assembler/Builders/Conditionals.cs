using System;
using System.Collections.Generic;
using System.Linq;

public static class Conditionals
{
	/// <summary>
	/// Builds the body of an if-statement or an else-if-statement
	/// </summary>
	private static Result BuildBody(Unit unit, ScopeNode body, List<Variable> active_variables)
	{
		var state = unit.GetState(unit.Position);
		Result? result;

		// Since this is a body of some statement is also has a scope
		using (var scope = new Scope(unit, body, active_variables))
		{
			// Merges all changes that happen in the scope with the outer scope
			var merge = new MergeScopeInstruction(unit, scope);

			// Build the body
			result = Builders.Build(unit, body);

			// Restore the state after the body
			unit.TryAppendPosition(body.End);
			unit.Append(merge);
		}

		unit.Set(state);

		return result;
	}

	/// <summary>
	/// Builds an if-statement or an else-if-statement
	/// </summary>
	private static Result Build(Unit unit, IfNode statement, Node condition, LabelInstruction end)
	{
		// Set the next label to be the end label if there is no successor since then there wont be any other comparisons
		var interphase = statement.Successor == null ? end.Label : unit.GetNextLabel();

		// Build the nodes around the actual condition by disabling the condition temporarily
		var instance = statement.Condition.Instance;
		statement.Condition.Instance = NodeType.DISABLED;

		Builders.Build(unit, statement.Left);

		statement.Condition.Instance = instance;

		var active_variables = Scope.GetAllActiveVariables(unit, statement);

		// Jump to the next label based on the comparison
		BuildCondition(unit, condition, interphase, active_variables);

		// Build the body of this if-statement
		var result = BuildBody(unit, statement.Body, active_variables);

		// If the body of the if-statement is executed it must skip the potential successors
		if (statement.Successor == null)
		{
			return result;
		}

		// Skip the next successor from this if-statement's body and add the interphase label
		unit.Append(new JumpInstruction(unit, end.Label));
		unit.Append(new LabelInstruction(unit, interphase));

		// Build the successor
		return Build(unit, statement.Successor, end);
	}

	private static Result Build(Unit unit, Node node, LabelInstruction end)
	{
		switch (node)
		{
			case IfNode x:
			{
				unit.TryAppendPosition(x);

				return Build(unit, x, x.Condition, end);
			}

			case ElseNode y:
			{
				unit.TryAppendPosition(y);

				var active_variables = Scope.GetAllActiveVariables(unit, node);
				var result = BuildBody(unit, y.Body, active_variables);

				return result;
			}

			default: throw new ApplicationException("Successor of an if-statement was not an else-if-statement or an else-statement");
		}
	}

	public static Result Start(Unit unit, IfNode node)
	{
		if (!Assembler.IsDebuggingEnabled && TryBuildBranchlessExecution(unit, node))
		{
			return new Result();
		}

		var branches = node.GetBranches().ToArray();
		var contexts = branches.Select(i => i is IfNode x ? x.Body.Context : i.To<ElseNode>().Body.Context).ToArray();

		Scope.Cache(unit, branches, contexts, node.GetParentContext());
		Scope.LoadConstants(unit, node);

		var end = new LabelInstruction(unit, unit.GetNextLabel());
		var result = Build(unit, node, end);
		unit.Append(end);

		return result;
	}

	public static void BuildCondition(Unit unit, Node condition, Label failure, List<Variable> active_variables)
	{
		// Load constants which might be edited inside the condition
		Scope.LoadConstants(unit, condition);

		var success = unit.GetNextLabel();

		var instructions = BuildCondition(unit, condition, success, failure);
		instructions.Add(new LabelInstruction(unit, success));

		// Remove all occurrences of the following pattern from the instructions:
		// jmp [Label]
		// [Label]:
		for (var i = instructions.Count - 2; i >= 0; i--)
		{
			if (instructions[i].Is(InstructionType.JUMP) && instructions[i + 1].Is(InstructionType.LABEL))
			{
				var jump = instructions[i].To<JumpInstruction>();
				var label = instructions[i + 1].To<LabelInstruction>();

				if (!jump.IsConditional && Equals(jump.Label, label.Label))
				{
					instructions.RemoveAt(i);
				}
			}
		}

		// Replace all occurrences of the following pattern in the instructions:
		// [Conditional jump] [Label 1]
		// jmp [Label 2]
		// [Label 1]:
		// =====================================
		// [Inverted conditional jump] [Label 2]
		// [Label 1]:
		for (var i = instructions.Count - 3; i >= 0; i--)
		{
			if (instructions[i].Is(InstructionType.JUMP) &&
			   instructions[i + 1].Is(InstructionType.JUMP) &&
			   instructions[i + 2].Is(InstructionType.LABEL))
			{
				var conditional_jump = instructions[i].To<JumpInstruction>();
				var jump = instructions[i + 1].To<JumpInstruction>();
				var label = instructions[i + 2].To<LabelInstruction>();

				if (conditional_jump.IsConditional && !jump.IsConditional && Equals(conditional_jump.Label, label.Label) && !Equals(jump.Label, label.Label))
				{
					conditional_jump.Invert();
					conditional_jump.Label = jump.Label;

					instructions.RemoveAt(i + 1);
				}
			}
		}

		// Remove unused labels
		var labels = instructions.Where(i => i.Is(InstructionType.LABEL)).Select(i => i.To<LabelInstruction>()).ToList();
		var jumps = instructions.Where(i => i.Is(InstructionType.JUMP)).Select(j => j.To<JumpInstruction>());

		foreach (var label in labels)
		{
			// Check if any jump instruction uses the current label
			if (!jumps.Any(i => i.Label == label.Label))
			{
				// Since the label is not used, it can be removed
				instructions.Remove(label);
			}
		}

		// Append all the instructions to the unit
		foreach (var instruction in instructions)
		{
			if (instruction.Is(InstructionType.TEMPORARY_COMPARE))
			{
				instruction.To<TemporaryCompareInstruction>().Append(active_variables);
			}
			else
			{
				unit.Append(instruction);
			}
		}
	}

	private class TemporaryCompareInstruction : TemporaryInstruction
	{
		private Node Comparison { get; }
		private Node Left => Comparison.First!;
		private Node Right => Comparison.Last!;

		public TemporaryCompareInstruction(Unit unit, Node comparison) : base(unit, InstructionType.TEMPORARY_COMPARE)
		{
			Comparison = comparison;
		}

		public void Append(List<Variable> active_variables)
		{
			var state = Unit.GetState(Unit.Position);

			// Since this is a body of some statement is also has a scope
			using (var scope = new Scope(Unit, Comparison, active_variables))
			{
				// Merges all changes that happen in the scope with the outer scope
				var merge = new MergeScopeInstruction(Unit, scope);

				// Build the body
				var left = References.Get(Unit, Left);
				var right = References.Get(Unit, Right);

				// Compare the two operands
				Unit.Append(new CompareInstruction(Unit, left, right));

				// Restore the state after the body
				Unit.Append(merge);
			}

			Unit.Set(state);
		}
	}

	private static List<Instruction> BuildCondition(Unit unit, Node condition, Label success, Label failure)
	{
		if (condition.Is(NodeType.OPERATOR))
		{
			var operation = condition.To<OperatorNode>();
			var type = operation.Operator.Type;

			if (type == OperatorType.LOGIC)
			{
				return BuildLogicalCondition(unit, operation, success, failure);
			}
			else if (type == OperatorType.COMPARISON)
			{
				return BuildComparison(unit, operation, success, failure);
			}
		}

		if (condition.Is(NodeType.CONTENT))
		{
			return BuildCondition(unit, condition.First ?? throw new ApplicationException("Encountered an empty parenthesis while building a condition"), success, failure);
		}

		var replacement = new OperatorNode(Operators.NOT_EQUALS, condition.Position);
		condition.Replace(replacement);

		replacement.SetOperands(condition, new NumberNode(Assembler.Format, 0L, replacement.Position));

		return BuildCondition(unit, replacement, success, failure);
	}

	private static List<Instruction> BuildComparison(Unit unit, OperatorNode condition, Label success, Label failure)
	{
		var x = condition.Left.GetType();
		var y = condition.Right.GetType();
		var unsigned = (x.Format.IsDecimal() || y.Format.IsDecimal()) || (x.Format.IsUnsigned() && y.Format.IsUnsigned());

		return new List<Instruction>
		{
			new TemporaryCompareInstruction(unit, condition),
			new JumpInstruction(unit, (ComparisonOperator)condition.Operator, false, !unsigned, success),
			new JumpInstruction(unit, failure)
		};
	}

	private static List<Instruction> BuildLogicalCondition(Unit unit, OperatorNode condition, Label success, Label failure)
	{
		var instructions = new List<Instruction>();
		var interphase = unit.GetNextLabel();

		if (Equals(condition.Operator, Operators.AND))
		{
			instructions.AddRange(BuildCondition(unit, condition.Left, interphase, failure));
			instructions.Add(new LabelInstruction(unit, interphase));
			instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
		}
		else if (Equals(condition.Operator, Operators.OR))
		{
			instructions.AddRange(BuildCondition(unit, condition.Left, success, interphase));
			instructions.Add(new LabelInstruction(unit, interphase));
			instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
		}
		else
		{
			throw new ApplicationException("Unsupported logical operator encountered while building a conditional statement");
		}

		return instructions;
	}

	/// <summary>
	/// Determines whether the specified node tree can be built without branching
	/// </summary>
	private static bool IsBranchlessExecutionPossible(Node root)
	{
		// If the specified node contains a function call, branchless execution is not possible
		// Memory accesses can not be allowed, because they can cause side effects if they are invalid
		return root.Find(NodeType.FUNCTION, NodeType.CALL, NodeType.IF, NodeType.RETURN, NodeType.LOOP_CONTROL, NodeType.LINK, NodeType.OFFSET, NodeType.OBJECT_LINK, NodeType.OBJECT_UNLINK) == null;
	}

	/// <summary>
	/// Updates the specified edit so that it is executed conditionally
	/// </summary>
	private static void SetConditional(Node edit, Condition condition)
	{
		if (edit.Is(Operators.ASSIGN))
		{
			edit.To<OperatorNode>().Condition = condition;
			return;
		}

		throw new ArgumentException("Invalid edit node passed as a parameter");
	}

	/// <summary>
	/// Returns whether the edit can be conditional
	/// </summary>
	private static bool IsValidBranchlessExecutionEdit(Node edit)
	{
		/// TODO: Allow other action operators as well
		if (!edit.Is(Operators.ASSIGN))
		{
			return false;
		}

		// Ensure the edited node is a variable node
		var edited = Analyzer.GetEdited(edit);

		if (!edited.Is(NodeType.VARIABLE))
		{
			return false;
		}

		var variable = edited.To<VariableNode>().Variable;

		// Ensure the variable is predictable
		return variable.IsPredictable;
	}

	private static bool TryBuildBranchlessExecution(Unit unit, IfNode statement)
	{
		// Require the condition to be single comparison for now
		var comparison = Analyzer.GetSource(statement.Condition);
		
		if (!comparison.Is(OperatorType.COMPARISON))
		{
			if (!comparison.Is(NodeType.CALL, NodeType.FUNCTION)) return false;
			
			comparison = new OperatorNode(Operators.NOT_EQUALS).SetOperands(
				comparison.Clone(),
				new NumberNode(Parser.Format, 0L)
			);
		}

		if ((statement.Successor != null && statement.Successor.Is(NodeType.ELSE_IF)) || !IsBranchlessExecutionPossible(statement.Body))
		{
			return false;
		}

		// Do not allow jump nodes
		if (statement.Find(NodeType.JUMP) != null || (statement.Successor != null && statement.Successor.Find(NodeType.JUMP) != null))
		{
			return false;
		}

		var operation = (ComparisonOperator)comparison.To<OperatorNode>().Operator;

		// NOTE: There can not be increments and decrements operations since they can not be processed in the back end

		// Since there will not be function calls, the only meaningful nodes currently are edits
		var a = statement.Body.FindAll(i => i.Is(OperatorType.ACTION));

		// If the specified node contains memory edits, branchless execution should not be built
		if (a.Any(i => !IsValidBranchlessExecutionEdit(i)))
		{
			return false;
		}

		// Take the edits whose destination is an external variable looking from the perspective of the statement
		a = a.Where(i => !Analyzer.GetEdited(i).To<VariableNode>().Variable.Context.IsInside(statement.Body.Context)).ToList();

		// If any of the edits require conditional decimal moves, that will not be happening (at least on x64)
		if (a.Any(i => Analyzer.GetEdited(i).To<VariableNode>().Variable.GetRegisterFormat() == Format.DECIMAL))
		{
			return false;
		}

		/// NOTE: There can not be nested scopes inside the branches since they are not allowed while building a branchless version of conditional statement

		if (statement.Successor != null)
		{
			var b = statement.Successor.To<ElseNode>().Body.FindAll(i => i.Is(OperatorType.ACTION));

			// If the specified node contains memory edits, branchless execution should not be built
			if (b.Any(i => !IsValidBranchlessExecutionEdit(i)))
			{
				return false;
			}

			// Take the edits whose destination is an external variable looking from the perspective of the successor
			b = b.Where(i => !Analyzer.GetEdited(i).To<VariableNode>().Variable.Context.IsInside(statement.Successor.To<ElseNode>().Body.Context)).ToList();

			// If any of the edits require conditional decimal moves, that will not be happening (at least on x64)
			if (b.Any(i => Analyzer.GetEdited(i).To<VariableNode>().Variable.GetRegisterFormat() == Format.DECIMAL))
			{
				return false;
			}

			// Situation 1:
			// if ... { a = 1 } else { a = -1 }
			// =>
			// a = -1
			// a ?= -1

			// Situation 2:
			// if ... { a = 1 } else { b = 1 }
			// =>
			// a ?= 1
			// b ?= 1

			// Situation 3:
			// b = 0, if ... { a = 1, b = 1 } else { a = b + 1 }
			// =>
			// a = 1
			// b ?= 1
			// a ?= b + 1

			// Situation 4:
			// b = 0, if ... { a = 1, b = 1 } else { b = b + 1 }
			// =>
			// a ?= 1
			// b ?= 1
			// b ?= b + 1

			// Build the nodes around the actual condition by disabling the condition temporarily
			var instance = statement.Condition.Instance;
			statement.Condition.Instance = NodeType.DISABLED;

			Builders.Build(unit, statement.Left);

			statement.Condition.Instance = instance;

			var left = References.Get(unit, comparison.Left);
			var right = References.Get(unit, comparison.Right);

			var condition = new Condition(left, right, operation);
			var inverse_condition = new Condition(left, right, operation.Counterpart!);

			var x = a.GroupBy(i => Analyzer.GetEdited(i).To<VariableNode>().Variable);
			var y = b.GroupBy(i => Analyzer.GetEdited(i).To<VariableNode>().Variable).ToDictionary(i => i.Key, i => i.ToList());

			foreach (var iterator in x)
			{
				if (!y.ContainsKey(iterator.Key))
				{
					iterator.ForEach(i => SetConditional(i, condition));
					continue;
				}

				var assignments = y[iterator.Key];

				// If there is an usage which is not an edit before any of the edits in the successor, the edits in the if-statement must be conditional
				if (statement.Successor.Find(i => i.Is(iterator.Key) && !Analyzer.IsEdited(i) && assignments.All(j => j.IsAfter(i) || i.IsUnder(j))) != null)
				{
					iterator.ForEach(i => SetConditional(i, condition));
				}

				assignments.ForEach(i => SetConditional(i, inverse_condition));

				y.Remove(iterator.Key);
			}

			y.Values.SelectMany(i => i).ForEach(i => SetConditional(i, inverse_condition));

			statement.Body.FindAll(NodeType.LOOP_CONTROL).Cast<LoopControlNode>().ForEach(i => i.Condition = condition);
			statement.Body.FindAll(NodeType.JUMP).Cast<JumpNode>().ForEach(i => i.Condition = condition);
			statement.Successor.FindAll(NodeType.LOOP_CONTROL).Cast<LoopControlNode>().ForEach(i => i.Condition = inverse_condition);
			statement.Successor.FindAll(NodeType.LOOP_CONTROL).Cast<JumpNode>().ForEach(i => i.Condition = condition);
		}
		else
		{
			// Build the nodes around the actual condition by disabling the condition temporarily
			var instance = statement.Condition.Instance;
			statement.Condition.Instance = NodeType.DISABLED;

			Builders.Build(unit, statement.Left);

			statement.Condition.Instance = instance;

			var left = References.Get(unit, comparison.Left);
			var right = References.Get(unit, comparison.Right);

			var condition = new Condition(left, right, operation);

			// Set every assignment to be conditional
			a.ForEach(i => SetConditional(i, condition));

			statement.Body.FindAll(NodeType.LOOP_CONTROL).Cast<LoopControlNode>().ForEach(i => i.Condition = condition);
			statement.Body.FindAll(NodeType.JUMP).Cast<JumpNode>().ForEach(i => i.Condition = condition);
		}

		Builders.Build(unit, statement.Body);

		if (statement.Successor != null)
		{
			Builders.Build(unit, statement.Successor.To<ElseNode>().Body);
		}

		return true;
	}
}