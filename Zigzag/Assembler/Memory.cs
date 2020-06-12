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
		var locks = moves.Where(m => m.IsRedundant && m.First.IsRegister).Select(m => LockStateInstruction.Lock(unit, m.First.Value.To<RegisterHandle>().Register)).ToList();
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

            if (move.First.IsRegister)
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
				register = unit.GetNextRegisterWithoutReleasing();
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
			Memory.ClearRegister(unit, register);
		}

		var handle = new RegisterHandle(register);
		var instruction = new XorInstruction(unit, new Result(handle, register.Format), new Result(handle, register.Format))
		{
			IsSafe = false,
			Description = "Sets the value of the destination to zero"
		};

		unit.Append(instruction);
	}
	
	/// <summary>
	/// Copies the given result to a register
	/// </summary>
	public static Result CopyToRegister(Unit unit, Result result, bool media_register)
	{
		if (result.IsRegister)
		{
			using (RegisterLock.Create(result))
			{
				var register = media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
				var destination = new Result(new RegisterHandle(register), register.Format);

				return new MoveInstruction(unit, destination, result)
				{
					IsFutureUsageAnalyzed = false // Important: Prevents a future usage cycle (maybe)

				}.Execute();
			}
		}
		else
		{
			var register = media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
			var destination = new Result(new RegisterHandle(register), register.Format);

			return new MoveInstruction(unit, destination, result)
			{
				IsFutureUsageAnalyzed = false // Important: Prevents a future usage cycle (maybe)

			}.Execute();
		}
	}

	/// <summary>
	/// Moves the given result to a register
	/// </summary>
	public static Result MoveToRegister(Unit unit, Result result, bool media_register)
	{
		// Prevents reduntant moving to registers
		if (result.Value.Type == (media_register ? HandleType.MEDIA_REGISTER : HandleType.REGISTER))
		{
			return result;
		}

		var register = media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
		var destination = new Result(new RegisterHandle(register), register.Format);

		return new MoveInstruction(unit, destination, result)
		{
			IsFutureUsageAnalyzed = false, // Important: Prevents a future usage cycle
			Description = "Move source to register",
			Type = MoveType.RELOCATE

		}.Execute();
	}

	/// <summary>
	/// Moves the given result to an available register
	/// </summary>
	public static void GetRegisterFor(Unit unit, Result value)
	{
		var register = unit.GetNextRegister();

		register.Handle = value;
		value.Value = new RegisterHandle(register);
	}
	
	public static Result Convert(Unit unit, Result result, bool move, params HandleType[] types)
	{
		return Convert(unit, result, types, move, false);
	}

	public static Result Convert(Unit unit, Result result, HandleType[] types, bool move, bool protect)
	{
		foreach (var type in types)
		{
			if (result.Value.Type == type)
			{
				return result;
			}

			var converted = TryConvert(unit, result, type, protect);

			if (converted != null)
			{
				if (move)
				{
					result.Value = converted.Value;
				}

				return converted;
			}
		}

		throw new ArgumentException("Couldn't convert reference to the requested format");
	}

	private static Result? TryConvert(Unit unit, Result result, HandleType type, bool protect)
	{
		switch (type)
		{
			case HandleType.MEDIA_REGISTER:
			case HandleType.REGISTER:
			{
				var register = (RegisterHandle?)null;

				// If the result is empty, a new available register can be assigned to it
				if (result.IsEmpty)
				{
					Memory.GetRegisterFor(unit, result);
					return result;
				}

				if (type == HandleType.MEDIA_REGISTER)
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
							return CopyToRegister(unit, new Result(register, register.Format), type == HandleType.MEDIA_REGISTER);
						}
					}
					else
					{
						return new Result(register, register.Format);
					}
				}

				var destination = MoveToRegister(unit, result, type == HandleType.MEDIA_REGISTER);

				if (protect && !dying)
				{
					using (new RegisterLock(destination.Value.To<RegisterHandle>().Register))
					{
						return CopyToRegister(unit, destination, type == HandleType.MEDIA_REGISTER);
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