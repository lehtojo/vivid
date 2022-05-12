using System;
using System.Collections.Generic;
using System.Linq;

public static class Loops
{
	/// <summary>
	/// Builds a loop control instruction such as continue and stop
	/// </summary>
	public static Result BuildControlInstruction(Unit unit, LoopControlNode node)
	{
		// Add position of the command node as debug information
		unit.TryAppendPosition(node.Position);

		if (node.Loop == null) throw new ApplicationException("Loop control instruction was not inside a loop");

		var scope = node.Loop.Scope ?? throw new ApplicationException("Missing loop scope");

		if (node.Instruction == Keywords.STOP)
		{
			if (node.Condition != null) Arithmetic.BuildCondition(unit, node.Condition);

			unit.Append(new MergeScopeInstruction(unit, scope));
			var label = node.Loop.Exit ?? throw new ApplicationException("Missing loop exit label");

			if (node.Condition != null) return new JumpInstruction(unit, node.Condition.Operator, false, !node.Condition.IsDecimal, label).Execute();

			return new JumpInstruction(unit, label).Execute();
		}
		else if (node.Instruction == Keywords.CONTINUE)
		{
			var statement = node.Loop;
			var start = statement.Start ?? throw new ApplicationException("Missing loop start label");

			if (statement.IsForeverLoop)
			{
				unit.Append(new MergeScopeInstruction(unit, scope));
				return new JumpInstruction(unit, start).Execute();
			}

			// Build the nodes around the actual condition by disabling the condition temporarily
			var instance = statement.Condition.Instance;
			statement.Condition.Instance = NodeType.DISABLED;

			// Initialization of the condition might happen multiple times, therefore inner labels can duplicate
			Inlines.LocalizeLabels(unit.Function, statement.Initialization.Next!);

			Builders.Build(unit, statement.Initialization.Next!);

			statement.Condition.Instance = instance;

			// Prepare for starting the loop again potentially
			unit.Append(new MergeScopeInstruction(unit, scope));

			// Build the actual condition
			var exit = statement.Exit ?? throw new ApplicationException("Missing loop exit label");
			BuildEndCondition(unit, statement.Condition, start, exit);
			
			return new Result();
		}
		else
		{
			throw new NotImplementedException("Unknown loop control instruction");
		}
	}

	/// <summary>
	/// Builds the body of the specified loop without any of the steps
	/// </summary>
	private static Result BuildForeverLoopBody(Unit unit, LoopNode statement, LabelInstruction start)
	{
		var active_variables = Scope.GetAllActiveVariables(unit, statement);

		var state = unit.GetState(unit.Position);
		var result = (Result?)null;

		using (var scope = new Scope(unit, statement.Body, active_variables))
		{
			statement.Scope = scope;

			// Append the label where the loop will start
			unit.Append(start);

			// Build the loop body
			result = Builders.Build(unit, statement.Body);

			unit.TryAppendPosition(statement.Body.End);
			unit.Append(new MergeScopeInstruction(unit, scope));
		}

		unit.Set(state);

		return result;
	}

	/// <summary>
	/// Builds the body of the specified loop with its steps
	/// </summary>
	private static Result BuildLoopBody(Unit unit, LoopNode statement, LabelInstruction start, List<Variable> active_variables)
	{
		var state = unit.GetState(unit.Position);
		var result = (Result?)null;

		using (var scope = new Scope(unit, statement.Body, active_variables))
		{
			statement.Scope = scope;

			// Append the label where the loop will start
			unit.Append(start);

			// Build the loop body
			result = Builders.Build(unit, statement.Body);
			
			unit.TryAppendPosition(statement.Body.End);

			if (!statement.IsForeverLoop)
			{
				// Build the loop action
				Builders.Build(unit, statement.Action);
			}

			// Build the nodes around the actual condition by disabling the condition temporarily
			var instance = statement.Condition.Instance;
			statement.Condition.Instance = NodeType.DISABLED;

			// Initialization of the condition might happen multiple times, therefore inner labels can duplicate
			Inlines.LocalizeLabels(unit.Function, statement.Initialization.Next!);

			Builders.Build(unit, statement.Initialization.Next!);

			statement.Condition.Instance = instance;

			unit.Append(new MergeScopeInstruction(unit, scope));
			BuildEndCondition(unit, statement.Condition, start.Label);
		}

		unit.Set(state);

		return result;
	}

	/// <summary>
	/// Builds the specified forever-loop
	/// </summary>
	private static Result BuildForeverLoop(Unit unit, LoopNode statement)
	{
		var start = unit.GetNextLabel();

		if (!Assembler.IsDebuggingEnabled)
		{
			// Try to cache loop variables
			Scope.Cache(unit, statement);
		}

		// Load constants which might be edited inside the loop
		Scope.LoadConstants(unit, statement, statement.Context, statement.Body.Context);

		// Register the start and exit label to the loop for control keywords
		statement.Start = unit.GetNextLabel();
		statement.Exit = unit.GetNextLabel();

		// Append the start label
		unit.Append(new LabelInstruction(unit, statement.Start));

		// Build the loop body
		var result = BuildForeverLoopBody(unit, statement, new LabelInstruction(unit, start));

		// Jump to the start of the loop
		unit.Append(new JumpInstruction(unit, start));

		// Append the exit label
		unit.Append(new LabelInstruction(unit, statement.Exit));

		return result;
	}

	/// <summary>
	/// Builds the specified loop
	/// </summary>
	public static Result Build(Unit unit, LoopNode statement)
	{
		unit.TryAppendPosition(statement);

		if (statement.IsForeverLoop)
		{
			return BuildForeverLoop(unit, statement);
		}

		// Create the start and end label of the loop
		var start = unit.GetNextLabel();
		var end = unit.GetNextLabel();

		// Register the start and exit label to the loop for control keywords
		statement.Start = start;
		statement.Exit = end;

		// Initialize the loop
		Builders.Build(unit, statement.Initialization);

		if (!Assembler.IsDebuggingEnabled)
		{
			// Try to cache loop variables
			Scope.Cache(unit, statement);
		}

		// Load constants which might be edited inside the loop
		Scope.LoadConstants(unit, statement, statement.Body.Context);

		// Build the nodes around the actual condition by disabling the condition temporarily
		var instance = statement.Condition.Instance;
		statement.Condition.Instance = NodeType.DISABLED;

		Builders.Build(unit, statement.Initialization.Next!);

		statement.Condition.Instance = instance;

		var active_variables = Scope.GetAllActiveVariables(unit, statement);

		// Jump to the end based on the comparison
		Conditionals.BuildCondition(unit, statement.Condition, end, active_variables);

		// Build the loop body
		var result = BuildLoopBody(unit, statement, new LabelInstruction(unit, start), active_variables);

		// Append the label where the loop ends
		unit.Append(new LabelInstruction(unit, end));

		return result;
	}

	/// <summary>
	/// Builds the the specified condition which should be placed at the end of a loop
	/// </summary>
	private static void BuildEndCondition(Unit unit, Node condition, Label success, Label? failure = null)
	{
		var exit = unit.GetNextLabel();

		var instructions = BuildCondition(unit, condition, success, exit);
		instructions.Add(new LabelInstruction(unit, exit));

		if (failure != null) instructions.Add(new JumpInstruction(unit, failure));

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
				instruction.To<TemporaryCompareInstruction>().Append();
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

		public void Append()
		{
			var state = Unit.GetState(Unit.Position);

			// Since this is a body of some statement is also has a scope
			using (var scope = new Scope(Unit, Comparison, Unit.Scope!.Actives))
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
}