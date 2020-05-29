using System;

public static class Memory
{
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

		if (target.IsVolatile)
		{
			register = unit.GetNextRegisterWithoutReleasing();
		}
		else
		{
			register = unit.GetNextNonVolatileRegister(false);
		}

		if (register == null)
		{
			unit.Release(target);
			return;
		}

		var destination = new RegisterHandle(register);
		
		var move = new MoveInstruction(unit, new Result(destination), target.Handle!);
		move.Type = MoveType.RELOCATE;

		unit.Append(move);

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
		var instruction = new XorInstruction(unit, new Result(handle), new Result(handle))
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
		if (result.Value.Type == HandleType.REGISTER)
		{
			using (RegisterLock.Create(result))
			{
				var register = media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
				var destination = new Result(new RegisterHandle(register));

				var move = new MoveInstruction(unit, destination, result);
				move.IsFutureUsageAnalyzed = false; // Important: Prevents a future usage cycle (maybe)
		
				return move.Execute();
			}
		}
		else
		{
			var register = media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
			var destination = new Result(new RegisterHandle(register));

			var move = new MoveInstruction(unit, destination, result);
			move.IsFutureUsageAnalyzed = false; // Important: Prevents a future usage cycle (maybe)
		
			return move.Execute();
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
		var destination = new Result(new RegisterHandle(register));

		var move = new MoveInstruction(unit, destination, result);
		move.IsFutureUsageAnalyzed = false; // Important: Prevents a future usage cycle
		move.Description = "Move source to register";
		move.Type = MoveType.RELOCATE;
		
		return move.Execute();
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
			if (result.Empty)
			{
				throw new ApplicationException("Tried to convert an empty result");
			}

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
							return CopyToRegister(unit, new Result(register), type == HandleType.MEDIA_REGISTER);
						}
					}
					else
					{
						return new Result(register);
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
				throw new ApplicationException("Tried to convert none-handle");
			}

			default: return null;
		}
	}
}