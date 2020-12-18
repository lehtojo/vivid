using System.Linq;
using System;

public static class Lambdas
{
	public static Result Build(Unit unit, LambdaNode node)
	{
		if (!node.Lambda.Implementations.Any())
		{
			throw new ApplicationException("Missing implementation for lambda");
		}
		
		unit.TryAppendPosition(node);

		var implementation = node.Lambda.Implementation;
		var root = implementation.Node ?? throw new ApplicationException("Missing implementation for lambda");

		var captured_variables = implementation.Captures;

		// Required memory is equal to memory required to store all the captured non-member variables, the function pointer and optionally the this pointer whose sizes are depedent on the chosen platform
		var required_memory = (long)captured_variables.Sum(v => v.Type!.ReferenceSize) + Assembler.Size.Bytes;

		// Allocate a memory structure which stores the lambda
		var lambda = Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, Types.LINK, new NumberNode(Assembler.Format, required_memory));

		// Store the function pointer first
		var function_pointer_location = new Result(new MemoryHandle(unit, lambda, 0), Assembler.Format);
		var function_pointer = new Result(new DataSectionHandle(node.Lambda.Implementation.GetFullname(), true), Assembler.Format);

		unit.Append(new MoveInstruction(unit, function_pointer_location, function_pointer));

		var position = Assembler.Size.Bytes;

		// Store each captured variable
		foreach (var captured_variable in captured_variables)
		{
			var source = References.GetVariable(unit, captured_variable.Captured, AccessMode.READ);
			var destination = new Result(new MemoryHandle(unit, lambda, position), captured_variable.Type!.Format);

			unit.Append(new MoveInstruction(unit, destination, source));

			position += captured_variable.Type!.ReferenceSize;
		}

		return lambda;
	}
}