using System;
using System.Collections.Generic;
using System.Linq;

public static class Loops
{
	/// <summary>
	/// Builds a loop command such as continue and stop
	/// </summary>
	public static Result BuildCommand(Unit unit, CommandNode node)
	{
		// Add position of the command node as debug information
		unit.AddDebugPosition(node.Position);

		if (node.Container == null) throw new ApplicationException("Loop control instruction was not inside a loop");

		if (node.Instruction == Keywords.STOP)
		{
			var label = node.Container.Exit ?? throw new ApplicationException("Missing loop exit label");
			return new JumpInstruction(unit, label).Add();
		}
		else if (node.Instruction == Keywords.CONTINUE)
		{
			var statement = node.Container;
			var start = statement.Start ?? throw new ApplicationException("Missing loop start label");

			if (statement.IsForeverLoop)
			{
				return new JumpInstruction(unit, start).Add();
			}

			// Build the nodes around the actual condition by disabling the condition temporarily
			var instance = statement.Condition.Instance;
			statement.Condition.Instance = NodeType.DISABLED;

			// Initialization of the condition might happen multiple times, therefore inner labels can duplicate
			Inlines.LocalizeLabels(unit.Function, statement.Initialization.Next!);

			Builders.Build(unit, statement.Initialization.Next!);

			statement.Condition.Instance = instance;

			// Build the actual condition
			var exit = statement.Exit ?? throw new ApplicationException("Missing loop exit label");
			BuildEndCondition(unit, statement.Condition, start, exit);
			
			return new Result();
		}

		throw new NotImplementedException("Unknown loop command");
	}

	/// <summary>
	/// Builds the body of the specified loop without any of the steps
	/// </summary>
	private static Result BuildForeverLoopBody(Unit unit, LoopNode statement, LabelInstruction start)
	{
		// Add the label where the loop will start
		unit.Add(start);

		// Build the loop body
		var result = Builders.Build(unit, statement.Body);

		unit.AddDebugPosition(statement.Body.End);
		return result;
	}

	/// <summary>
	/// Builds the body of the specified loop with its steps
	/// </summary>
	private static Result BuildLoopBody(Unit unit, LoopNode statement, LabelInstruction start)
	{
		// Add the label where the loop will start
		unit.Add(start);

		// Build the loop body
		var result = Builders.Build(unit, statement.Body);
		
		unit.AddDebugPosition(statement.Body.End);

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

		BuildEndCondition(unit, statement.Condition, start.Label);

		return result;
	}

	/// <summary>
	/// Builds the specified forever-loop
	/// </summary>
	private static Result BuildForeverLoop(Unit unit, LoopNode statement)
	{
		var start = unit.GetNextLabel();

		// Register the start and exit label to the loop for control keywords
		statement.Start = unit.GetNextLabel();
		statement.Exit = unit.GetNextLabel();

		// Add the start label
		unit.Add(new LabelInstruction(unit, statement.Start));

		// Build the loop body
		var result = BuildForeverLoopBody(unit, statement, new LabelInstruction(unit, start));

		// Jump to the start of the loop
		unit.Add(new JumpInstruction(unit, start));

		// Add the exit label
		unit.Add(new LabelInstruction(unit, statement.Exit));

		return result;
	}

	/// <summary>
	/// Builds the specified loop
	/// </summary>
	public static Result Build(Unit unit, LoopNode statement)
	{
		unit.AddDebugPosition(statement);

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

		// Build the nodes around the actual condition by disabling the condition temporarily
		var instance = statement.Condition.Instance;
		statement.Condition.Instance = NodeType.DISABLED;

		Builders.Build(unit, statement.Initialization.Next!);

		statement.Condition.Instance = instance;

		// Jump to the end based on the comparison
		Conditionals.BuildCondition(unit, statement.Condition, end);

		// Build the loop body
		var result = BuildLoopBody(unit, statement, new LabelInstruction(unit, start));

		// Add the label where the loop ends
		unit.Add(new LabelInstruction(unit, end));

		return result;
	}

	/// <summary>
	/// Builds the the specified condition which should be placed at the end of a loop
	/// </summary>
	private static void BuildEndCondition(Unit unit, Node condition, Label success, Label? failure = null)
	{
		var exit = unit.GetNextLabel();

		var instructions = Conditionals.BuildCondition(unit, condition, success, exit);
		instructions.Add(new LabelInstruction(unit, exit));

		if (failure != null) instructions.Add(new JumpInstruction(unit, failure));

		Conditionals.BuildConditionInstructions(unit, instructions);
	}
}