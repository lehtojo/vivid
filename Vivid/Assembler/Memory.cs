using System;
using System.Collections.Generic;
using System.Linq;

public static class Memory
{
	/// <summary>
	/// Loads the operand so that it is ready based on the specified settings
	/// </summary>
	public static Result LoadOperand(Unit unit, Result operand, bool media_register, bool assigns)
	{
		if (!assigns) return operand;

		if (operand.IsMemoryAddress)
		{
			return Memory.CopyToRegister(unit, operand, Settings.Size, media_register, Trace.For(unit, operand));
		}
		else
		{
			Memory.MoveToRegister(unit, operand, Settings.Size, media_register, Trace.For(unit, operand));
		}

		return operand;
	}

	/// <summary>
	/// Minimizes intersection between the specified move instructions and tries to use exchange instructions
	/// </summary>
	private static List<Instruction> MinimizeIntersections(Unit unit, List<DualParameterInstruction> moves)
	{
		// Find moves that can be replaced with an exchange instruction
		var result = new List<DualParameterInstruction>(moves);
		var exchanges = new List<ExchangeInstruction>();
		var exchanged_indices = new SortedSet<int>();

		if (Settings.IsX64)
		{
			for (var i = 0; i < result.Count; i++)
			{
				for (var j = 0; j < result.Count; j++)
				{
					if (i == j || exchanged_indices.Contains(i) || exchanged_indices.Contains(j)) continue;

					var current = result[i];
					var other = result[j];

					// Can not exchange non-integer values
					if (current.First.Value.Format.IsDecimal() || current.Second.Value.Format.IsDecimal()) continue;

					if (current.First.Value.Equals(other.Second.Value) && current.Second.Value.Equals(other.First.Value))
					{
						exchanges.Add(new ExchangeInstruction(unit, current.Second, other.Second));
						exchanged_indices.Add(i);
						exchanged_indices.Add(j);
						break;
					}
				}
			}
		}

		// Add the created exchanges and remove the moves which were replaced by the exchanges
		result.AddRange(exchanges);
		exchanged_indices.OrderByDescending(i => i).ForEach(i => result.RemoveAt(i));

		// Order the move instructions so that intersections are minimized
		var optimized_indices = new SortedSet<int>();

		for (var i = 0; i < result.Count; i++)
		{
			for (var j = i + 1; j < result.Count; j++)
			{
				var a = result[i];
				var b = result[j];

				if (!a.First.Value.Equals(b.Second.Value)) continue;

				// Swap:
				result[i] = b;
				result[j] = a;
			}
		}

		return result.Select(i => (Instruction)i).ToList();
	}

	/// <summary>
	/// Aligns the specified moves so that intersections are minimized
	/// </summary>
	public static void Align(Unit unit, List<MoveInstruction> moves)
	{
		var complex = new List<Instruction>();

		// Execute complex moves before anything else, because they might require multiple steps
		for (var i = moves.Count - 1; i >= 0; i--)
		{
			var move = moves[i];

			if (!move.First.IsAnyRegister || move.Second.IsEmpty)
			{
				complex.Add(move);
				moves.RemoveAt(i);
			}
		}

		var locks = moves.Where(i => i.IsRedundant && i.First.IsStandardRegister).Select(i => LockStateInstruction.Lock(unit, i.First.Value.To<RegisterHandle>().Register)).ToList();
		var unlocks = locks.Select(i => LockStateInstruction.Unlock(unit, i.Register)).ToList();

		var aligned = MinimizeIntersections(unit, moves.Select(i => i.To<DualParameterInstruction>()).ToList());

		for (var i = aligned.Count - 1; i >= 0; i--)
		{
			var instruction = aligned[i];

			if (instruction.Is(InstructionType.EXCHANGE))
			{
				var exchange = instruction.To<ExchangeInstruction>();

				var first = exchange.First.Value.To<RegisterHandle>().Register;
				var second = exchange.Second.Value.To<RegisterHandle>().Register;

				aligned.Insert(i + 1, LockStateInstruction.Lock(unit, second));
				aligned.Insert(i + 1, LockStateInstruction.Lock(unit, first));

				unlocks.Add(LockStateInstruction.Unlock(unit, first));
				unlocks.Add(LockStateInstruction.Unlock(unit, second));
			}
			else if (instruction.Is(InstructionType.MOVE))
			{
				var move = instruction.To<MoveInstruction>();

				if (move.First.IsAnyRegister)
				{
					var register = move.First.Value.To<RegisterHandle>().Register;

					aligned.Insert(i + 1, LockStateInstruction.Lock(unit, register));
					unlocks.Add(LockStateInstruction.Unlock(unit, register));
				}
			}
			else
			{
				throw new ApplicationException("Unsupported instruction type found while aligning move instructions");
			}
		}

		var result = complex.Concat(locks).Concat(aligned).Concat(unlocks).Reverse();

		foreach (var instruction in result)
		{
			unit.Add(instruction, true);
		}
	}

