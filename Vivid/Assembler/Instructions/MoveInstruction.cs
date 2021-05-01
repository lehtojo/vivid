using System;
using System.Collections.Generic;
using System.Linq;

public enum MoveType
{
	/// <summary>
	/// The source value is loaded to the destination attaching the source value to the destination and leaving the source untouched
	/// </summary>
	COPY,
	/// <summary>
	/// The source value is loaded to destination attaching the destination value to the destination
	/// </summary>
	LOAD,
	/// <summary>
	/// The source value is loaded to the destination attaching the source value to the destination and updating the source to be equal to the destination
	/// </summary>
	RELOCATE
}

/// <summary>
/// Moves the second operand to the location of the first operand
/// This instruction also acts as a conversion instruction
/// This instruction works on all architectures
/// </summary>
public class MoveInstruction : DualParameterInstruction
{
	private static readonly Dictionary<ComparisonOperator, string[]> Conditionals = new();

	public static void Initialize()
	{
		if (Assembler.IsArm64)
		{
			Conditionals.Add(Operators.GREATER_THAN, new[] { "gt" });
			Conditionals.Add(Operators.GREATER_OR_EQUAL, new[] { "ge" });
			Conditionals.Add(Operators.LESS_THAN, new[] { "lt" });
			Conditionals.Add(Operators.LESS_OR_EQUAL, new[] { "le" });
			Conditionals.Add(Operators.EQUALS, new[] { "eq" });
			Conditionals.Add(Operators.NOT_EQUALS, new[] { "ne" });
			return;
		}

		Conditionals.Add(Operators.GREATER_THAN, new[] { Instructions.X64.CONDITIONAL_MOVE_GREATER_THAN, Instructions.X64.CONDITIONAL_MOVE_ABOVE, Instructions.X64.CONDITIONAL_SET_GREATER_THAN, Instructions.X64.CONDITIONAL_SET_ABOVE });
		Conditionals.Add(Operators.GREATER_OR_EQUAL, new[] { Instructions.X64.CONDITIONAL_MOVE_GREATER_THAN_OR_EQUALS, Instructions.X64.CONDITIONAL_MOVE_ABOVE_OR_EQUALS, Instructions.X64.CONDITIONAL_SET_GREATER_THAN_OR_EQUALS, Instructions.X64.CONDITIONAL_SET_ABOVE_OR_EQUALS });
		Conditionals.Add(Operators.LESS_THAN, new[] { Instructions.X64.CONDITIONAL_MOVE_LESS_THAN, Instructions.X64.CONDITIONAL_MOVE_BELOW, Instructions.X64.CONDITIONAL_SET_LESS_THAN, Instructions.X64.CONDITIONAL_SET_BELOW });
		Conditionals.Add(Operators.LESS_OR_EQUAL, new[] { Instructions.X64.CONDITIONAL_MOVE_LESS_THAN_OR_EQUALS, Instructions.X64.CONDITIONAL_MOVE_BELOW_OR_EQUALS, Instructions.X64.CONDITIONAL_SET_LESS_THAN_OR_EQUALS, Instructions.X64.CONDITIONAL_SET_BELOW_OR_EQUALS });
		Conditionals.Add(Operators.EQUALS, new[] { Instructions.X64.CONDITIONAL_MOVE_EQUALS, Instructions.X64.CONDITIONAL_MOVE_ZERO, Instructions.X64.CONDITIONAL_SET_EQUALS, Instructions.X64.CONDITIONAL_SET_ZERO });
		Conditionals.Add(Operators.NOT_EQUALS, new[] { Instructions.X64.CONDITIONAL_MOVE_NOT_EQUALS, Instructions.X64.CONDITIONAL_MOVE_NOT_ZERO, Instructions.X64.CONDITIONAL_SET_NOT_EQUALS, Instructions.X64.CONDITIONAL_SET_NOT_ZERO });
	}

	private MoveType _Type = MoveType.COPY;
	public new MoveType Type
	{
		get => _Type;

		set
		{
			_Type = value;
			UpdateResultFormat();
		}
	}

	public bool IsSafe { get; set; } = false;
	public bool IsRedundant => First.Value.Equals(Second.Value) && (First.Format.IsDecimal() || Second.Format.IsDecimal() ? First.Format == Second.Format : First.Size == Second.Size);

	public Condition? Condition { get; private set; }

