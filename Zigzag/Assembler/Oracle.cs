using System.Collections.Generic;
using System.Linq;
using System;

using VariableGroup = System.Collections.Generic.List<Variable>;

public static class Oracle
{
	private static void DifferentiateDependencies(Unit unit, Result result)
	{
		// Get all variable dependencies
		var dependencies = result.Metadata.Secondary
			.Where(a => a.Type == AttributeType.VARIABLE)
			.Select(a => (VariableAttribute)a);

		// Ensure there is dependencies
		if (dependencies.Count() == 0)
		{
			return;
		}

		var primary_variable = (VariableAttribute)result.Metadata.Primary!;
		var secondary_variables = dependencies.Select(d => d.Variable);

		// Duplicate the current result and share it between the dependencies
		var duplicate = new DuplicateInstruction(unit, result);
		duplicate.Description = "Separate " + primary_variable.Variable.Name  + " from its dependencies { " + string.Join(", ", secondary_variables.Select(v => v.Name)) + " }";

		var duplication = duplicate.Execute();

		foreach (var dependency in dependencies)
		{
			// Redirect the dependency to use the result of the duplication
			//unit.Cache(dependency.Variable, duplication, true);
			unit.Append(new SetVariableInstruction(unit, dependency.Variable, duplication));

			duplication.Metadata.Attach(new VariableAttribute(dependency.Variable));
		}

		// Attach the primary attribute since it's still valid
		duplication.Metadata.Attach(result.Metadata.Primary!);
	}

	public static void DifferentiateTarget(Unit unit, Result result, Variable target)
	{
		// Duplicate the result and give it to the target
		var duplication = new DuplicateInstruction(unit, result).Execute();

		// Redirect the dependency to the target
		unit.Append(new SetVariableInstruction(unit, target, duplication));
		//unit.Cache(target, duplicate, true);

		duplication.Metadata.Attach(new VariableAttribute(target));
	}

	/// <summary>
	/// Resolves all write dependencies in the given result
	/// </summary>
	private static void Resolve(Unit unit, Result result, Variable variable)
	{
		var destination = (VariableAttribute?)result.Metadata.Primary;

		if (destination == null)
		{
			return;
		}

		if (destination.Variable == variable)
		{
			//DifferentiateDependencies(unit, result);
			var dependencies = GetAllDependencies(unit, result);

			if (dependencies.Count() <= 1)
			{
				return;
			}

			var duplicate = new DuplicateInstruction(unit, result);
			duplicate.Description = "Separate variable " + variable.Name;

			var duplication = duplicate.Execute();

			// Set the duplication result as a new value for the edited variable
			unit.Append(new SetVariableInstruction(unit, variable, duplication));

			duplication.Metadata.Attach(new VariableAttribute(variable));
		}
	}

	private static void SimulateMoves(Unit unit, Instruction i)
	{
		/*if (i.Type == InstructionType.ASSIGN)
		{
			var instruction = (DualParameterInstruction)i;
			var destination = instruction.First;
			var source = instruction.Second;

			// Check if the destination is a variable
			if (destination.Metadata.Primary is VariableAttribute attribute && attribute.Variable.IsPredictable)
			{
				unit.Cache(attribute.Variable, instruction.Result, true);

				// The source value now contains the new value of the destination
				source.Metadata.Attach(new VariableAttribute(attribute.Variable));
			}
		}
		else if (
			/// TODO: Must be generalized in the future
			(i is AdditionInstruction addition && addition.Assigns) ||
			(i is SubtractionInstruction subtraction && subtraction.Assigns) ||
			(i is MultiplicationInstruction multiplication && multiplication.Assigns) ||
			(i is DivisionInstruction division && division.Assigns)
		)
		{
			var instruction = (DualParameterInstruction)i;
			var destination = instruction.First;
			var source = instruction.Second;

			// Check if the destination is a variable
			if (destination.Metadata.Primary is VariableAttribute attribute && attribute.Variable.IsPredictable)
			{
				unit.Cache(attribute.Variable, instruction.Result, true);

				// The source value now contains the new value of the destination
				source.Metadata.Attach(new VariableAttribute(attribute.Variable));
			}
		}*/
	}

	private static bool IsPropertyOf(Variable expected, Result result)
	{
		return result.Metadata.Primary is VariableAttribute attribute && attribute.Variable == expected;
	}

	private static IEnumerable<Variable> GetAllDependencies(Unit unit, Result result)
	{
		return unit.Scope!.Variables.Where(p => p.Value.Equals(result)).Select(p => p.Key);
	}