	/// <summary>
	/// Moves the value inside the given register to other register or releases it memory
	/// </summary>
	public static void ClearRegister(Unit unit, Register target)
	{
		// 1. If the register is already available, no need to clear it
		// 2. If the value inside the register does not own the register, no need to clear it
		if (target.IsAvailable() || target.IsHandleCopy()) return;

		var register = (Register?)null;

		target.Lock();

		var directives = (List<Directive>?)null;
		if (target.Value != null) { directives = Trace.For(unit, target.Value); }

		register = GetNextRegisterWithoutReleasing(unit, target.IsMediaRegister, directives);

		target.Unlock();

		if (register == null)
		{
			unit.Release(target);
			return;
		}

		if (target.Value == null) return;

		var destination = new RegisterHandle(register);

		unit.Add(new MoveInstruction(unit, new Result(destination, target.Value.Format), target.Value!)
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
		if (!register.IsAvailable()) ClearRegister(unit, register);

		var handle = new RegisterHandle(register);

		var instruction = BitwiseInstruction.CreateXor(unit, new Result(handle, Settings.Format), new Result(handle, Settings.Format), Settings.Format);
		instruction.Description = "Sets the value of the destination to zero";

		unit.Add(instruction);
	}

	/// <summary>
	/// Tries to apply the specified directive
	/// </summary>
	private static Register? Consider(Unit unit, Directive directive, bool media_register)
	{
		return directive.Type switch
		{
			DirectiveType.NON_VOLATILITY => unit.GetNextNonVolatileRegister(media_register, false),
			DirectiveType.AVOID_REGISTERS => media_register ? unit.GetNextMediaRegisterWithoutReleasing(directive.To<AvoidRegistersDirective>().Registers) : unit.GetNextRegisterWithoutReleasing(directive.To<AvoidRegistersDirective>().Registers),
			DirectiveType.SPECIFIC_REGISTER => directive.To<SpecificRegisterDirective>().Register,
			_ => throw new ArgumentException("Unknown directive type encountered")
		};
	}

	/// <summary>
	/// Determines the next register to use
	/// </summary>
	public static Register GetNextRegister(Unit unit, bool media_register, List<Directive>? directives = null, bool is_result = false)
	{
		var register = (Register?)null;

		if (directives != null)
		{
			foreach (var directive in directives)
			{
				var result = Consider(unit, directive, media_register);

				if (result == null || media_register != result.IsMediaRegister) continue;

				if (is_result ? (result.IsAvailable() || result.IsDeactivating()) : result.IsAvailable())
				{
					register = result;
					break;
				}
			}
		}

		if (register == null)
		{
			return media_register ? unit.GetNextMediaRegister() : unit.GetNextRegister();
		}

		return register;
	}

	/// <summary>
	/// Tries to get a register without releasing based on the specified directives
	/// </summary>
	public static Register? GetNextRegisterWithoutReleasing(Unit unit, bool media_register, List<Directive>? directives = null)
	{
		var register = (Register?)null;

		if (directives != null)
		{
			foreach (var directive in directives)
			{
				var result = Consider(unit, directive, media_register);

				if (result != null && media_register == result.IsMediaRegister && result.IsAvailable())
				{
					register = result;
					break;
				}
			}
		}

		if (register == null)
		{
			return media_register ? unit.GetNextMediaRegisterWithoutReleasing() : unit.GetNextRegisterWithoutReleasing();
		}

		return register;
	}

	/// <summary>
	/// Copies the specified result to a register
	/// </summary>
	public static Result CopyToRegister(Unit unit, Result result, Size size, bool media_register, List<Directive>? directives = null)
	{
		// NOTE: Before the condition checked whether the result was a standard register, so it was changed to check any register, since why would not media registers need register locks as well
		var format = media_register ? Format.DECIMAL : size.ToFormat(result.Format.IsUnsigned());

		if (result.IsAnyRegister)
		{
			var source = result.Register;

			source.Lock();
			var register = GetNextRegister(unit, media_register, directives);
			var destination = new Result(new RegisterHandle(register), format);
			result = new MoveInstruction(unit, destination, result).Add();
			source.Unlock();

			return result;
		}
		else
		{
			var register = GetNextRegister(unit, media_register, directives);
			var destination = new Result(new RegisterHandle(register), format);

			return new MoveInstruction(unit, destination, result).Add();
		}
	}

	/// <summary>
	/// Moves the specified result to a register considering the specified directives
	/// </summary>
	public static Result MoveToRegister(Unit unit, Result result, Size size, bool media_register, List<Directive>? directives = null)
	{
		// Prevents redundant moving to registers
		if (result.Value.Type == (media_register ? HandleType.MEDIA_REGISTER : HandleType.REGISTER)) return result;

		var format = media_register ? Format.DECIMAL : size.ToFormat(result.Format.IsUnsigned());
		var register = GetNextRegister(unit, media_register, directives);
		var destination = new Result(new RegisterHandle(register), format);

		return new MoveInstruction(unit, destination, result)
		{
			Description = "Move source to register",
			Type = MoveType.RELOCATE

		}.Add();
	}

	/// <summary>
	/// Moves the specified result to a register considering the specified directives
	/// </summary>
	public static Result Convert(Unit unit, Result result, Size size, List<Directive>? directives = null)
	{
		Register? register = null;
		Result? destination;

		var format = result.Format.IsDecimal() ? Format.DECIMAL : size.ToFormat(result.Format.IsUnsigned());

		if (result.IsMediaRegister)
		{
			return result;
		}
		else if (result.IsConstant)
		{
			result.Format = format;
			return result;
		}
		else if (result.IsStandardRegister)
		{
			if (result.Size.Bytes >= size.Bytes) return result;
		}
		else if (result.IsMemoryAddress)
		{
			register = GetNextRegister(unit, format.IsDecimal(), directives);
			destination = new Result(new RegisterHandle(register), format);

			return new MoveInstruction(unit, destination, result)
			{
				Description = "Converts the format of the source operand",
				Type = MoveType.RELOCATE

			}.Add();
		}
		else
		{
			throw new ArgumentException("Unsupported conversion requested");
		}

		// Use the register of the result to extend the value
		/// NOTE: This will always extend the value, so there will be no loss of information
		register = result.Value.To<RegisterHandle>().Register;

		destination = new Result(new RegisterHandle(register), format);

		return new MoveInstruction(unit, destination, result)
		{
			Description = "Converts the format of the source operand",
			Type = MoveType.RELOCATE

		}.Add();
	}

	/// <summary>
	/// Moves the specified result to an available register
	/// </summary>
	public static void GetRegisterFor(Unit unit, Result result, bool unsigned, bool media_register)
	{
		var register = GetNextRegister(unit, media_register, Trace.For(unit, result));

		register.Value = result;

		result.Value = new RegisterHandle(register);
		result.Format = media_register ? Format.DECIMAL : Instruction.GetSystemFormat(unsigned);
	}

	/// <summary>
	/// Moves the specified result to an available register
	/// </summary>
	public static void GetResultRegisterFor(Unit unit, Result result, bool unsigned, bool media_register)
	{
		var register = GetNextRegister(unit, media_register, Trace.For(unit, result), true);

		register.Value = result;

		result.Value = new RegisterHandle(register);
		result.Format = media_register ? Format.DECIMAL : Instruction.GetSystemFormat(unsigned);
	}

	public static Result Convert(Unit unit, Result result, Size size, HandleType[] types, bool protect, List<Directive>? directives)
	{
		foreach (var type in types)
		{
			var converted = TryConvert(unit, result, size, type, protect, directives);
			if (converted != null) return converted;
		}

		throw new ArgumentException("Could not convert the result to the requested format");
	}

	private static Result? TryConvert(Unit unit, Result result, Size size, HandleType type, bool protect, List<Directive>? directives)
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
					GetRegisterFor(unit, result, result.Format.IsUnsigned(), is_media_register);
					return result;
				}

