using System;
using System.Linq;

/// <summary>
/// Multiplicates to specified values together
/// </summary>
public class MultiplicationInstruction : DualParameterInstruction
{
	private const string X64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "imul";
	private const string ARM64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "mul";

	private const int SIGNED_INTEGER_MULTIPLICATION_FIRST = 0;
	private const int SIGNED_INTEGER_MULTIPLICATION_SECOND = 1;

	private const string X64_SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulss";
	private const string X64_DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulsd";

	private const string ARM64_DECIMAL_MULTIPLICATION_INSTRUCTION = "fmul";

	public const string X64_EXTENDED_MULTIPLICATION_INSTRUCTION = "lea";
	public const string X64_MULTIPLY_BY_POWER_OF_TWO_INSTRUCTION = "sal";

	public const string ARM64_MULTIPLY_BY_POWER_OF_TWO_INSTRUCTION = "lsl";

	public bool Assigns { get; private set; }

	public MultiplicationInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format)
	{
		Assigns = assigns;
	}

	private static bool IsPowerOfTwo(long x)
	{
		return (x & (x - 1)) == 0;
	}

	private static bool IsConstantValidForExtendedMultiplication(long x)
	{
		return IsPowerOfTwo(x) && x <= (Assembler.IsX64 ? 8 : 1L << 32);
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
			var instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION : X64_DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION;
			
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
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				)
			);

			return;
		}

		var multiplication = TryGetConstantMultiplication();

		if (multiplication != null && multiplication.Constant > 0)
		{
			if (IsPowerOfTwo(multiplication.Constant))
			{
				var count = new ConstantHandle((long)Math.Log2(multiplication.Constant));

				result = Memory.LoadOperand(Unit, multiplication.Multiplicand, false, Assigns);

				Build(
					X64_MULTIPLY_BY_POWER_OF_TWO_INSTRUCTION,
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
				// Example: imul rax, 3 => lea ..., [rax*2+rax]
				var calculation = new ExpressionHandle
				(
					multiplication.Multiplicand,
					(int)multiplication.Constant - 1,
					multiplication.Multiplicand,
					0
				);

				Build(
					X64_EXTENDED_MULTIPLICATION_INSTRUCTION,
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
		}

		result = Memory.LoadOperand(Unit, First, false, Assigns);

		Build(
			X64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION,
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

		return;
	}

	public void BuildExtendedMultiplicationArm64(Result multiplicand, int shift)
	{
		if (Assigns)
		{
			// If the destination operand is assigned and it's a memory address, load it, calculate and store it lastly
			var result = Memory.LoadOperand(Unit, First, false, Assigns);
		
			// Example:
			// a *= 9
			//
			// x0: a
			//
			// add x0, x0, x0, lsl #3

			Build(
				AdditionInstruction.SHARED_STANDARD_ADDITION_INSTRUCTION,
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
					new Result(new ModifierHandle($"{BitwiseInstruction.ARM64_SHIFT_LEFT_INSTRUCTION} #{shift}"), Assembler.Format),
					ParameterFlag.NONE,
					HandleType.MODIFIER
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, false);

		Build(
			AdditionInstruction.SHARED_STANDARD_ADDITION_INSTRUCTION,
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
				new Result(new ModifierHandle($"{BitwiseInstruction.ARM64_SHIFT_LEFT_INSTRUCTION} #{shift}"), Assembler.Format),
				ParameterFlag.NONE,
				HandleType.MODIFIER
			)
		);

		return;
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? ARM64_DECIMAL_MULTIPLICATION_INSTRUCTION : ARM64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.REGISTER };

		var multiplication = TryGetConstantMultiplication();

		var first = First;
		var second = Second;

		if (!is_decimal && multiplication != null && multiplication.Constant > 0)
		{
			if (IsPowerOfTwo(multiplication.Constant))
			{
				first = multiplication.Multiplicand;
				second = new Result(new ConstantHandle((long)Math.Log2(multiplication.Constant)), Assembler.Format);
				types = is_decimal ? new[] { HandleType.CONSTANT, HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
				instruction = ARM64_MULTIPLY_BY_POWER_OF_TWO_INSTRUCTION;
			}
			else if (IsConstantValidForExtendedMultiplication(multiplication.Constant - 1))
			{
				BuildExtendedMultiplicationArm64(multiplication.Multiplicand, (int)Math.Log2(multiplication.Constant - 1));
				return;
			}
		}

		if (Assigns)
		{
			// If the destination operand is assigned and it's a memory address, load it, calculate and store it lastly
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

		return;
	}

	public override Result GetDestinationDependency()
	{
		if (Result.Format.IsDecimal())
		{
			return First;
		}

		var constant_multiplication = TryGetConstantMultiplication();

		if (constant_multiplication != null && constant_multiplication.Constant > 0 && IsConstantValidForExtendedMultiplication(constant_multiplication.Constant - 1))
		{
			return Result;
		}

		return First;
	}

	public bool RedirectX64(Handle handle)
	{
		if (Operation != X64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION || Assigns)
		{
			return false;
		}

		var first = Parameters[SIGNED_INTEGER_MULTIPLICATION_FIRST];
		var second = Parameters[SIGNED_INTEGER_MULTIPLICATION_SECOND];

		if (handle.Type == HandleType.REGISTER && (first.IsMemoryAddress || first.IsStandardRegister) && second.IsConstant)
		{
			Operation = X64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION;

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
		if (Operation == ARM64_SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == ARM64_DECIMAL_MULTIPLICATION_INSTRUCTION && handle.Is(HandleType.MEDIA_REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		return false;
	}

	public override bool Redirect(Handle handle)
	{
		if (Assembler.IsArm64)
		{
			return RedirectArm64(handle);
		}

		return RedirectX64(handle);
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.MULTIPLICATION;
	}
}