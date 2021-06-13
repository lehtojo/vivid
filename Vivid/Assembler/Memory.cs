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
		if (!assigns)
		{
			return operand;
		}

		if (operand.IsMemoryAddress)
		{
			return Memory.CopyToRegister(unit, operand, Assembler.Size, media_register, Trace.GetDirectives(unit, operand));
		}
		else
		{
			Memory.MoveToRegister(unit, operand, Assembler.Size, media_register, Trace.GetDirectives(unit, operand));
		}

		return operand;
	}

	/// <summary>
	/// Minimizes intersection between the specified move instructions and tries to use exchange instructions
	/// </summary>
	private static List<Instruction> Align(Unit unit, List<DualParameterInstruction> moves)
	{
		// Find moves that can be replaced with an exchange instruction
		var result = new List<DualParameterInstruction>(moves);
		var exchanges = new List<ExchangeInstruction>();
		var exchanged_indices = new SortedSet<int>();

		if (Assembler.IsX64)
		{
			for (var i = 0; i < result.Count; i++)
			{
				for (var j = 0; j < result.Count; j++)
				{
					if (i == j || exchanged_indices.Contains(i) || exchanged_indices.Contains(j))
					{
						continue;
					}

					var current = result[i];
					var other = result[j];

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
				if (i == j || optimized_indices.Contains(i) || optimized_indices.Contains(j))
				{
					continue;
				}

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
	public static List<Instruction> Align(Unit unit, List<MoveInstruction> moves)
	{
		return Align(unit, moves, out _);
	}

	/// <summary>
	/// Safely executes the specified move instructions, making sure that no value is corrupted
	/// </summary>
	public static List<Instruction> Align(Unit unit, List<MoveInstruction> moves, out List<Register> registers)
	{
		var locks = moves.Where(i => i.IsRedundant && i.First.IsStandardRegister).Select(i => LockStateInstruction.Lock(unit, i.First.Value.To<RegisterHandle>().Register)).ToList();
		var unlocks = locks.Select(i => LockStateInstruction.Unlock(unit, i.Register)).ToList();

		registers = locks.Select(i => i.Register).ToList();

		// Now remove all redundant moves
		moves.RemoveAll(m => m.IsRedundant);

		var optimized = Align(unit, moves.Select(i => i.To<DualParameterInstruction>()).ToList());

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

				registers.Add(first);
				registers.Add(second);
			}
			else if (instruction.Is(InstructionType.MOVE))
			{
				var move = instruction.To<MoveInstruction>();

				if (move.First.IsAnyRegister)
				{
					var register = move.First.Value.To<RegisterHandle>().Register;

					optimized.Insert(i + 1, LockStateInstruction.Lock(unit, register));
					unlocks.Add(LockStateInstruction.Unlock(unit, register));

					registers.Add(register);
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
			var directives = (List<Directive>?)null;

			if (target.Handle != null)
			{
				directives = Trace.GetDirectives(unit, target.Handle);
			}

			register = GetNextRegisterWithoutReleasing(unit, target.IsMediaRegister, directives);
		}

		if (register == null)
		{
			unit.Release(target);
			return;
		}

		if (target.Handle == null)
		{
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
	/// Tries to apply the most important directive
	/// </summary>
	public static Register? Consider(Unit unit, List<Directive> directives, bool media_register)
	{
		var register = (Register?)null;

		foreach (var directive in directives)
		{
			var result = Consider(unit, directive, media_register);

			if (result != null && media_register == result.IsMediaRegister && result.IsAvailable(unit.Position))
			{
				register = result;
				break;
			}
		}

		return register;
	}

	/// <summary>
	/// Determines the next register
	/// </summary>
	public static Register GetNextRegister(Unit unit, bool media_register, List<Directive>? directives = null, bool is_result = false)
	{
		var register = (Register?)null;

		if (directives != null)
		{
			foreach (var directive in directives)
			{
				var result = Consider(unit, directive, media_register);

				if (result != null && media_register == result.IsMediaRegister && (is_result ? result.IsAvailable(unit.Position + 1) : result.IsAvailable(unit.Position)))
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
	/// Tries to get a register without releasing based on the specified hint
	/// </summary>
	private static Register? GetNextRegisterWithoutReleasing(Unit unit, bool media_register, List<Directive>? directives = null)
	{
		var register = (Register?)null;

		if (directives != null)
		{
			foreach (var directive in directives)
			{
				var result = Consider(unit, directive, media_register);

				if (result != null && media_register == result.IsMediaRegister && result.IsAvailable(unit.Position))
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
	/// Copies the given result to a register
	/// </summary>
	public static Result CopyToRegister(Unit unit, Result result, Size size, bool media_register, List<Directive>? directives = null)
	{
		// NOTE: Before the condition checked whether the result was a standard register, so it was changed to check any register, since why would not media registers need register locks as well
		var format = media_register ? Format.DECIMAL : size.ToFormat();

		if (result.IsAnyRegister)
		{
			using (RegisterLock.Create(result))
			{
				var register = GetNextRegister(unit, media_register, directives);
				var destination = new Result(new RegisterHandle(register), format);

				return new MoveInstruction(unit, destination, result).Execute();
			}
		}
		else
		{
			var register = GetNextRegister(unit, media_register, directives);
			var destination = new Result(new RegisterHandle(register), format);

			return new MoveInstruction(unit, destination, result).Execute();
		}
	}

	/// <summary>
	/// Moves the given result to a register considering the specified hints
	/// </summary>
	public static Result MoveToRegister(Unit unit, Result result, Size size, bool media_register, List<Directive>? directives = null)
	{
		// Prevents redundant moving to registers
		if (result.Value.Type == (media_register ? HandleType.MEDIA_REGISTER : HandleType.REGISTER))
		{
			return result;
		}

		var format = media_register ? Format.DECIMAL : size.ToFormat();
		var register = GetNextRegister(unit, media_register, directives);
		var destination = new Result(new RegisterHandle(register), format);

		return new MoveInstruction(unit, destination, result)
		{
			Description = "Move source to register",
			Type = MoveType.RELOCATE

		}.Execute();
	}

	/// <summary>
	/// Moves the given result to a register considering the specified hints
	/// </summary>
	public static Result Convert(Unit unit, Result result, Size size, List<Directive>? directives = null)
	{
		Register? register = null;
		Result? destination;

		var format = result.Format.IsDecimal() ? Format.DECIMAL : size.ToFormat();

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

			}.Execute();
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

		}.Execute();
	}

	/// <summary>
	/// Moves the specified result to an available register
	/// </summary>
	public static void GetRegisterFor(Unit unit, Result result, bool media_register)
	{
		var register = GetNextRegister(unit, media_register, Trace.GetDirectives(unit, result));

		register.Handle = result;

		result.Value = new RegisterHandle(register);
		result.Format = media_register ? Format.DECIMAL : Assembler.Format;
	}

	/// <summary>
	/// Moves the specified result to an available register
	/// </summary>
	public static void GetResultRegisterFor(Unit unit, Result result, bool media_register)
	{
		var register = GetNextRegister(unit, media_register, Trace.GetDirectives(unit, result), true);

		register.Handle = result;

		result.Value = new RegisterHandle(register);
		result.Format = media_register ? Format.DECIMAL : Assembler.Format;
	}

	public static Result Convert(Unit unit, Result result, Size size, HandleType[] types, bool protect, List<Directive>? directives)
	{
		foreach (var type in types)
		{
			var converted = TryConvert(unit, result, size, type, protect, directives);
			if (converted != null) return converted;
		}

		throw new ArgumentException("Could not convert reference to the requested format");
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
					GetRegisterFor(unit, result, is_media_register);
					return result;
				}

				// If the format does not match the required register type, only copy it since the conversion may be lossy
				if (result.Format.IsDecimal() != is_media_register)
				{
					return CopyToRegister(unit, result, size, is_media_register, directives);
				}

				var expiring = result.IsExpiring(unit.Position);

				// The result must be loaded into a register
				// The recommendation should not be given to the load instructions if copying is needed, since it would conflict with the copy instructions
				var copy = protect && !expiring;
				var destination = MoveToRegister(unit, result, size, is_media_register, copy ? null : directives);

				// If no copying is needed, the loaded value can be returned right away
				if (!copy)
				{
					return destination;
				}

				using (new RegisterLock(destination.Value.To<RegisterHandle>().Register))
				{
					return CopyToRegister(unit, destination, size, is_media_register, directives);
				}
			}

			case HandleType.MEMORY:
			{
				if (!Assembler.IsArm64 || !result.IsDataSectionHandle)
				{
					return null;
				}

				var handle = result.Value.To<DataSectionHandle>();

				// Example:
				// S0
				// =>
				// adrp x0, :got:S0
				// ldr x0, [x0, :got_lo12:S0]
				var intermediate = new GetRelativeAddressInstruction(unit, handle).Execute();
				var offset = new Result(new Lower12Bits(handle, true), Assembler.Format);
				var address = Memory.MoveToRegister(unit, new Result(new ComplexMemoryHandle(intermediate, offset, 1), Assembler.Format), Assembler.Size, false);

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