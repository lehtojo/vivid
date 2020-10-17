using System.Collections.Generic;
using System.Linq;
using System;

public class VariableUsageInfo
{
	public Variable Variable { get; }
	public Result? Reference { get; set; }
	public int Usages { get; }

	public VariableUsageInfo(Variable variable, int usages)
	{
		Variable = variable;
		Usages = usages;
	}
}

public static class Loops
{
	/// <summary>
	/// Returns whether the given variable is a local variable
	/// </summary>
	private static bool IsNonLocalVariable(Variable variable, params Context[] local_contexts)
	{
		return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
	}

	/// <summary>
	/// Analyzes how many times each variable in the given node tree is used and sorts the result as well
	/// </summary>
	private static Dictionary<Variable, int> GetNonLocalVariableUsageCount(Unit unit, Node root, params Context[] local_contexts)
	{
		var variables = new Dictionary<Variable, int>();

		foreach (var iterator in root)
		{
			if (iterator is VariableNode node && IsNonLocalVariable(node.Variable, local_contexts))
			{
				if (node.Variable.IsPredictable)
				{
					variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
				}
				else if (!node.Parent?.Is(NodeType.LINK) ?? false)
				{
					if (unit.Self == null)
					{
						throw new ApplicationException("Detected an use of the this pointer but it was missing");
					}

					variables[unit.Self] = variables.GetValueOrDefault(unit.Self, 0) + 1;
				}
			}
			else
			{
				foreach (var usage in GetNonLocalVariableUsageCount(unit, iterator, local_contexts))
				{
					variables[usage.Key] = variables.GetValueOrDefault(usage.Key, 0) + usage.Value;
				}
			}
		}

		return variables;
	}

	public static Result BuildControlInstruction(Unit unit, LoopControlNode node)
	{
		if (node.Loop == null)
		{
			throw new ApplicationException("Loop control instruction was not inside a loop");
		}

		if (node.Instruction == Keywords.STOP)
		{
			var exit = node.Loop.Exit ?? throw new ApplicationException("Missing loop exit label");

			var symmetry_start = unit.Loops[node.Loop.Identifier!.Value] ?? throw new ApplicationException("Loop was not registered to unit");
			var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

			// Restore the state after the body
			symmetry_end.Append();

			// Restore the state after the body
			unit.Append(symmetry_end);

			return new JumpInstruction(unit, exit).Execute();
		}
		else if (node.Instruction == Keywords.CONTINUE)
		{
			var start = node.Loop.Start ?? throw new ApplicationException("Missing loop exit label");

			var symmetry_start = unit.Loops[node.Loop.Identifier!.Value] ?? throw new ApplicationException("Loop was not registered to unit");
			var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

			// Restore the state after the body
			symmetry_end.Append();

			// Restore the state after the body
			unit.Append(symmetry_end);

			return new JumpInstruction(unit, start).Execute();
		}
		else
		{
			throw new NotImplementedException("Loop control instruction not implemented yet");
		}
	}

	/// <summary>
	/// Returns info about variable usage in the given loop
	/// </summary>
	private static List<VariableUsageInfo> GetAllVariableUsages(Unit unit, LoopNode node)
	{
		// Get all non-local variables in the loop and their number of usages
		var variables = GetNonLocalVariableUsageCount(unit, node, node.Body.Context)
					   .Select(i => new VariableUsageInfo(i.Key, i.Value)).ToList();

		// Sort the variables based on their number of usages (most used variable first)
		variables.Sort((a, b) => -a.Usages.CompareTo(b.Usages));

		return variables;
	}

	/// <summary>
	/// Returns whether the given loop contains functions
	/// </summary>
	private static bool ContainsFunction(LoopNode node)
	{
		return node.Find(n => n.Is(NodeType.FUNCTION)) != null;
	}

	/// <summary>
	/// Tries to move most used loop variables into registers
	/// </summary>
	private static void CacheLoopVariables(Unit unit, LoopNode node)
	{
		var variables = GetAllVariableUsages(unit, node);

		// If the loop contains at least one function, the variables should be cached into non-volatile registers
		// (Otherwise there would be a lot of register moves trying to save the cached variables)
		var non_volatile_mode = ContainsFunction(node);

		unit.Append(new CacheVariablesInstruction(unit, node, variables, non_volatile_mode));
	}