	private static void SimulateLoads(Unit unit, Instruction instruction)
	{
		//#error Analyze if two variables share the same result, if so they are linked. If the two variables are linked, external and they are inside a conditional scope, they must be separated before the scope 
		if (instruction is GetVariableInstruction v)
		{
			var handle = unit.GetCurrentVariableHandle(v.Variable);

			if (handle != null)
			{
				if (v.Mode == AccessMode.WRITE)
				{
					Resolve(unit, handle, v.Variable);
				}

				v.Connect(handle);
			}
			else
			{
				v.Configure(References.CreateVariableHandle(unit, v.Self, v.Variable), new VariableAttribute(v.Variable));

				if (v.Mode != AccessMode.WRITE && v.Variable.IsPredictable)
				{		
					unit.Append(new SetVariableInstruction(unit, v.Variable, v.Result));
				}
			}
		}
		else if (instruction is GetConstantInstruction c)
		{
			var handle = unit.GetCurrentConstantHandle(c.Value);

			c.Configure(References.CreateConstantNumber(unit, c.Value));
		}
		else if (instruction is RequireVariablesInstruction r)
		{
			foreach (var variable in r.Variables)
			{
				var handle = unit.GetCurrentVariableHandle(variable);

				if (handle == null)
				{
					throw new ApplicationException("Couldn't get the current handle of a variable for a Require-Variables-Instruction");
				}

				r.References.Add(handle);
			}
		}
	}

	private static VariableGroup[] GetAllLinkedVariables(Unit unit)
	{
		var states = unit.Scope!.Variables.Keys.Select(variable => new VariableLoad 
		(
			variable,
			unit.GetCurrentVariableHandle(variable) ?? throw new ApplicationException("Couldn't get variable handle while analyzing linked variables")
		
		)).ToList();

		var groups = new List<VariableGroup>();

		while (states.Count > 0)
		{
			var state = states.Pop()!;

			// Try to find other states that share the same references with the current state
			var linked_variables = states.Where(s => s != state && s.Reference.Equals(state.Reference)).ToList();

			if (linked_variables.Count() > 0)
			{
				var group = linked_variables.Select(s => s.Variable).ToList();
				group.Add(state.Variable);

				// Add all the linked variables to the result list
				groups.Add(group);
				
				// Remove all the linked variables from the scan list since they won't match with other states anymore
				linked_variables.ForEach(s => states.Remove(s));
			}
		}

		return groups.ToArray();
	}

	private static void SeparateEditedVariablesInGroup(Unit unit, VariableGroup group, Node[] roots)
	{
		var edited_variables = new List<Variable>();

		foreach (var variable in group)
		{
			// Check if the variable is edited inside of any of the roots
			if (roots.Any(root => variable.IsEditedInside(root)))
			{
				edited_variables.Add(variable);
			}
		}

		if (edited_variables.Count == 0)
		{
			return;
		}

		// Get the shared value in the variable group
		var shared_value = unit.GetCurrentVariableHandle(group.First()) ?? throw new ApplicationException("Couldn't get current handle of a variable while separating a linked variable group");

		foreach (var variable in edited_variables)
		{
			// Duplicate the shared value and give it to the edited variable
			var duplicate = new DuplicateInstruction(unit, shared_value);
			duplicate.Description = "Separate " + variable.Name  + " from variable group { " + string.Join(", ", group.Select(v => v.Name)) + " }";

			var duplication = duplicate.Execute();

			// Set the duplication result as a new value for the edited variable
			unit.Append(new SetVariableInstruction(unit, variable, duplication));

			duplication.Metadata.Attach(new VariableAttribute(variable));
		}
	}

	private static void SimulateLinkage(Unit unit, Instruction instruction)
	{
		if (instruction.Type == InstructionType.PREPARE_FOR_CONDITIONAL_EXECUTION)
		{
			var roots = instruction.To<PrepareForConditionalExecutionInstruction>().Roots;
			var linked_variable_groups = GetAllLinkedVariables(unit);

			// Add all variable groups to the separation list which are affected inside any of the roots
			foreach (var linked_variable_group in linked_variable_groups)
			{
				SeparateEditedVariablesInGroup(unit, linked_variable_group, roots);
			}
		}
	}

	private static void SimulateCaching(Unit unit)
	{
		unit.Simulate(UnitPhase.APPEND_MODE, i =>
		{
			SimulateMoves(unit, i);
			SimulateLoads(unit, i);
			SimulateLinkage(unit, i);
		});
	}

