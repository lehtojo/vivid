using System.Collections.Generic;
using System.Linq;
using System;

public static class Oracle
{
	private static void SimulateLoads(Unit unit, Instruction instruction)
	{
		if (instruction is RequireVariablesInstruction r)
		{
			foreach (var variable in r.Variables)
			{
				var handle = unit.GetCurrentVariableHandle(variable);

				if (handle == null)
				{
					continue;
				}

				r.References.Add(handle);
			}
		}
	}

	private static void SimulateCaching(Unit unit)
	{
		unit.Simulate(UnitPhase.APPEND_MODE, i =>
		{
			SimulateLoads(unit, i);
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

		instruction.Redirect(new DirectToReturnRegister(instruction));
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

		instruction.Redirect(new DirectToRegister(objective, register));
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

		var numerator_register = unit.GetNumeratorRegister();
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
				instruction.Redirect(new DirectToNonVolatileRegister(intersections.ToArray()));
			}

			intersections = divisions.FindAll(i => result.Lifetime.IsOnlyActive(i.Position));

			if (intersections.Count > 0)
			{
				var last = intersections.OrderByDescending(i => i.Position).First();

				instruction.Redirect(new AvoidRegisters(last, new Register[] { numerator_register, remainder_register }));
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