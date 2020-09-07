using System;
using System.Collections.Generic;
using System.Linq;

public static class Memory
{
	/// <summary>
	/// Minimizes intersection between the specified move instructions and tries to use exchange instructions
	/// </summary>
	private static List<Instruction> OptimizeMoves(Unit unit, List<DualParameterInstruction> moves)
	{
		// Find moves that can be replaced with an exchange instruction
		var result = new List<DualParameterInstruction>(moves);
		var exchanges = new List<ExchangeInstruction>();
		var exchanged_indices = new SortedSet<int>();

		for (var i = 0; i < result.Count; i++)
		{
			for (var j = 0; j < result.Count; j++)
			{
				if (i == j || exchanged_indices.Contains(i) || exchanged_indices.Contains(j)) continue;

				var current = result[i];
				var other = result[j];

				if (current.First.Value.Equals(other.Second.Value) &&
					current.Second.Value.Equals(other.First.Value))
				{
					exchanges.Add(new ExchangeInstruction(unit, current.Second, other.Second, false));
					exchanged_indices.Add(i);
					exchanged_indices.Add(j);
					break;
				}
			}
		}

		// Append the created exchanges and remove the moves which were replaced by the exchanges
		result.AddRange(exchanges);
		exchanged_indices.OrderByDescending(i => i).ForEach(i => result.RemoveAt(i));

		// Order the move instructions so that intersections are minimized
		var optimized_indices = new SortedSet<int>();

		Start:
			
		for (var i = 0; i < result.Count; i++)
		{
			for (var j = i; j < result.Count; j++)
			{
				if (i == j || optimized_indices.Contains(i) || optimized_indices.Contains(j)) continue;
					
				var a = result[i];
				var b = result[j];
				
				if (a.First.Value.Equals(b.Second.Value))
				{
					// Swap
					var y = result[j];
					result.RemoveAt(j);

					var x = result[i];
					result.RemoveAt(i);

					result.Insert(i, y);
					result.Insert(j, x);

					optimized_indices.Add(i);
					optimized_indices.Add(j);

					goto Start;
				}
			}
		}

		return result.Select(i => (Instruction)i).ToList();
	}

	/// <summary>
	/// Safely executes the specified move instructions, making sure that no value is corrupted
	/// </summary>
	public static List<Instruction> Relocate(Unit unit, List<MoveInstruction> moves)
	{
		var locks = moves.Where(m => m.IsRedundant && m.First.IsStandardRegister).Select(m => LockStateInstruction.Lock(unit, m.First.Value.To<RegisterHandle>().Register)).ToList();
		var unlocks = locks.Select(l => LockStateInstruction.Unlock(unit, l.Register)).ToList();

		// Now remove all redundant moves
		moves.RemoveAll(m => m.IsRedundant);

		var optimized = OptimizeMoves(unit, moves.Select(m => m.To<DualParameterInstruction>()).ToList());

      for (var i = optimized.Count - 1; i >= 0; i--)
      {
         var instruction = optimized[i];

         if (instruction.Is(InstructionType.EXCHANGE))
         {
            var exchange = instruction.To<ExchangeInstruction>();

            var first = exchange.First.Value.To<RegisterHandle>().Register;
            var second = exchange.Second.Value.To<RegisterHandle>().Register;

            optimized.Insert(i + 1, LockStateInstruction.Lock(unit, second));
            optimized.Insert(i + 1, LockStateInstruction.Lock(unit, first));

            unlocks.Add(LockStateInstruction.Unlock(unit, first));
            unlocks.Add(LockStateInstruction.Unlock(unit, second));
         }
         else if (instruction.Is(InstructionType.MOVE))
         {
            var move = instruction.To<MoveInstruction>();

            if (move.First.IsStandardRegister)
            {
               var register = move.First.Value.To<RegisterHandle>().Register;

               optimized.Insert(i + 1, LockStateInstruction.Lock(unit, register));
               unlocks.Add(LockStateInstruction.Unlock(unit, register));
            }
         }
         else
         {
            throw new ApplicationException("Unsupported instruction type found while optimizing relocation");
         }
      }
		
		return locks.Concat(optimized.Concat(unlocks)).ToList();
	}

	/// <summary>
	/// Moves the value inside the given register to other register or releases it memory
	/// </summary>
	public static void ClearRegister(Unit unit, Register target)
	{
		if (target.IsAvailable(unit.Position))
		{
			return;
		}

		var register = (Register?)null;	

		using (RegisterLock.Create(target))
		{
			if (target.IsVolatile)
			{	
				if (target.IsMediaRegister)
				{
					register = unit.GetNextMediaRegisterWithoutReleasing();
				}
				else
				{
					register = unit.GetNextRegisterWithoutReleasing();
				}
			}
			else
			{
				register = unit.GetNextNonVolatileRegister(false);
			}
		}

		if (register == null)
		{
			unit.Release(target);
			return;
		}

		var destination = new RegisterHandle(register);

		unit.Append(new MoveInstruction(unit, new Result(destination, register.Format), target.Handle!)
		{
			Type = MoveType.RELOCATE,
			Description = "Relocates the source value so that the register is cleared for another purpose"
		});

		target.Reset();
	}

	/// <summary>
	/// Sets the value of the register to zero
	/// </summary>
	public static void Zero(Unit unit, Register register)
	{
		if (!register.IsAvailable(unit.Position))
		{
			ClearRegister(unit, register);
		}

		var handle = new RegisterHandle(register);

		var instruction = BitwiseInstruction.Xor(unit, new Result(handle, register.Format), new Result(handle, register.Format), register.Format);
		instruction.Description = "Sets the value of the destination to zero";

		unit.Append(instruction);
	}