	public static void SimulateLifetimes(Unit unit)
	{
		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			foreach (var result in instruction.GetAllUsedResults())
			{
				result.Lifetime.Reset();
			}
		});

		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			foreach (var result in instruction.GetAllUsedResults())
			{
				result.Use(unit.Position);
			}
		});
	}

	private static void TryRedirectToReturnRegister(Unit unit, Instruction instruction, Register register, IEnumerable<Instruction> calls)
	{
		// Get the instruction area that the redirection would affect
		var start = instruction.GetRedirectionRoot().Position;
		var end = instruction.Result.Lifetime.End; // NOTE: If any problems arise with weird register overwrites, it may be due to this (previously: unit.Position)
		
		// Check if the register is available
		if (register.Handle == null || !register.Handle.Lifetime.IsIntersecting(start, end))
		{
			// If the register is volatile, there must not be any function calls in the calculated instruction range since they would corrupt the register
			if (register.IsVolatile && calls.Any(c => c.Result.Lifetime.IsIntersecting(start, end)))
			{
				return;
			}

			instruction.Redirect(new RegisterHandle(register));
		}
	}

	private static void TryRedirect(Unit unit, Instruction instruction, Register register, IEnumerable<Instruction> calls)
	{
		// Get the instruction area that the redirection would affect
		var start = instruction.GetRedirectionRoot().Position;
		var end = instruction.Result.Lifetime.End; // NOTE: If any problems arise with weird register overwrites, it may be due to this (previously: unit.Position)
		
		// Check if the register is available
		if (register.Handle == null || !register.Handle.Lifetime.IsIntersecting(start, end))
		{
			// If the register is volatile, there must not be any function calls in the calculated instruction range since they would corrupt the register
			if (register.IsVolatile && calls.Any(c => c.Position >= start && c.Position <= end))
			{
				return;
			}

			instruction.Redirect(new RegisterHandle(register));
		}
	}

	private static void ConnectReturnStatements(Unit unit)
	{
		var calls = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (i is ReturnInstruction instruction)
			{
				var is_decimal = instruction.ReturnType == Types.DECIMAL;
				var return_register = is_decimal ? unit.GetDecimalReturnRegister() : unit.GetStandardReturnRegister();

				TryRedirectToReturnRegister(unit, instruction, return_register, calls);
			}
		});
	}

	public static void SimulateRegisterUsage(Unit unit)
	{
		var calls = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);

		// Try to redirect call parameters that follow x64 calling conventions
		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			if (instruction.Type == InstructionType.EVACUATE)
			{
				// Evacuate-Instruction moves all important values from volatile register to non-volatile registers or into memory, so resetting volatile registers is acceptable
				unit.VolatileRegisters.ForEach(r => r.Reset());
			}

			if (instruction is CallInstruction call && call.Convention == CallingConvention.X64)
			{
				foreach (var parameter in call.ParameterInstructions)
				{
					if (parameter is MoveInstruction move)
					{
						var source = move.Second.Instruction;
						
						if (source != null && move.First.Value is RegisterHandle destination)
						{
							TryRedirect(unit, source, destination.Register, calls);
						}
					}
				}
			}

			// When a call is made, it might corrupt all the volatile registers, so they must be reset
			if (instruction.Type == InstructionType.CALL)
			{
				unit.VolatileRegisters.ForEach(r => r.Reset());
			}
		});

		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			var result = instruction.Result;

			if (calls.Any(f => result.Lifetime.IsActive(f.Position) && result.Lifetime.Start != f.Position && result.Lifetime.End != f.Position) &&
				!(result.Value is RegisterHandle handle && !handle.Register.IsVolatile))
			{         
				// Get the instruction range that the redirection would affect
				var start = instruction.GetRedirectionRoot().Position;
				var end = instruction.Result.Lifetime.End;

				// Try to get the next non-volatile register which is available in the specified range
				var register = unit.GetNextNonVolatileRegister(start, end);

				// Check if any register satisfied the range condition
				if (register != null)
				{
					instruction.Redirect(new RegisterHandle(register));
					register.Handle = result;
				}
			}
		});
	}

	public static Unit Channel(Unit unit)
	{
		if (unit.Optimize)
		{
			SimulateCaching(unit);
		}

		SimulateLifetimes(unit);

		if (unit.Optimize)
		{
			ConnectReturnStatements(unit);
			SimulateRegisterUsage(unit);
		}

		return unit;
	}
}