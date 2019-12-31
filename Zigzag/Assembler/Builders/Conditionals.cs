public class Conditionals
{
	/**
     * Builds an if statement node into instructions
     * @param unit Unit used to assemble
     * @param root If statements represented in node form
     * @param end Label that is used as an exit from the if statement's body
     * @return If statement built into instructions
     */
	private static Instructions Build(Unit unit, IfNode root, string end)
	{
		var instructions = new Instructions();
		var next = root.Successor != null ? unit.NextLabel : end;

		var condition = root.Condition;

		// Assemble the condition
		if (condition.GetNodeType() == NodeType.OPERATOR_NODE)
		{
			var operation = condition as OperatorNode;
			var success = new RequestableLabel(unit);

			Comparison.Jump(unit, instructions, operation, true, success, new Label(next));

			if (success.Used)
			{
				instructions.Label(success.GetName());
			}

			unit.Step(instructions);
		}

		Instructions? successor = null;
		var clone = unit.Clone();

		// Assemble potential successor
		if (root.Successor != null)
		{
			var node = root.Successor;

			if (node.GetNodeType() == NodeType.ELSE_IF_NODE)
			{
				successor = Conditionals.Build(clone, (IfNode)node, end);
			}
			else
			{
				successor = clone.Assemble(node);
			}
		}

		// Clone the unit since if statements may have multiple sections that don't affect each other
		clone = unit.Clone();

		var body = clone.Assemble(root.Body);
		instructions.Append(body);

		clone.Stack.Restore(instructions);

		// Merge all assembled sections together
		if (successor != null)
		{
			instructions.Append("jmp {0}", end);
			instructions.Label(next);
			instructions.Append(successor);
		}

		return instructions;
	}

	/**
     * Builds an if statement into instructions
     * @param unit Unit used to assemble
     * @param node If statement represented in node form
     * @return If statement built into instructions
     */
	public static Instructions Start(Unit unit, IfNode node)
	{
		var end = unit.NextLabel;

		var instructions = Conditionals.Build(unit, node, end);
		instructions.Append("{0}: ", end);

		unit.Reset();

		return instructions;
	}
}
/*
			 *	a < b | a == b
			 *  
			 *  cmp eax, ebx
			 *	jl success
			 *	cmp eax, ebx
			 *	jne failure
			 *	
			 *	a < b & a == b
			 *	cmp eax, ebx
			 *	jqe failure
			 *	cmp eax, ebx
			 *	jne failure
			 *	
			 *	
			 *	(a < b & a > c) | (a == b & a == c)
			 *	
			 *	cmp eax, ebx
			 *	jge failure_1
			 *	cmp eax, ecx
			 *	jg success
			 *	failure_1:
			 *	cmp eax, ebx
			 *	jne failure
			 *	cmp eax, ecx
			 *	jne failure
			 *	
			 *	(a < b & a > c) & (a == b & a == c)
			 *	
			 *	cmp eax, ebx
			 *	jge failure
			 *	cmp eax, ecx
			 *	jle failure
			 *	
			 *	cmp eax, ebx
			 *	jne failure
			 *	cmp eax, ecx
			 *	jne failure
			 *	
			 *	(a < b & a > c) & (a == b | a == c)
			 *	
			 *	cmp eax, ebx
			 *	jge failure
			 *	cmp eax, ecx
			 *	jle failure
			 *	
			 *	cmp eax, ebx
			 *	je success
			 *	cmp eax, ecx
			 *	jne failure
			 *	
			 *	(a < b | a > c) & (a == b & a == c)
			 *	cmp eax, ebx
			 *	jl success_1
			 *	cmp eax, ecx
			 *	jle failure
			 *	success_1:
			 *	cmp eax, ebx
			 *	jne failure
			 *	cmp eax, ecx
			 *	jne failure
			 */
/*
* Intermediate: Complex? & (Left is OR?)
* 
* OR:
* 1. Success 
* 2. Failure
* 
* AND:
* 1. Failure
* 2. Failure (Invert?)
* 
* 
* 
*/
