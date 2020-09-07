using System.Collections.Generic;
using System.Linq;
using System;

using VariableGroup = System.Collections.Generic.List<Variable>;

public static class Oracle
{
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
			var dependencies = GetAllDependencies(unit, result);

			if (dependencies.Count() <= 1)
			{
				return;
			}

         var duplicate = new DuplicateInstruction(unit, result)
         {
            Description = "Separate variable " + variable.Name
         };

         var duplication = duplicate.Execute();

			// Set the duplication result as a new value for the edited variable
			unit.Append(new SetVariableInstruction(unit, variable, duplication));

			duplication.Metadata.Attach(new VariableAttribute(variable));
		}
	}

	private static IEnumerable<Variable> GetAllDependencies(Unit unit, Result result)
	{
		return unit.Scope!.Variables.Where(p => p.Value.Equals(result)).Select(p => p.Key);
	}

	private static void SimulateLoads(Unit unit, Instruction instruction)
	{
		if (instruction is GetVariableInstruction v && v.Mode == AccessMode.WRITE)
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
				v.Configure(References.CreateVariableHandle(unit, v.Self, v.SelfType, v.Variable), new VariableAttribute(v.Variable));

				if (v.Mode != AccessMode.WRITE && v.Variable.IsPredictable)
				{		
					unit.Append(new SetVariableInstruction(unit, v.Variable, v.Result));
				}
			}
		}
		else if (instruction is RequireVariablesInstruction r)
		{
			foreach (var variable in r.Variables)
			{
				var handle = unit.GetCurrentVariableHandle(variable);

				if (handle == null)
				{
					throw new ApplicationException("Could not get the current handle of a variable for a Require-Variables-Instruction");
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
			unit.GetCurrentVariableHandle(variable) ?? throw new ApplicationException("Could not get variable handle while analyzing linked variables")
		
		)).ToList();

		var groups = new List<VariableGroup>();

		while (states.Count > 0)
		{
			var state = states.Pop()!;

			// Try to find other states that share the same references with the current state
			var linked_variables = states.Where(s => s.Reference.Equals(state.Reference)).ToList();

			if (linked_variables.Any())
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
		// Find all variables which are edited inside of any of the roots
		var edited_variables = group.Where(v => roots.Any(r => v.IsEditedInside(r))).ToList();

		if (edited_variables.Count == 0)
		{
			return;
		}

		// Get the shared value in the variable group
		var shared_value = unit.GetCurrentVariableHandle(group.First()) ?? throw new ApplicationException("Could not get current handle of a variable while separating a linked variable group");

		foreach (var variable in edited_variables)
		{
         // Duplicate the shared value and give it to the edited variable
         var duplicate = new DuplicateInstruction(unit, shared_value)
         {
            Description = "Separate " + variable.Name + " from variables " + string.Join(", ", group.Select(v => v.Name))
         };

         var duplication = duplicate.Execute();

			// Set the duplication result as a new value for the edited variable
			unit.Append(new SetVariableInstruction(unit, variable, duplication));

			duplication.Metadata.Attach(new VariableAttribute(variable));
		}
	}

	private static void SimulateLinkage(Unit unit, Instruction instruction)
	{
		if (instruction.Type == InstructionType.BRANCH)
		{
			var branch_instruction = instruction.To<BranchInstruction>();
			var branches = branch_instruction.Branches;

			var linked_variable_groups = GetAllLinkedVariables(unit);

			// Add all variable groups to the separation list which are affected inside any of the roots
			foreach (var linked_variable_group in linked_variable_groups)
			{
				SeparateEditedVariablesInGroup(unit, linked_variable_group, branches);
			}
		}
	}

	private static void SimulateCaching(Unit unit)
	{
		unit.Simulate(UnitPhase.APPEND_MODE, i =>
		{
			SimulateLoads(unit, i);
			SimulateLinkage(unit, i);
		});
	}
	
	public static void SimulateLifetimes(Unit unit)
	{
		var start = DateTime.Now;

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

	private static void TryRedirectToReturnRegister(ReturnInstruction instruction, Register register, IEnumerable<Instruction> calls)
	{
		// Get the instruction area that the redirection would affect
		var start = instruction.GetRedirectionRoot().Position;
		var end = instruction.Result.Lifetime.End; // NOTE: If any problems arise with weird register overwrites, it may be due to this (previously: unit.Position)
		
		// If the register is volatile, there must not be any function calls in the calculated instruction range since they would corrupt the register
		if (register.IsVolatile && calls.Any(c => c.Position >= start && c.Position < end))
		{
			return;
		}

		instruction.Direct(new DirectToReturnRegister(instruction));
	}

	private static void TryRedirect(Instruction objective, Instruction instruction, Register register, IEnumerable<Instruction> calls)
	{
		// Get the instruction area that the redirection would affect
		var start = instruction.GetRedirectionRoot().Position;
		var end = instruction.Result.Lifetime.End; // NOTE: If any problems arise with weird register overwrites, it may be due to this (previously: unit.Position)
		
		// If the register is volatile, there must not be any function calls in the calculated instruction range since they would corrupt the register
		if (register.IsVolatile && calls.Any(c => c.Position >= start && c.Position < end))
		{
			return;
		}

		instruction.Direct(new DirectToRegister(objective, register));
	}

	private static void ConnectReturnStatements(Unit unit)
	{
		var calls = unit.Instructions.FindAll(i => i.Is(InstructionType.CALL));

		unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
		{
			if (i is ReturnInstruction instruction)
			{
				var is_decimal = instruction.ReturnType == Types.DECIMAL;
				var return_register = is_decimal ? unit.GetDecimalReturnRegister() : unit.GetStandardReturnRegister();

				TryRedirectToReturnRegister(instruction, return_register, calls);
			}
		});
	}

	public static void SimulateRegisterUsage(Unit unit)
	{
		var calls = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);
		var divisions = unit.Instructions.FindAll(i => i.Type == InstructionType.DIVISION);

		// Try to redirect call parameters that follow x64 calling conventions
		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			if (instruction is CallInstruction call && call.Convention == CallingConvention.X64)
			{
				foreach (var parameter in call.ParameterInstructions)
				{
					if (parameter is MoveInstruction move)
					{
						var source = move.Second.Instruction;
						
						if (source != null && move.First.IsStandardRegister)
						{
							TryRedirect(call, source, move.First.Value.To<RegisterHandle>().Register, calls);
						}
					}
				}

				// Evacuate-Instruction moves all important values from volatile register to non-volatile registers or into memory, so resetting volatile registers is acceptable
				unit.VolatileRegisters.ForEach(r => r.Reset());
			}

			// When a call is made, it might corrupt all the volatile registers, so they must be reset
			if (instruction.Is(InstructionType.CALL))
			{
				unit.VolatileRegisters.ForEach(r => r.Reset());
			}
		});

		var nominator_register = unit.GetNominatorRegister();
		var remainder_register = unit.GetRemainderRegister();

		// Redirect values to non-volatile register if they intersect with functions
		unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
		{
			var result = instruction.Result;

			// Decimal values should not be redirected since they are usually in media registers
			if (result.Format.IsDecimal())
			{
				return;
			}

			var intersections = calls.FindAll(c => result.Lifetime.IsOnlyActive(c.Position));

			if (intersections.Count > 0 && 
				(!result.IsStandardRegister || result.Value.To<RegisterHandle>().Register.IsVolatile))
			{
				instruction.Direct(new DirectToNonVolatileRegister(intersections.ToArray()));
			}

			intersections = divisions.FindAll(i => result.Lifetime.IsOnlyActive(i.Position));

			if (intersections.Count > 0)
			{
				var last = intersections.OrderByDescending(i => i.Position).First();

				instruction.Direct(new AvoidRegisters(last, new Register[] { nominator_register, remainder_register }));
			}
		});
	}

	public static Unit Channel(Unit unit)
	{
		SimulateCaching(unit);
		SimulateLifetimes(unit);
		ConnectReturnStatements(unit);
		SimulateRegisterUsage(unit);

		return unit;
	}
}