	private static Result BuildForeverLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
	{
		var active_variables = Scope.GetAllActiveVariablesForScope(unit, new Node[] { loop }, loop.Body.Context.Parent!, loop.Body.Context);

		var state = unit.GetState(unit.Position);
		var result = (Result?)null;

		using (var scope = new Scope(unit, active_variables))
		{
			// Append the label where the loop will start
			unit.Append(start);

			var symmetry_start = new SymmetryStartInstruction(unit, active_variables);
			unit.Append(symmetry_start);

			// Register loop to the unit
			loop.Identifier = Guid.NewGuid();
			unit.Loops.Add(loop.Identifier!.Value, symmetry_start);

			// Build the loop body
			result = Builders.Build(unit, loop.Body);

			var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

			// Restore the state after the body
			symmetry_end.Append();

			// Restore the state after the body
			unit.Append(symmetry_end);
		}

		unit.Set(state);

		return result;
	}

	private static Result BuildLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
	{
		var active_variables = Scope.GetAllActiveVariablesForScope(unit, new Node[] { loop }, loop.Body.Context.Parent!, loop.Body.Context);

		var state = unit.GetState(unit.Position);
		var result = (Result?)null;

		using (var scope = new Scope(unit, active_variables))
		{
			// Append the label where the loop will start
			unit.Append(start);

			var symmetry_start = new SymmetryStartInstruction(unit, active_variables);
			unit.Append(symmetry_start);

			// Register loop to the unit
			loop.Identifier = Guid.NewGuid();
			unit.Loops.Add(loop.Identifier!.Value, symmetry_start);

			// Build the loop body
			result = Builders.Build(unit, loop.Body);

			if (!loop.IsForeverLoop)
			{
				// Build the loop action
				Builders.Build(unit, loop.Action);
			}

			scope.AppendFinalizers = false;

			var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

			// Restore the state after the body
			symmetry_end.Append();

			// Restore the state after the body
			unit.Append(symmetry_end);

			// Initialize the condition
			loop.GetConditionInitialization().ForEach(i => Builders.Build(unit, i));

			BuildEndCondition(unit, loop.Condition, start.Label);

			// Keep all scope variables which are needed later active
			active_variables.ForEach(i => unit.Append(new GetVariableInstruction(unit, i)));
		}

		unit.Set(state);

		return result;
	}

	private static Result BuildForeverLoop(Unit unit, LoopNode node)
	{
		var start = unit.GetNextLabel();

		// Get the current state of the unit for later recovery
		var recovery = new SaveStateInstruction(unit);
		unit.Append(recovery);

		// Initialize the loop
		CacheLoopVariables(unit, node);

		Scope.PrepareConditionallyChangingConstants(unit, node, node.Context, node.Body.Context);
		unit.Append(new BranchInstruction(unit, new Node[] { node.Body }));

		// Register the start and exit label to the loop for control keywords
		node.Start = unit.GetNextLabel();
		node.Exit = unit.GetNextLabel();

		// Append the start label
		unit.Append(new LabelInstruction(unit, node.Start));

		// Build the loop body
		var result = BuildForeverLoopBody(unit, node, new LabelInstruction(unit, start));

		// Jump to the start of the loop
		unit.Append(new JumpInstruction(unit, start));

		// Recover the previous state
		unit.Append(new RestoreStateInstruction(unit, recovery));

		// Append the exit label
		unit.Append(new LabelInstruction(unit, node.Exit));

		return result;
	}

	public static Result Build(Unit unit, LoopNode node)
	{
		if (node.IsForeverLoop)
		{
			return BuildForeverLoop(unit, node);
		}

		// Create the start and end label of the loop
		var start = unit.GetNextLabel();
		var end = unit.GetNextLabel();

		// Register the start and exit label to the loop for control keywords
		node.Start = start;
		node.Exit = end;

		// Initialize the loop
		Builders.Build(unit, node.Initialization);

		// Try to cache loop variables
		CacheLoopVariables(unit, node);

		Scope.PrepareConditionallyChangingConstants(unit, node, node.Body.Context);
		unit.Append(new BranchInstruction(unit, new Node[] { node.Initialization, node.Condition, node.Action, node.Body }));

		// Initialize the condition
		node.GetConditionInitialization().ForEach(i => Builders.Build(unit, i));

		// Jump to the end based on the comparison
		Conditionals.BuildCondition(unit, node.Context.Parent!, node.Condition, end);

		// Get the current state of the unit for later recovery
		var recovery = new SaveStateInstruction(unit);
		unit.Append(recovery);

		// Build the loop body
		var result = BuildLoopBody(unit, node, new LabelInstruction(unit, start));

		// Append the label where the loop ends
		unit.Append(new LabelInstruction(unit, end));

		// Recover the previous state
		unit.Append(new RestoreStateInstruction(unit, recovery));

		return result;
	}