	/// <summary>
	/// Tries to apply the hint
	/// </summary>
	public static Register? Consider(Unit unit, Hint hint, bool media_register)
	{
		if (hint is DirectToNonVolatileRegister)
		{
			// Try to get an available non-volatile register
			return unit.GetNextNonVolatileRegister(false);			
		}
		else if (hint is DirectToReturnRegister x)
		{
			// Try to direct towards the next return register
			var register = x.GetClosestReturnInstruction(unit.Position).ReturnRegister;

			return register.IsAvailable(unit.Position) ? register : null;
		}
		else if (hint is AvoidRegisters y)
		{
			// Try to filter the specified registers
			return media_register ? unit.GetNextMediaRegisterWithoutReleasing(y.Registers) : unit.GetNextRegisterWithoutReleasing(y.Registers);
		}
		else if (hint is DirectToRegister z)
		{
			return z.Register.IsAvailable(unit.Position) ? z.Register : null;
		}

		return null;
	}

	/// <summary>
	/// Determines the next register
	/// </summary>
	private static Register GetNextRegister(Unit unit, bool media_register, Hint? hint = null)
	{
		var register = hint != null ? Consider(unit, hint, media_register) : null;

		if (register == null || !register.IsAvailable(unit.Position))
		{
			return media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
		}

		return register;
	}
	
	/// <summary>
	/// Copies the given result to a register
	/// </summary>
	public static Result CopyToRegister(Unit unit, Result result, bool media_register, Hint? hint = null)
	{
		if (result.IsStandardRegister)
		{
			using (RegisterLock.Create(result))
			{
				var register = GetNextRegister(unit, media_register, hint);
				var destination = new Result(new RegisterHandle(register), register.Format);

				return new MoveInstruction(unit, destination, result).Execute();
			}
		}
		else
		{
			var register = GetNextRegister(unit, media_register, hint);
			var destination = new Result(new RegisterHandle(register), register.Format);

			return new MoveInstruction(unit, destination, result).Execute();
		}
	}

	/// <summary>
	/// Moves the given result to a register considering the specified hints
	/// </summary>
	public static Result MoveToRegister(Unit unit, Result result, bool media_register, Hint? hint = null)
	{
		// Prevents reduntant moving to registers
		if (result.Value.Type == (media_register ? HandleType.MEDIA_REGISTER : HandleType.REGISTER))
		{
			return result;
		}

		var register = GetNextRegister(unit, media_register, hint);
		var destination = new Result(new RegisterHandle(register), register.Format);

		return new MoveInstruction(unit, destination, result)
		{
			Description = "Move source to register",
			Type = MoveType.RELOCATE

		}.Execute();
	}

	/// <summary>
	/// Moves the given result to a register considering the specified hints
	/// </summary>
	public static Result Convert(Unit unit, Result result, Format format, Hint? hint = null)
	{
		if (result.IsMediaRegister)
		{
			return result;
		}

		if (!result.IsStandardRegister)
		{
			throw new InvalidOperationException("Tried to convert format but the result was not in a register");
		}

		Register? register = null;

		// Try to use the specified hint
		if (hint != null)
		{
			register = Consider(unit, hint, false);
		}

		// If the hint did not produce any register, the register of the result can be used
		if (register == null)
		{
			register = result.Value.To<RegisterHandle>().Register;
		}

		var destination = new Result(new RegisterHandle(register), format);

		return new MoveInstruction(unit, destination, result)
		{
			Description = "Converts the format of the source operand",
			Type = MoveType.RELOCATE

		}.Execute();
	}

	/// <summary>
	/// Moves the given result to an available register
	/// </summary>
	public static void GetRegisterFor(Unit unit, Result value, bool media_register)
	{
		var register = GetNextRegister(unit, media_register, value.GetRecommendation(unit));

		register.Handle = value;
		value.Value = new RegisterHandle(register);
	}

	public static Result Convert(Unit unit, Result result, HandleType[] types, bool protect, Hint? recommendation)
	{
		foreach (var type in types)
		{
			var converted = TryConvert(unit, result, type, protect, recommendation);

			if (converted != null)
			{
				return converted;
			}
		}

		throw new ArgumentException("Could not convert reference to the requested format");
	}

	private static Result? TryConvert(Unit unit, Result result, HandleType type, bool protect, Hint? recommendation)
	{
		switch (type)
		{
			case HandleType.MEDIA_REGISTER:
			case HandleType.REGISTER:
			{
				var is_media_register = type == HandleType.MEDIA_REGISTER;

				// If the result is empty, a new available register can be assigned to it
				if (result.IsEmpty)
				{
					GetRegisterFor(unit, result, is_media_register);
					return result;
				}

				RegisterHandle? register;

				if (is_media_register)
				{
					register = unit.TryGetCachedMediaRegister(result);
				}
				else
				{
					register = unit.TryGetCached(result);
				}

				var dying = result.IsExpiring(unit.Position);

				if (register != null)
				{
					if (protect && !dying)
					{
						using (new RegisterLock(register.Register))
						{
							return CopyToRegister(unit, new Result(register, register.Format), is_media_register);
						}
					}
					else
					{
						return new Result(register, register.Format);
					}
				}

				// The result must be loaded into a register
				// The recommendation should not be given to the load instructions if copying is needed, since it would conflict with the copy instructions
				var copy = protect && !dying;
				var destination = MoveToRegister(unit, result, is_media_register, copy ? null : recommendation);

				if (copy)
				{
					using (new RegisterLock(destination.Value.To<RegisterHandle>().Register))
					{
						return CopyToRegister(unit, destination, is_media_register, recommendation);
					}
				}

				return destination;
			}

			case HandleType.NONE:
			{
				throw new ApplicationException("Tried to convert to an empty handle");
			}

			default: return null;
		}
	}
}