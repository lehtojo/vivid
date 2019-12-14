using System.Collections.Generic;
using System.Linq;
using System;

public class Loop
{
	private const int MAX_RESERVED_REGISTERS = 3;

	/// <summary>
	/// Returns list of all used variables in the node tree. 
	/// The list is sorted by variable usage
	/// </summary>
	/// <param name="node">Node tree to scan</param>
	/// <returns>List of all used variables sorted by usage amount</returns>
	private static List<Variable> GetRelevantVariables(LoopNode node)
	{
		// .Where(v => v.Context == node.Context.Parent)

		// Find all used variables (contains duplicates)
		var variables = node.FindAll(n => n.GetNodeType() == NodeType.VARIABLE_NODE)
								.Select(n => n as VariableNode)
									.Select(n => n.Variable);

		var occurrences = new Dictionary<Variable, int>();

		// Calculate how many times variables are used (removes duplicates)
		foreach (var variable in variables)
		{
			if (!occurrences.ContainsKey(variable))
			{
				var count = variables.Count(v => v == variable);
				occurrences.Add(variable, count);
			}
		}

		// Find the most used variables
		return occurrences.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
	}

	private static bool GetSymmetricalInitialization(Unit unit, Instructions instructions, LoopNode node, out List<Variable> variables, out List<Register> registers)
	{
		variables = new List<Variable>();
		registers = new List<Register>();

		if (node.Find(n => n.GetNodeType() == NodeType.FUNCTION_NODE) != null)
		{
			return false;
		}

		variables = GetRelevantVariables(node);
		
		var count = Math.Min(MAX_RESERVED_REGISTERS, variables.Count);

		for (var i = 0; i < count; i++)
		{
			var variable = variables[i];
			var register = unit.GetNextRegister();

			var source = References.GetVariableReference(unit, variable, ReferenceType.READ);
			instructions.Append(source);

			Memory.Move(unit, instructions, source.Reference, new RegisterReference(register));

			register.Attach(Value.GetVariable(new RegisterReference(register), variable));
			registers.Add(register);
		}

		return true;
	}

	private static void Connect(Unit unit, Instructions instructions, List<Variable> variables, List<Register> registers)
	{
		for (var i = 0; i < registers.Count; i++)
		{
			var variable = variables[i];
			var register = registers[i];

			var source = References.GetVariableReference(unit, variable, ReferenceType.READ);
			instructions.Append(source);

			Memory.Move(unit, instructions, source.Reference, new RegisterReference(register));

			register.Attach(Value.GetVariable(new RegisterReference(register), variable));
		}
	}

	private static Instructions GetForeverLoop(Unit unit, LoopNode node)
	{
		var instructions = new Instructions();

		// Try to create symmetrical initialization
		var symmetrical = GetSymmetricalInitialization(unit, instructions, node, out List<Variable> variables, out List<Register> registers);

		// Create loop start label
		var start = unit.NextLabel;
		instructions.Label(start);

		var body = unit.Assemble(node.Body);
		instructions.Append(body);

		if (symmetrical)
		{
			Connect(unit, instructions, variables, registers);
		}

		instructions.Append("jmp {0}", start);

		unit.Reset();

		return instructions;
	}

	public static Instructions Build(Unit unit, LoopNode node)
	{
		var instructions = new Instructions();

		// Execute initialization before the loops starts
		if (!node.IsForever && node.Initialization.GetNodeType() != NodeType.NORMAL)
		{
			instructions.Append(unit.Assemble(node.Initialization));
		}

		// Reset the unit since loop must be repeatable
		unit.Reset();

		// Forever loops need special handling for better performance
		if (node.IsForever)
		{
			return instructions.Append(GetForeverLoop(unit, node));
		}

		// Try to create symmetrical initialization
		var symmetrical = GetSymmetricalInitialization(unit, instructions, node, out List<Variable> variables, out List<Register> registers);

		// Create loop start label
		var start = unit.NextLabel;
		instructions.Label(start);

		// Create loops end label
		var end = unit.NextLabel;

		// Assemble the loop condidition
		var condition = Comparison.Jump(unit, (OperatorNode)node.Condition, true, end);
		instructions.Append(condition);

		unit.Step(instructions);

		// Create the loop body
		var body = unit.Assemble(node.Body);
		instructions.Append(body);

		// Restore stack original position
		unit.Stack.Restore(instructions);

		if (symmetrical)
		{
			Connect(unit, instructions, variables, registers);
		}
		
		// Create the action step of the loop
		var action = node.Action;

		if (action.GetNodeType() != NodeType.NORMAL)
		{
			instructions.Append(unit.Assemble(action));
		}

		// Add code for repeating the loop
		instructions.Append("jmp {0}", start);
		instructions.Label(end);

		// Loop state mustn't be visible to later code
		unit.Reset();

		return instructions;
	}
}