	public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format, InstructionType.MOVE)
	{
		IsUsageAnalyzed = false;
	}

	public MoveInstruction(Unit unit, Result first, Result second, Condition? condition) : base(unit, first, second, Assembler.Format, InstructionType.MOVE)
	{
		Condition = condition;
		IsUsageAnalyzed = false;

		if (Condition != null)
		{
			Dependencies = new[] { Result, First, Second, Condition.Left, Condition.Right };
		}
	}

	private void UpdateResultFormat()
	{
		Result.Format = First.Format;
	}

	private bool IsDecimalConversionNeeded()
	{
		return First.Format != Second.Format;
	}

	private void OnBuildDecimalConversion(int flags_first, int flags_second)
	{
		var is_destination_media_register = First.IsMediaRegister;
		var is_destination_register = First.IsStandardRegister;
		var is_destination_memory_address = First.IsMemoryAddress;
		var is_source_constant = Second.IsConstant;

		string? instruction;

		if (is_destination_media_register)
		{
			// Destination: integer
			// Source: decimal
			//
			// Examples:
			// 
			// 10 => xmm0
			// movsd xmm0, qword [C0]
			//
			// C0 dq 10.0
			//
			// rax => xmm1
			// cvtsi2sd xmm1, rax
			//
			// [rsp+16] => xmm2
			// cvtsi2sd xmm1, qword [rsp+16]

			if (is_source_constant)
			{
				// Example:
				// 0 => xmm0
				// pxor xmm0, xmm0
				if (Numbers.IsZero(Second.Value.To<ConstantHandle>().Value))
				{
					if (Assembler.IsArm64)
					{
						// Examples:
						// 0 => d0
						// fmov d0, xzr

						Build(
							Instructions.Arm64.DECIMAL_MOVE,
							new InstructionParameter(
								First,
								flags_first,
								HandleType.MEDIA_REGISTER
							),
							new InstructionParameter(
								new Result(new RegisterHandle(Unit.GetZeroRegister()), Assembler.Format),
								ParameterFlag.NONE,
								HandleType.MEDIA_REGISTER
							),
							new InstructionParameter(
								Second,
								flags_second | ParameterFlag.HIDDEN | ParameterFlag.BIT_LIMIT_64,
								HandleType.CONSTANT
							)
						);

						return;
					}

					Build(
						Instructions.X64.MEDIA_REGISTER_BITWISE_XOR,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEDIA_REGISTER
						),
						new InstructionParameter(
							First,
							ParameterFlag.NONE,
							HandleType.MEDIA_REGISTER
						),
						new InstructionParameter(
							Second,
							flags_second | ParameterFlag.HIDDEN | ParameterFlag.BIT_LIMIT_64,
							HandleType.CONSTANT
						)
					);

					return;
				}

				if (Assembler.IsArm64)
				{
					// Ensure the source value is in decimal format
					Second.Value.To<ConstantHandle>().Convert(Format.DECIMAL);
					Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
					Second.Format = Format.DECIMAL;

					// Example:
					// adrp x0, C0 (C0: 10.0)
					// ldr d0, [x0, :lo12:C0]

					Build(
						Instructions.Arm64.LOAD,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEDIA_REGISTER
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.MEMORY
						)
					);

					return;
				}

				BuildDecimalConstantMoveX64(flags_first, flags_second);
			}
			else if (Assembler.IsArm64)
			{
				// Example:
				// scvtf d0, x0

				instruction = Instructions.Arm64.CONVERT_INTEGER_TO_DECIMAL;

				Build(
					instruction,
					Assembler.Size,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.REGISTER
					)
				);
			}
			else
			{
				// Examples:
				// cvtsi2ss xmm0, rax
				// cvtsi2sd xmm1, qword ptr [rbx]

				instruction = Assembler.Is32Bit ? Instructions.X64.CONVERT_INTEGER_TO_SINGLE_PRECISION : Instructions.X64.CONVERT_INTEGER_TO_DOUBLE_PRECISION;

				Build(
					instruction,
					Assembler.Size,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.REGISTER,
						HandleType.MEMORY
					)
				);
			}
		}
		else if (is_destination_register)
		{
			// Destination: integer
			// Source: decimal
			//
			// Examples:
			// 
			// 3.14159 => rax
			// mov rax, 3
			//
			// xmm0 => rax
			// cvtsd2si rax, xmm0
			//
			// [rsp+16] => rax
			// cvtsd2si rax, qword [rsp+16]

			if (is_source_constant)
			{
				// Example (x64):
				// 3.14159 => rax
				// mov rax, 3
				//
				// Example (Arm64):
				// 3.14159 => x0
				// mov x0, #3

				// Ensure the source value is in integer format
				Second.Value.To<ConstantHandle>().Convert(First.Format);
				Second.Format = First.Format;

				instruction = Instructions.Shared.MOVE;

				Build(
					instruction,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.CONSTANT
					)
				);
			}
			else
			{
				if (Assembler.IsArm64)
				{
					// Example:
					// fcvtzs x0, d0

					instruction = Instructions.Arm64.CONVERT_DECIMAL_TO_INTEGER;

					Build(
						instruction,
						Assembler.Size,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.REGISTER
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.MEDIA_REGISTER
						)
					);

					return;
				}

				instruction = Assembler.Is32Bit ? Instructions.X64.CONVERT_SINGLE_PRECISION_TO_INTEGER : Instructions.X64.CONVERT_DOUBLE_PRECISION_TO_INTEGER;

				Build(
					instruction,
					Assembler.Size,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEDIA_REGISTER,
						HandleType.MEMORY
					)
				);
			}
		}
		else if (is_destination_memory_address)
		{
			if (!First.Format.IsDecimal())
			{
				// Destination: integer
				// Source: decimal
				//
				// Examples:
				//
				// xmm0 => [rsp+8]:
				// cvtsd2si rax, xmm0
				// mov qword [rsp+8], rax
				//
				// [rsp+32] (decimal) => [rsp+64] (integer)
				// cvtsd2si rax, qword [rsp+32]
				// mov qword [rsp+64], rax
				//
				// 3.141 => [rsp+16]:
				// mov qword [rsp+16], 3

				if (is_source_constant)
				{
					// Convert the decimal value to integer format
					Second.Value.To<ConstantHandle>().Convert(First.Format);
					Second.Format = Assembler.Format;

					if (Assembler.IsArm64)
					{
						// Example:
						// 3.141 => [sp, #16] (integer)
						// mov x0, #3
						// str x0, [sp, #16]

						Build(
							Instructions.Arm64.STORE,
							new InstructionParameter(
								Second,
								flags_second,
								HandleType.REGISTER
							),
							new InstructionParameter(
								First,
								flags_first,
								HandleType.MEMORY
							)
						);

						return;
					}

					// Example:
					// 3.141 => [rsp+16]
					// mov qword ptr [rsp+16], 3

					Build(
						Instructions.Shared.MOVE,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.CONSTANT
						)
					);
				}
				else
				{
					if (Assembler.IsArm64)
					{
						// Example:
						// d0 (decimal) => [sp, #16] (integer)
						// Register d0 is converted to integer by requiring standard register x0
						// str x0, [sp, #16]

						Build(
							Instructions.Arm64.STORE,
							new InstructionParameter(
								Second,
								flags_second,
								HandleType.REGISTER
							),
							new InstructionParameter(
								First,
								flags_first,
								HandleType.MEMORY
							)
						);

						return;
					}

					// Example:
					// xmm0 (decimal) => [rsp+16] (integer)
					// Register xmm0 is converted to integer by requiring standard register rax
					// mov qword ptr [rsp+16], rax

					Build(
						Instructions.Shared.MOVE,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.REGISTER
						)
					);
				}
			}
			else
			{
				// Destination: decimal
				// Source: integer
				//
				// Examples:
				//
				// rax => [rsp+8]:
				// ctvsi2sd xmm0, rax
				// movsd qword [rsp+8], xmm0
				//
				// 10 => [rsp+16]:
				// mov rax, 10.0
				// mov qword [rsp+16], rax

				if (is_source_constant)
				{
					// Convert the integer value to the decimal format
					Second.Value.To<ConstantHandle>().Value = BitConverter.DoubleToInt64Bits((long)Second.Value.To<ConstantHandle>().Value);

					if (Assembler.IsArm64)
					{
						// Example:
						// 10.0 => [x0]
						// mov x1, #10.0
						// str x1, [x0]

						Build(
							Instructions.Arm64.STORE,
							new InstructionParameter(
								Second,
								flags_second,
								HandleType.REGISTER
							),
							new InstructionParameter(
								First,
								flags_first,
								HandleType.MEMORY
							)
						);

						return;
					}

					// Example:
					// 10.0 => [rax]
					// mov rcx, 10.0
					// mov [rax], rcx

					Build(
						Instructions.Shared.MOVE,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.REGISTER
						)
					);
				}
				else
				{
					if (Assembler.IsArm64)
					{
						// Example:
						// x0 (integer) => [sp, #8] (decimal)
						// Register x0 is converted to decimal by requiring decimal register d0
						// str d0, [sp, #8]

						Build(
							Instructions.Arm64.STORE,
							new InstructionParameter(
								Second,
								flags_second,
								HandleType.MEDIA_REGISTER
							),
							new InstructionParameter(
								First,
								flags_first,
								HandleType.MEMORY
							)
						);

						return;
					}

					instruction = Assembler.Is32Bit ? Instructions.X64.SINGLE_PRECISION_MOVE : Instructions.X64.DOUBLE_PRECISION_MOVE;

					Build(
						instruction,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.MEDIA_REGISTER
						)
					);
				}
			}
		}
	}

	private void OnBuildDecimalMoves(int flags_first, int flags_second)
	{
		if (IsDecimalConversionNeeded())
		{
			OnBuildDecimalConversion(flags_first, flags_second);
			return;
		}
		else if ((First.IsMediaRegister || First.IsEmpty) && Second.IsConstant && Numbers.IsZero(Second.Value.To<ConstantHandle>().Value))
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// 0 => d0
				// fmov d0, xzr

				Build(
					Instructions.Arm64.DECIMAL_MOVE,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						new Result(new RegisterHandle(Unit.GetZeroRegister()), Assembler.Format),
						ParameterFlag.NONE,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second | ParameterFlag.HIDDEN | ParameterFlag.BIT_LIMIT_64,
						HandleType.CONSTANT
					)
				);

				return;
			}

			// Examples:
			// 0 => xmm0
			// pxor xmm0, xmm0

			Build(
				Instructions.X64.MEDIA_REGISTER_BITWISE_XOR,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.HIDDEN | ParameterFlag.BIT_LIMIT_64,
					HandleType.CONSTANT
				)
			);

			return;
		}

		var instruction = Assembler.Is32Bit ? Instructions.X64.SINGLE_PRECISION_MOVE : Instructions.X64.DOUBLE_PRECISION_MOVE;

		if (Second.IsConstant)
		{
			if (Assembler.IsX64)
			{
				BuildDecimalConstantMoveX64(flags_first, flags_second);
				return;
			}

			// Move the source value to the data section so that it can be loaded to a media register
			Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
		}

		if (First.IsMemoryAddress)
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// ldr d0, [x0, #-8]

				instruction = Instructions.Arm64.STORE;

				Build(
					instruction,
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEMORY
					)
				);

				return;
			}

			// Examples:
			// movsd qword ptr [rax-8], xmm0

			Build(
				instruction,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.MEDIA_REGISTER
				)
			);

			return;
		}

		var types = new[] { HandleType.CONSTANT, HandleType.MEDIA_REGISTER, HandleType.MEMORY };

		if (Assembler.IsArm64)
		{
			// Examples:
			// fmov d0, d1
			//
			// adrp x0, C0
			// ldr d0, [x0, :lo12:C0]

			if (Second.IsMemoryAddress)
			{
				instruction = Instructions.Arm64.LOAD;
				types = new[] { HandleType.MEMORY };
			}
			else
			{
				instruction = Instructions.Arm64.DECIMAL_MOVE;
				types = new[] { HandleType.MEDIA_REGISTER };
			}
		}

		// Examples:
		// movss xmm0, xmm1
		// movsd xmm0, qword ptr [rax]

		Build(
			instruction,
			new InstructionParameter(
				First,
				flags_first,
				HandleType.MEDIA_REGISTER
			),
			new InstructionParameter(
				Second,
				flags_second,
				types
			)
		);
	}

	/// <summary>
	/// Moves the second operand, which must be a constant, to the destination whose format must be decimal
	/// </summary>
	private void BuildDecimalConstantMoveX64(int flags_first, int flags_second)
	{
		if (Type == MoveType.RELOCATE)
		{
			var handle = Second.Value.To<ConstantHandle>();

			if (Second.Format.IsDecimal())
			{
				handle.Value = BitConverter.DoubleToInt64Bits((double)handle.Value);
				handle.Format = Assembler.Format;
			}
			else
			{
				handle.Value = BitConverter.DoubleToInt64Bits((double)(long)handle.Value);
				handle.Format = Assembler.Format;
			}

			Second.Format = Assembler.Format;

			// Example:
			// xmm0 <- 10.0
			// mov rax, 0x4024000000000000
			// movq xmm0, rax

			Build(
				First.IsMemoryAddress ? Instructions.Shared.MOVE : Instructions.X64.RAW_MEDIA_REGISTER_MOVE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.BIT_LIMIT_64,
					HandleType.REGISTER
				)
			);
		}
		else
		{
			var handle = Second.Value.Finalize().To<ConstantHandle>();

			if (Second.Format.IsDecimal())
			{
				handle.Value = BitConverter.DoubleToInt64Bits((double)handle.Value);
				handle.Format = Assembler.Format;
			}
			else
			{
				handle.Value = BitConverter.DoubleToInt64Bits((double)(long)handle.Value);
				handle.Format = Assembler.Format;
			}

			// Example:
			// xmm0 <- 10.0
			// mov rax, 0x4024000000000000
			// movq xmm0, rax

			Build(
				First.IsMemoryAddress ? Instructions.Shared.MOVE : Instructions.X64.RAW_MEDIA_REGISTER_MOVE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					new Result(handle, Assembler.Format),
					flags_second | ParameterFlag.BIT_LIMIT_64,
					HandleType.REGISTER
				)
			);
		}
	}

	private void LoadConstantArm64(long value, int flags_first, int flags_second)
	{
		var sections = new ushort[4];

		for (var i = 0; i < sections.Length; i++)
		{
			sections[i] = (ushort)(value & 0xFFFF);
			value >>= 16;
		}

		var section = sections[0];

		Build(
			Instructions.Shared.MOVE,
			new InstructionParameter(
				First,
				flags_first,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new ConstantHandle((long)section), Assembler.Format),
				flags_second,
				HandleType.CONSTANT,
				HandleType.REGISTER
			)
		);

		for (var i = 1; i < sections.Length; i++)
		{
			section = sections[i];

			if (section == 0)
			{
				continue;
			}

			Unit.Append(new LoadShiftedConstantInstruction(Unit, First, section, i * 16), true);
		}
	}

	private void BuildConditionalMoveArm64(int flags_first)
	{
		Memory.MoveToRegister(Unit, First, Assembler.Size, false, Trace.GetDirectives(Unit, First));
		Memory.MoveToRegister(Unit, Second, Assembler.Size, false, Trace.GetDirectives(Unit, First));

		Arithmetic.BuildCondition(Unit, Condition!);

		var condition = Conditionals[Condition!.Operator].First();

		Build(
			Instructions.Arm64.CONDITIONAL_MOVE,
			Assembler.Size,
			new InstructionParameter(
				First,
				flags_first | ParameterFlag.NO_ATTACH | ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				First,
				ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new ModifierHandle(condition), Assembler.Format),
				ParameterFlag.NONE,
				HandleType.MODIFIER
			)
		);
	}

	private void BuildConditionalMoveX64(int flags_first)
	{
		Memory.MoveToRegister(Unit, First, Assembler.Size, false, Trace.GetDirectives(Unit, First));
		Memory.MoveToRegister(Unit, Second, Assembler.Size, false, Trace.GetDirectives(Unit, First));

		Arithmetic.BuildCondition(Unit, Condition!);

		var options = Conditionals[Condition!.Operator];
		var instruction = options[Condition.IsDecimal ? 1 : 0];

		Build(
			instruction,
			Assembler.Size,
			new InstructionParameter(
				First,
				flags_first | ParameterFlag.NO_ATTACH | ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER
			)
		);
	}

	public override void OnBuild()
	{
		UpdateResultFormat();

		// Move should not happen if the source is the same as the destination
		if (IsRedundant)
		{
			return;
		}

		// Ensure the destination is available if it is a register
		if (IsSafe && First.IsAnyRegister)
		{
			Memory.ClearRegister(Unit, First.Value.To<RegisterHandle>().Register);
		}

		var flags_first = ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS);
		var flags_second = ParameterFlag.NONE;

		switch (Type)
		{
			case MoveType.COPY:
			{
				// The result value must be attached to the destination
				break;
			}

			case MoveType.LOAD:
			{
				// Destination value must be attached to the destination
				flags_first |= ParameterFlag.ATTACH_TO_DESTINATION;
				break;
			}

			case MoveType.RELOCATE:
			{
				// Source value must be attached and relocated to destination
				flags_second |= ParameterFlag.ATTACH_TO_DESTINATION | ParameterFlag.RELOCATE_TO_DESTINATION;
				break;
			}
		}

		if (First.Format.IsDecimal() && Condition != null)
		{
			throw new InvalidOperationException("Conditional media register moves are not supported");
		}

		if (Condition != null)
		{
			if (Assembler.IsArm64)
			{
				BuildConditionalMoveArm64(flags_first);
				return;
			}

			BuildConditionalMoveX64(flags_first);
			return;
		}

		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			OnBuildDecimalMoves(flags_first, flags_second);
			return;
		}

		if (First.IsStandardRegister && Second.IsConstant && Second.Value.To<ConstantHandle>().Value.Equals(0L))
		{
			if (Assembler.IsArm64)
			{
				Second.Value = new RegisterHandle(Unit.GetZeroRegister());

				// Example: mov x0, xzr
				Build(
					Instructions.Shared.MOVE,
					Assembler.Size,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.REGISTER
					)
				);

				return;
			}

			// Example: xor rax, rax
			Build(
				Instructions.X64.XOR,
				Assembler.Size,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.HIDDEN,
					HandleType.CONSTANT
				)
			);
		}
		else if (First.IsMemoryAddress)
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// str x0, [x1]
				// str x0, [sp, #8]
				Build(
					Instructions.Arm64.STORE,
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.REGISTER
					),
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEMORY
					)
				);

				return;
			}

			// Examples:
			// mov [rsp+8], 314159
			// mov [rdi], -1
			Build(
				Instructions.Shared.MOVE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);
		}
		else if (Assembler.IsX64 && Second.IsDataSectionHandle && Second.Value.To<DataSectionHandle>().Address)
		{
			var handle = Second.Value.To<DataSectionHandle>();
			var address = handle.Address;

			handle.Address = false;

			// Example:
			// mov rax, function_f_S0 => lea rax, [rip+function_f_S0]

			Build(
				Instructions.X64.EVALUATE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.MEMORY
				)
			);

			handle.Address = address;
		}
		else if (Assembler.IsArm64 && Second.IsDataSectionHandle)
		{
			var handle = Second.Value.To<DataSectionHandle>();

			if (!handle.Address)
			{
				// Example:
				// [S0] => x0
				//
				// adrp x0, :got:S0
				// ldr x0, [x0, :got_lo12:S0]
				// ldr x0, [x0]

				var intermediate = new GetRelativeAddressInstruction(Unit, handle).Execute();
				var source = new ComplexMemoryHandle(intermediate, new Result(new Lower12Bits(handle, true), Assembler.Format), 1);
				var address = Memory.MoveToRegister(Unit, new Result(source, Assembler.Format), Assembler.Size, false);

				// Since the address is evaluated the second must use it
				Second.Value = new MemoryHandle(Unit, address, (int)handle.Offset);

				Build(
					Instructions.Arm64.LOAD,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEMORY
					)
				);

				return;
			}
			else
			{
				// Example:
				// S0 => x0
				//
				// adrp x0, :got:S0
				// ldr x0, [x0, :got_lo12:S0]

				var intermediate = new GetRelativeAddressInstruction(Unit, handle).Execute();
				Second.Value = new ComplexMemoryHandle(intermediate, new Result(new Lower12Bits(handle, true), Assembler.Format), 1);

				Build(
					Instructions.Arm64.LOAD,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEMORY
					)
				);

				// Example:
				// S0 + 8 => x0
				//
				// adrp x0, :got:S0
				// ldr x0, [x0, :got_lo12:S0]
				// add x0, x0, #8
				if (handle.Offset != 0)
				{
					// Add the handle offset
					Unit.Append(new AdditionInstruction(Unit, First, new Result(new ConstantHandle(handle.Offset), Assembler.Format), Assembler.Format, true), true);
				}
			}
		}
		else if (Second.IsExpression)
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// add x0, sp, #16
				// add x0, x0, x1
				// add x0, x0, #-1
				// add x0, x0, x1, lsl #1

				Build(
					Instructions.Shared.ADD,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.EXPRESSION
					)
				);

				return;
			}

			// Examples:
			// lea rcx, [rbx+16]
			// lea rax, [rcx*4+rbx-1]
			Build(
				Instructions.X64.EVALUATE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.EXPRESSION
				)
			);
		}
		else if (Second.IsMemoryAddress)
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// ldr x0, [x20, #16]
				Build(
					Instructions.Arm64.LOAD,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second | ParameterFlag.BIT_LIMIT_64,
						HandleType.CONSTANT,
						HandleType.REGISTER,
						HandleType.MEMORY
					)
				);

				return;
			}

			// Examples:
			// mov rax, [rsp+8]
			Build(
				Instructions.Shared.MOVE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.BIT_LIMIT_64,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}
		else
		{
			if (Assembler.IsArm64 && Second.IsConstant && Second.Value.To<ConstantHandle>().Bits > 16 && Second.Value.To<ConstantHandle>().Value is long constant)
			{
				LoadConstantArm64(constant, flags_first, flags_second);
				return;
			}

			// Examples (x64):
			// mov rax, 7
			// mov rbx, rax
			//
			// Examples (Arm64):
			// mov x0, #7
			// mov x20, x0
			//
			Build(
				Instructions.Shared.MOVE,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.BIT_LIMIT_64,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);
		}
	}

	private class Variant
	{
		public Format[] InputDestinationFormats;
		public Format[] InputSourceFormats;
		public HandleType InputDestinationTypes;
		public HandleType InputSourceTypes;
		public string Operation;
		public Size OutputDestinationSize;
		public Size OutputSourceSize;

		public Variant(Format input_destination_format, Format input_source_format, HandleType input_destination_types, HandleType input_source_types, string operation, Size output_destination_size, Size output_source_size)
		{
			InputDestinationFormats = new[] { input_destination_format };
			InputSourceFormats = new[] { input_source_format };
			InputDestinationTypes = input_destination_types;
			InputSourceTypes = input_source_types;
			Operation = operation;
			OutputDestinationSize = output_destination_size;
			OutputSourceSize = output_source_size;
		}

		public Variant(Size input_destination_size, Size input_source_size, HandleType input_destination_types, HandleType input_source_types, string operation, Size output_destination_size, Size output_source_size)
		{
			InputDestinationFormats = new[] { input_destination_size.ToFormat(), input_destination_size.ToFormat(false) };
			InputSourceFormats = new[] { input_source_size.ToFormat(), input_source_size.ToFormat(false) };
			InputDestinationTypes = input_destination_types;
			InputSourceTypes = input_source_types;
			Operation = operation;
			OutputDestinationSize = output_destination_size;
			OutputSourceSize = output_source_size;
		}
	}

	private static readonly Variant[] Variants = new Variant[]
	{
		new(Size.XMMWORD, Size.XMMWORD, HandleType.MEMORY | HandleType.MEDIA_REGISTER, HandleType.MEMORY | HandleType.MEDIA_REGISTER, Instructions.X64.UNALIGNED_XMMWORD_MOVE, Size.XMMWORD, Size.XMMWORD),
		new(Size.YMMWORD, Size.YMMWORD, HandleType.MEMORY | HandleType.MEDIA_REGISTER, HandleType.MEMORY | HandleType.MEDIA_REGISTER, Instructions.X64.UNALIGNED_YMMWORD_MOVE, Size.YMMWORD, Size.YMMWORD),
	};

	private Variant? TryGetVariant()
	{
		return Variants.Where(i =>
			i.InputDestinationFormats.Contains(Destination!.Value!.Format) &&
			i.InputSourceFormats.Contains(Source!.Value!.Format) &&
			Flag.Has((int)i.InputDestinationTypes, (int)Destination!.Value!.Type) &&
			Flag.Has((int)i.InputSourceTypes, (int)Source!.Value!.Type)

		).FirstOrDefault();
	}

	private bool IsMoveInstructionX64()
	{
		return Operation == Instructions.Shared.MOVE || Operation == Instructions.X64.UNSIGNED_CONVERSION_MOVE || Operation == Instructions.X64.SIGNED_CONVERSION_MOVE || Operation == Instructions.X64.SIGNED_DWORD_CONVERSION_MOVE;
	}

	private bool IsStoreInstructionArm64()
	{
		return Operation == Instructions.Arm64.STORE || Operation == Instructions.Arm64.STORE_UINT16 || Operation == Instructions.Arm64.STORE_UINT8;
	}

	private bool IsLoadInstructionArm64()
	{
		return Operation == Instructions.Arm64.LOAD || Operation == Instructions.Arm64.LOAD_INT8 || Operation == Instructions.Arm64.LOAD_INT16 || Operation == Instructions.Arm64.LOAD_INT32 || Operation == Instructions.Arm64.LOAD_UINT8 || Operation == Instructions.Arm64.LOAD_UINT16;
	}

	public void OnPostBuildX64()
	{
		var variant = TryGetVariant();

		if (variant != null)
		{
			Operation = variant.Operation;
			Destination!.Value!.Format = variant.OutputDestinationSize.ToFormat();
			Source!.Value!.Format = variant.OutputSourceSize.ToFormat();
			return;
		}

		// Skip decimal formats since they are correct by default
		if (Destination!.Value!.Format.IsDecimal() || Source!.Value!.Format.IsDecimal())
		{
			return;
		}

		if (!IsMoveInstructionX64())
		{
			return;
		}

		var is_source_memory_address = Source!.Value?.Type == HandleType.MEMORY;
		var is_destination_memory_address = Destination!.Value?.Type == HandleType.MEMORY;

		if (is_destination_memory_address && is_source_memory_address)
		{
			throw new ApplicationException("Both destination and source operands were memory handles at the same time in move instruction");
		}

		if (is_destination_memory_address && !is_source_memory_address)
		{
			Source.Value!.Format = Destination.Value!.Format;
			return;
		}

		// Return if no conversion is needed
		if (Source!.Value!.Size.Bytes == Destination!.Value!.Size.Bytes || Source.Value.Is(HandleType.CONSTANT))
		{
			Operation = Instructions.Shared.MOVE;
			return;
		}

		// NOTE: Now the destination parameter must be a register

		if (Source.Value.Size.Bytes > Destination.Value.Size.Bytes)
		{
			Source.Value.Format = Destination.Value.Format;
			return;
		}

		// NOTE: Now the size of source operand must be less than the size of destination operand

		if (Source.Value.IsUnsigned)
		{
			if (Destination.Value.Size == 64 && Source.Value.Size == 32)
			{
				// Example: mov eax, ecx (64 <- 32)
				// In 64-bit mode if you move data from 32-bit register to another 32-bit register it zeroes out the high half of the destination 64-bit register
				Destination.Value.Format = Format.UINT32;
				return;
			}

			// Examples:
			// movzx rax, cx (64 <- 16)
			// movzx rax, cl (64 <- 8)
			// 
			// movzx eax, cx (32 <- 16)
			// movzx eax, cl (32 <- 8)
			//
			// movzx ax, cl (16 <- 8)

			Operation = Instructions.X64.UNSIGNED_CONVERSION_MOVE;
			return;
		}

		if (Destination.Value.Size == 64 && Source.Value.Size == 32)
		{
			// Example: movsxd rax, ecx (64 <- 32)
			Operation = Instructions.X64.SIGNED_DWORD_CONVERSION_MOVE;
			return;
		}

		// Examples:
		// movsx rax, cx (64 <- 16)
		// movsx rax, cl (64 <- 8)
		//
		// movsx eax, cx (32 <- 16)
		// movsx eax, cl (32 <- 8)
		//
		// movsx ax, cl (16 <- 8)

		Operation = Instructions.X64.SIGNED_CONVERSION_MOVE;
	}

	public void OnPostBuildArm64()
	{
		var source = Source!.Value!;
		var destination = Destination!.Value!;

		// Skip decimal formats
		if (source.Format.IsDecimal() || destination.Format.IsDecimal())
		{
			return;
		}

		var is_load = IsLoadInstructionArm64();
		var is_store = IsStoreInstructionArm64();

		// NOTE: When a value is moved to 32-bit register, the higher bits are zeroed out
		if (is_load || is_store)
		{
			var inspected = is_load ? source : destination;
			var value = is_load ? destination : source;

			if (inspected.IsUnsigned)
			{
				// Examples:
				// 
				// ldrb w0, [x1] ([16, 32, 64] <- 8)
				// ldrh w0, [x1] ([8, 32, 64] <- 16)
				// ldr w0, [x1] ([8, 16, 64] <- 32)
				// ldr x0, [x1] ([8, 16, 32] <- 64)
				// 
				// strb w0, [x1] ([8, 16, 32, 64] -> 8)
				// strh w0, [x1] ([8, 16, 32, 64] -> 16)
				// str w0, [x1] ([8, 16, 32, 64] -> 32)
				// str x0, [x1] ([8, 16, 32, 64] -> 64)

				Operation = inspected.Size.Bits switch
				{
					8 => is_load ? Instructions.Arm64.LOAD_UINT8 : Instructions.Arm64.STORE_UINT8,
					16 => is_load ? Instructions.Arm64.LOAD_UINT16 : Instructions.Arm64.STORE_UINT16,
					32 => is_load ? Instructions.Arm64.LOAD : Instructions.Arm64.STORE,
					64 => is_load ? Instructions.Arm64.LOAD : Instructions.Arm64.STORE,
					_ => throw new ApplicationException("Could not resolve the size of source value")
				};

				if (inspected.Size.Bits == 64)
				{
					value.Format = Size.QWORD.ToFormat();
				}
				else
				{
					value.Format = Size.DWORD.ToFormat();
				}

				return;
			}

			// Examples:
			// 
			// ldrsb w0, [x1] ([8, 16, 32] <- 8)
			// ldrsh w0, [x1] ([8, 16, 32] <- 16)
			// ldr w0, [x1] ([8, 16, 32] <- 32)
			// ldr x0, [x1] ([8, 16, 32] <- 64)
			//
			// ldrsb x0, [x1] (64 <- 8)
			// ldrsh x0, [x1] (64 <- 16)
			// ldrsw x0, [x1] (64 <- 32)
			// ldr x0, [x1] (64 <- 64)
			// 
			// strb w0, [x1] ([8, 16, 32, 64] -> 8)
			// strh w0, [x1] ([8, 16, 32, 64] -> 16)
			// str w0, [x1] ([8, 16, 32, 64] -> 32)
			// str x0, [x1] ([8, 16, 32, 64] -> 64) 

			Operation = inspected.Size.Bits switch
			{
				8 => is_load ? Instructions.Arm64.LOAD_INT8 : Instructions.Arm64.STORE_UINT8,
				16 => is_load ? Instructions.Arm64.LOAD_INT16 : Instructions.Arm64.STORE_UINT16,
				32 => is_load ? (destination.Size.Bits == 64 ? Instructions.Arm64.LOAD_INT32 : Instructions.Arm64.LOAD) : Instructions.Arm64.STORE,
				64 => is_load ? Instructions.Arm64.LOAD : Instructions.Arm64.STORE,
				_ => throw new ApplicationException("Could not resolve the size of source value")
			};

			if (inspected.Size.Bits == 64 || is_load && destination.Size.Bits == 64)
			{
				value.Format = Size.QWORD.ToFormat();
			}
			else
			{
				value.Format = Size.DWORD.ToFormat();
			}

			return;
		}

		// NOTE: Now the source and destination handles must be registers or constants
		if (source.Size == destination.Size || Source.IsConstant)
		{
			return;
		}

		// Examples (signed or unsigned):
		// mov w0, w1 (32 <- 64)
		// mov w0, w1 (16 <- 32)
		// mov w0, w1 (8 <- 16)
		if (destination.Size.Bits < source.Size.Bits)
		{
			destination.Format = Size.DWORD.ToFormat();
			source.Format = Size.DWORD.ToFormat();
			return;
		}

		if (!source.IsUnsigned)
		{
			// Examples (source is signed and destination unknown):
			// sxtb x0, w1 (64 <- 8)
			// sxth x0, w1 (64 <- 16)
			// sxtw x0, w1 (64 <- 32)
			//
			// sxtb w0, w1 ([16, 32] <- 8)
			// sxth w0, w1 (32 <- 16)

			Operation = source.Size.Bits switch
			{
				8 => Instructions.Arm64.CONVERT_INT8_TO_INT64,
				16 => Instructions.Arm64.CONVERT_INT16_TO_INT64,
				32 => Instructions.Arm64.CONVERT_INT32_TO_INT64,
				_ => throw new ApplicationException("Could not resolve the size of source value")
			};

			return;
		}

		// Examples (source is unsigned and destination unknown):
		// mov w0, w1 (64 <- 32)
		//
		// and w0, w1, #0xFFFF ([32, 64] <- 16)
		// and w0, w1, #0xFF ([16, 32, 64] <- 8)

		destination.Format = Size.DWORD.ToFormat();

		if (destination.Size.Bits == 64 && source.Size.Bits == 32)
		{
			return;
		}

		var mask = source.Size.Bits == 16 ? (long)0xFFFF : (long)0xFF;
		var mask_parameter = new InstructionParameter(
			new Result(new ConstantHandle(mask), Assembler.Format),
			ParameterFlag.NONE,
			HandleType.CONSTANT
		);

		mask_parameter.Value = mask_parameter.Result.Value;

		Operation = Instructions.Shared.AND;
		Parameters.Add(mask_parameter);
	}

	public override void OnPostBuild()
	{
		if (Assembler.IsX64)
		{
			OnPostBuildX64();
		}
		else
		{
			OnPostBuildArm64();
		}
	}

	public override bool Redirect(Handle handle)
	{
		if (Assembler.IsX64)
		{
			if (Operation == Instructions.Shared.MOVE && (handle.Is(HandleType.REGISTER) || (handle.Is(HandleType.MEMORY) && !Source!.IsMemoryAddress && (!Source.IsConstant || Source.Value!.To<ConstantHandle>().Bits <= 32))))
			{
				Destination!.Value = handle;
				return true;
			}

			if ((Operation == Instructions.X64.SINGLE_PRECISION_MOVE || Operation == Instructions.X64.DOUBLE_PRECISION_MOVE || Operation == Instructions.X64.RAW_MEDIA_REGISTER_MOVE) && (handle.Is(HandleType.MEDIA_REGISTER) || (handle.Is(HandleType.MEMORY) && !Source!.IsMemoryAddress)))
			{
				Destination!.Value = handle;
				return true;
			}

			return false;
		}

		if (Operation == Instructions.Shared.MOVE && handle.Is(HandleType.REGISTER))
		{
			Destination!.Value = handle;
			return true;
		}

		return false;
	}
}