	private static void BuildEndCondition(Unit unit, Node condition, Label success)
	{
		var failure = unit.GetNextLabel();

		var instructions = BuildCondition(unit, condition, success, failure);
		instructions.Add(new LabelInstruction(unit, failure));

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

		// Replace all occurances of the following pattern in the instructions:
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
			if (!jumps.Any(j => j.Label == label.Label))
			{
				// Since the label isn't used, it can be removed
				instructions.Remove(label);
			}
		}

		// Append all the instructions to the unit
		foreach (var instruction in instructions)
		{
			if (instruction.Is(InstructionType.PSEUDO_COMPARE))
			{
				instruction.To<PseudoCompareInstruction>().Append();
			}
			else
			{
				unit.Append(instruction);
			}
		}
	}

	private class PseudoCompareInstruction : TemporaryInstruction
	{
		private Node Comparison { get; }
		private Node Left => Comparison.First!;
		private Node Right => Comparison.Last!;

		public PseudoCompareInstruction(Unit unit, Node comparison) : base(unit)
		{
			Comparison = comparison;
		}

		public void Append()
		{
			// Get the current state of the unit for later recovery
			var recovery = new SaveStateInstruction(Unit);
			Unit.Append(recovery);

			var state = Unit.GetState(Unit.Position);

			// Since this is a body of some statement is also has a scope
			using (new Scope(Unit, Unit.Scope!.ActiveVariables))
			{
				// Merges all changes that happen in the scope with the outer scope
				var merge = new MergeScopeInstruction(Unit);

				// Build the body
				var left = References.Get(Unit, Left);
				var right = References.Get(Unit, Right);

				// Compare the two operands
				Unit.Append(new CompareInstruction(Unit, left, right));

				// Restore the state after the body
				Unit.Append(merge);
			}

			Unit.Set(state);

			// Recover the previous state
			Unit.Append(new RestoreStateInstruction(Unit, recovery));
		}

		public override InstructionType GetInstructionType()
		{
			return InstructionType.PSEUDO_COMPARE;
		}
	}

	private static List<Instruction> BuildCondition(Unit unit, Node condition, Label success, Label failure)
	{
		if (condition.Is(NodeType.OPERATOR))
		{
			var operation = condition.To<OperatorNode>();

			return operation.Operator.Type switch
			{
				OperatorType.LOGIC => BuildLogicalCondition(unit, operation, success, failure),
				OperatorType.COMPARISON => BuildComparison(unit, operation, success, failure),
				_ => throw new ApplicationException(
				   "Unsupported operator encountered while building a conditional statement")
			};
		}

		if (condition.Is(NodeType.CONTENT))
		{
			return BuildCondition(unit, condition.First ?? throw new ApplicationException("Encountered an empty parenthesis while building a condition"), success, failure);
		}

		var replacement = new OperatorNode(Operators.NOT_EQUALS);
		condition.Replace(replacement);

		replacement.SetOperands(condition, new NumberNode(Assembler.Format, 0L));

		return BuildCondition(unit, replacement, success, failure);
	}

	private static List<Instruction> BuildComparison(Unit unit, OperatorNode condition, Label success, Label failure)
	{
		var x = condition.Left.GetType() ?? throw new ApplicationException("Could not get the type of left operand");
		var y = condition.Right.GetType() ?? throw new ApplicationException("Could not get the type of right operand");
		var unsigned = (x.Format.IsDecimal() || y.Format.IsDecimal()) || (x.Format.IsUnsigned() && y.Format.IsUnsigned());

		return new List<Instruction>
		{
			new PseudoCompareInstruction(unit, condition),
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