				// If the format does not match the required register type, only copy it since the conversion may be lossy
				if (result.Format.IsDecimal() != is_media_register)
				{
					return CopyToRegister(unit, result, size, is_media_register, directives);
				}

				var expiring = result.IsDeactivating();

				// The result must be loaded into a register
				if (protect && !expiring)
				{
					// Do not use the directives here, because it would conflict with the upcoming copy
					result = MoveToRegister(unit, result, size, is_media_register, null);
					var register = result.Register;

					// Now copy the registered value into another register using the directives
					register.Lock();
					result = CopyToRegister(unit, result, size, is_media_register, directives);
					register.Unlock();

					return result;
				}

				return MoveToRegister(unit, result, size, is_media_register, directives);
			}

			case HandleType.MEMORY:
			{
				if (!Settings.IsArm64 || !result.IsDataSectionHandle) return null;

				var handle = result.Value.To<DataSectionHandle>();

				// Example:
				// S0
				// =>
				// adrp x0, :got:S0
				// ldr x0, [x0, :got_lo12:S0]
				var intermediate = new GetRelativeAddressInstruction(unit, handle).Add();
				var offset = new Result(new Lower12Bits(handle, true), Settings.Format);
				var address = Memory.MoveToRegister(unit, new Result(new ComplexMemoryHandle(intermediate, offset, 1), Settings.Format), Settings.Size, false);

				return new Result(new MemoryHandle(unit, address, (int)handle.Offset), result.Format);
			}

			case HandleType.NONE:
			{
				throw new ApplicationException("Tried to convert to an empty handle");
			}

			default: return null;
		}
	}
}