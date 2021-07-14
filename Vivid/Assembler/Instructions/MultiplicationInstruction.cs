using System;
using System.Linq;

/// <summary>
/// Multiplicates to specified values together
/// This instruction works on all architectures
/// </summary>
public class MultiplicationInstruction : DualParameterInstruction
{
	public bool Assigns { get; private set; }

	public MultiplicationInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format, InstructionType.MULTIPLICATION)
	{
		Assigns = assigns;
	}

	private static bool IsConstantValidForExtendedMultiplication(long x)
	{
		return Common.IsPowerOfTwo(x) && x <= (Assembler.IsX64 ? Instructions.X64.EVALUATE_MAX_MULTIPLIER : 1L << 32);
	}

	private class ConstantMultiplication
	{
		public Result Multiplicand;
		public long Constant;

		public ConstantMultiplication(Result other, Result constant)
		{
			Multiplicand = other;
			Constant = (long)constant.Value.To<ConstantHandle>().Value;
		}
	}

	private ConstantMultiplication? TryGetConstantMultiplication()
	{
		if (First.Value.Type == HandleType.CONSTANT && !First.Format.IsDecimal())
		{
			return new ConstantMultiplication(Second, First);
		}

		return Second.Value.Type == HandleType.CONSTANT && !Second.Format.IsDecimal() ? new ConstantMultiplication(First, Second) : null;
	}

	public override void OnBuild()
	{
		if (Assigns && First.IsMemoryAddress)
		{
			Unit.Append(new MoveInstruction(Unit, First, Result), true);
		}

		if (Assembler.IsX64)
		{
			OnBuildX64();
		}
		else
		{
			OnBuildArm64();
		}
	}

	public void OnBuildX64()
	{
		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);
		var result = (Result?)null;

		// Handle decimal multiplication separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.Is32Bit ? Instructions.X64.SINGLE_PRECISION_MULTIPLY : Instructions.X64.DOUBLE_PRECISION_MULTIPLY;
			var types = Second.Format.IsDecimal() ? new[] { HandleType.MEDIA_REGISTER, HandleType.MEMORY } : new[] { HandleType.MEDIA_REGISTER };

			result = Memory.LoadOperand(Unit, First, true, Assigns);

			Build(
				instruction,
				new InstructionParameter(
					result,
					ParameterFlag.READS | flags,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					types
				)
			);

			return;
		}

		var multiplication = TryGetConstantMultiplication();

		if (multiplication != null && multiplication.Constant > 0)
		{
			if (!Assigns && Common.IsPowerOfTwo(multiplication.Constant) && multiplication.Constant <= Instructions.X64.EVALUATE_MAX_MULTIPLIER && !First.IsExpiring(Unit.Position))
			{
				Memory.GetResultRegisterFor(Unit, Result, false);

				result = Memory.LoadOperand(Unit, multiplication.Multiplicand, false, Assigns);

				// Example:
				// mov rax, rcx
				// imul rax, 4
				// =>
				// lea rax, [rcx*4]

				var calculation = new ExpressionHandle
				(
					result,
					(int)multiplication.Constant,
					null,
					0
				);

				Build(
					Instructions.X64.EVALUATE,
					Assembler.Size,
					new InstructionParameter(
						Result,
						ParameterFlag.DESTINATION,
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(calculation, Assembler.Format),
						ParameterFlag.NONE,
						HandleType.EXPRESSION
					)
				);

				return;
			}

			if (Common.IsPowerOfTwo(multiplication.Constant))
			{
				var count = new ConstantHandle((long)Math.Log2(multiplication.Constant));

				result = Memory.LoadOperand(Unit, multiplication.Multiplicand, false, Assigns);

				Build(
					Instructions.X64.SHIFT_LEFT,
					Assembler.Size,
					new InstructionParameter(
						result,
						ParameterFlag.READS | flags,
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(count, Assembler.Format),
						ParameterFlag.NONE,
						HandleType.CONSTANT
					)
				);

				return;
			}

			if (IsConstantValidForExtendedMultiplication(multiplication.Constant - 1))
			{
				result = Memory.LoadOperand(Unit, multiplication.Multiplicand, false, Assigns);

				var destination = (Result?)null;

				if (Assigns)
				{
					destination = result;
				}
				else
				{
					Memory.GetResultRegisterFor(Unit, Result, false);
					destination = Result;
				}

				// Example: imul rax, 3 => lea ..., [rax*2+rax]
				var expression = new ExpressionHandle
				(
					result,
					(int)multiplication.Constant - 1,
					result,
					0
				);

				Build(
					Instructions.X64.EVALUATE,
					Assembler.Size,
					new InstructionParameter(
						destination,
						ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | (Assigns ? ParameterFlag.NO_ATTACH : ParameterFlag.NONE),
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(expression, Assembler.Format),
						ParameterFlag.NONE,
						HandleType.EXPRESSION
					)
				);

				return;
			}
		}

		result = Memory.LoadOperand(Unit, First, false, Assigns);

		Build(
			Instructions.X64.SIGNED_MULTIPLY,
			Assembler.Size,
			new InstructionParameter(
				result,
				ParameterFlag.READS | flags,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.CONSTANT,
				HandleType.REGISTER,
				HandleType.MEMORY
			)
		);
	}

	public void BuildExtendedMultiplicationArm64(Result multiplicand, int shift)
	{
		if (Assigns)
		{
			// If the destination operand is assigned and it is a memory address, load it, calculate and store it lastly
			var result = Memory.LoadOperand(Unit, First, false, Assigns);

			// Example:
			// a *= 9
			//
			// x0: a
			//
			// add x0, x0, x0, lsl #3

			Build(
				Instructions.Shared.ADD,
				Assembler.Size,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					HandleType.REGISTER
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					new Result(new ModifierHandle($"{Instructions.Arm64.SHIFT_LEFT} #{shift}"), Assembler.Format),
					ParameterFlag.NONE,
					HandleType.MODIFIER
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, false);

		Build(
			Instructions.Shared.ADD,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				multiplicand,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				multiplicand,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new ModifierHandle($"{Instructions.Arm64.SHIFT_LEFT} #{shift}"), Assembler.Format),
				ParameterFlag.NONE,
				HandleType.MODIFIER
			)
		);
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? Instructions.Arm64.DECIMAL_MULTIPLY : Instructions.Arm64.SIGNED_MULTIPLY;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.REGISTER };

		var multiplication = TryGetConstantMultiplication();

		var first = First;
		var second = Second;

		if (!is_decimal && multiplication != null && multiplication.Constant > 0)
		{
			if (Common.IsPowerOfTwo(multiplication.Constant))
			{
				first = multiplication.Multiplicand;
				second = new Result(new ConstantHandle((long)Math.Log2(multiplication.Constant)), Assembler.Format);
				types = is_decimal ? new[] { HandleType.CONSTANT, HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
				instruction = Instructions.Arm64.SHIFT_LEFT;
			}
			else if (IsConstantValidForExtendedMultiplication(multiplication.Constant - 1))
			{
				BuildExtendedMultiplicationArm64(multiplication.Multiplicand, (int)Math.Log2(multiplication.Constant - 1));
				return;
			}
		}

		if (Assigns)
		{
			// If the destination operand is assigned and it is a memory address, load it, calculate and store it lastly
			var result = Memory.LoadOperand(Unit, First, is_decimal, Assigns);

			Build(
				instruction,
				Assembler.Size,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					base_register_type
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					base_register_type
				),
				new InstructionParameter(
					second,
					ParameterFlag.NONE,
					types
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, is_decimal);

		Build(
			instruction,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				base_register_type
			),
			new InstructionParameter(
				first,
				ParameterFlag.NONE,
				base_register_type
			),
			new InstructionParameter(
				second,
				ParameterFlag.NONE,
				types
			)
		);
	}

	public bool RedirectX64(Handle handle)
	{
		var first = Parameters[0];
		var second = Parameters[1];

		if (Operation == Instructions.X64.SHIFT_LEFT)
		{
			if (!second.IsConstant) return false;

			// Example:
			// sal rax, 2 => lea rcx, [rax*4]

			var shift = (long)second.Value!.To<ConstantHandle>().Value;

			// Maximum multiplier is eight so the exponent must be three or less
			if (shift > 3) return false;

			var expression = new ExpressionHandle
			(
				new Result(first.Value!, Assembler.Format),
				(int)Math.Pow(2, shift),
				null,
				0
			);

			Operation = Instructions.X64.EVALUATE;

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(expression, ParameterFlag.NONE));

			return true;
		}

		if (Operation == Instructions.X64.EVALUATE)
		{
			if (!handle.Is(HandleType.REGISTER)) return false;

			Destination!.Value = handle;
			return true;
		}

		if (Operation != Instructions.X64.SIGNED_MULTIPLY) return false;

		if (handle.Type == HandleType.REGISTER && (first.IsMemoryAddress || first.IsStandardRegister) && second.IsConstant)
		{
			Operation = Instructions.X64.SIGNED_MULTIPLY;

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(first.Value!, ParameterFlag.NONE));
			Parameters.Add(new InstructionParameter(second.Value!, ParameterFlag.NONE));

			return true;
		}

		return false;
	}

	public bool RedirectArm64(Handle handle)
	{
		if ((Operation == Instructions.Arm64.SIGNED_MULTIPLY || Operation == Instructions.Arm64.SHIFT_LEFT) && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == Instructions.Arm64.DECIMAL_MULTIPLY && handle.Is(HandleType.MEDIA_REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		return false;
	}

	public override bool Redirect(Handle handle, bool root)
	{
		if (Assembler.IsArm64)
		{
			return RedirectArm64(handle);
		}

		return RedirectX64(handle);
	}
}