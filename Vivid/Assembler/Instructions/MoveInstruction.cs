using System;

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
/// This function also acts as a conversion function
/// </summary>
public class MoveInstruction : DualParameterInstruction
{
	public const string SHARED_MOVE_INSTRUCTION = "mov";
	public const string SHARED_BITWISE_AND_INSTRUCTION = "and";

	public const string X64_LOAD_ADDRESS_INSTRUCTION = "lea";
	public const string X64_UNSIGNED_CONVERSION = "movzx";
	public const string X64_SIGNED_CONVERSION = "movsx";
	public const string X64_SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE = "movsxd";

	public const string X64_SINGLE_PRECISION_MOVE = "movss";
	public const string X64_DOUBLE_PRECISION_MOVE = "movsd";

	public const string ARM64_DECIMAL_MOVE_INSTRUCTION = "fmov";

	public const string X64_CONVERT_SINGLE_PRECISION_TO_INTEGER = "cvttss2si";
	public const string X64_CONVERT_DOUBLE_PRECISION_TO_INTEGER = "cvttsd2si";

	public const string ARM64_CONVERT_DECIMAL_TO_INTEGER = "fcvtzs";

	public const string X64_CONVERT_INTEGER_TO_SINGLE_PRECISION = "cvtsi2ss";
	public const string X64_CONVERT_INTEGER_TO_DOUBLE_PRECISION = "cvtsi2sd";

	public const string ARM64_CONVERT_INTEGER_TO_DECIMAL = "scvtf";

	public const string X64_MEDIA_REGISTER_BITWISE_XOR = "pxor";
	public const string X64_BITWISE_XOR_INSTRUCTION = "xor";

	public const string ARM64_STORE_INSTRUCTION = "str";
	public const string ARM64_LOAD_INSTRUCTION = "ldr";

	public const string ARM64_LOAD_UINT8_INSTRUCTION = "ldrb";
	public const string ARM64_LOAD_UINT16_INSTRUCTION = "ldrh";
	public const string ARM64_LOAD_INT8_INSTRUCTION = "ldrsb";
	public const string ARM64_LOAD_INT16_INSTRUCTION = "ldrsh";
	public const string ARM64_LOAD_INT32_INSTRUCTION = "ldrsw";
	
	public const string ARM64_STORE_UINT8_INSTRUCTION = "strb";
	public const string ARM64_STORE_UINT16_INSTRUCTION = "strh";

	public const string ARM64_CONVERT_INT8_TO_INT64 = "sxtb";
	public const string ARM64_CONVERT_INT16_TO_INT64 = "sxth";
	public const string ARM64_CONVERT_INT32_TO_INT64 = "sxtw";

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

	public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format)
	{
		IsUsageAnalyzed = false; // Important: Prevents a future usage cycle (maybe)
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
							ARM64_DECIMAL_MOVE_INSTRUCTION,
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
						X64_MEDIA_REGISTER_BITWISE_XOR,
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

				// Ensure the source value is in decimal format
				Second.Value.To<ConstantHandle>().Convert(Format.DECIMAL);
				Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
				Second.Format = Format.DECIMAL;

				if (Assembler.IsArm64)
				{
					// Example:
					// adrp x0, C0 (C0: 10.0)
					// ldr d0, [x0, :lo12:C0]

					Build(
						ARM64_LOAD_INSTRUCTION,
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

				instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_MOVE : X64_DOUBLE_PRECISION_MOVE;

				// Example:
				// movsd xmm0, qword [C0] (C0: 10.0)

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
						HandleType.MEMORY
					)
				);
			}
			else if (Assembler.IsArm64)
			{
				// Example:
				// scvtf d0, x0

				instruction = ARM64_CONVERT_INTEGER_TO_DECIMAL;

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
						HandleType.REGISTER
					)
				);
			}
			else
			{
				// Examples:
				// cvtsi2ss xmm0, rax
				// cvtsi2sd xmm1, qword ptr [rbx]

				instruction = Assembler.Is32bit ? X64_CONVERT_INTEGER_TO_SINGLE_PRECISION : X64_CONVERT_INTEGER_TO_DOUBLE_PRECISION;

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
				// Example (ARM64):
				// 3.14159 => x0
				// mov x0, #3

				// Ensure the source value is in integer format
				Second.Value.To<ConstantHandle>().Convert(First.Format);
				Second.Format = First.Format;

				instruction = SHARED_MOVE_INSTRUCTION;

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

					instruction = ARM64_CONVERT_DECIMAL_TO_INTEGER;

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
							HandleType.MEDIA_REGISTER
						)
					);

					return;
				}

				instruction = Assembler.Is32bit ? X64_CONVERT_SINGLE_PRECISION_TO_INTEGER : X64_CONVERT_DOUBLE_PRECISION_TO_INTEGER;

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
					// Convert the decimal value to the destination's integer format
					Second.Value.To<ConstantHandle>().Convert(First.Format);
					Second.Format = Assembler.Format;

					if (Assembler.IsArm64)
					{
						// Example:
						// 3.141 => [sp, #16] (integer)
						// mov x0, #3
						// str x0, [sp, #16]

						Build(
							ARM64_STORE_INSTRUCTION,
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
						SHARED_MOVE_INSTRUCTION,
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
							ARM64_STORE_INSTRUCTION,
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
						SHARED_MOVE_INSTRUCTION,
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
					// Convert the integer value to the destination's decimal format
					Second.Value.To<ConstantHandle>().Value = BitConverter.DoubleToInt64Bits((long)Second.Value.To<ConstantHandle>().Value);

					if (Assembler.IsArm64)
					{
						// Example:
						// 10.0 => [x0]
						// mov x1, #10.0
						// str x1, [x0]

						Build(
							ARM64_STORE_INSTRUCTION,
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
						SHARED_MOVE_INSTRUCTION,
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
							ARM64_STORE_INSTRUCTION,
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

					instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_MOVE : X64_DOUBLE_PRECISION_MOVE;

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
		else if (First.IsMediaRegister && Second.IsConstant && Numbers.IsZero(Second.Value.To<ConstantHandle>().Value))
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// 0 => d0
				// fmov d0, xzr

				Build(
					ARM64_DECIMAL_MOVE_INSTRUCTION,
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
				X64_MEDIA_REGISTER_BITWISE_XOR,
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

		var instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_MOVE : X64_DOUBLE_PRECISION_MOVE;

		if (Second.IsConstant)
		{
			// Move the source value to the data section so that it can be loaded to a media register
			Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
		}

		if (First.IsMemoryAddress)
		{
			if (Assembler.IsArm64)
			{
				// Examples:
				// ldr d0, [x0, #-8]

				instruction = ARM64_STORE_INSTRUCTION;

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

		var types = new[] { HandleType.MEDIA_REGISTER, HandleType.MEMORY };
		
		if (Assembler.IsArm64)
		{
			// Examples:
			// fmov d0, d1
			//
			// adrp x0, C0
			// ldr d0, [x0, :lo12:C0]

			if (Second.IsMemoryAddress)
			{
				instruction = ARM64_LOAD_INSTRUCTION;
				types = new[] { HandleType.MEMORY };
			}
			else
			{
				instruction = ARM64_DECIMAL_MOVE_INSTRUCTION;
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
			SHARED_MOVE_INSTRUCTION,
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

	public override void OnBuild()
	{
		UpdateResultFormat();

		// Move shouldn't happen if the source is the same as the destination
		if (IsRedundant)
		{
			return;
		}

		// Ensure the destination is available if it's a register
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
					SHARED_MOVE_INSTRUCTION,
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
				X64_BITWISE_XOR_INSTRUCTION,
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
					ARM64_STORE_INSTRUCTION,
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
				SHARED_MOVE_INSTRUCTION,
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
		else if (Assembler.IsX64 && Second.Value is DataSectionHandle && Second.Value.To<DataSectionHandle>().Address)
		{
			Second.Value.To<DataSectionHandle>().Address = false;

			// Example:
			// mov rax, function_f_S0 => lea rax, [rip+function_f_S0]

			Build(
				X64_LOAD_ADDRESS_INSTRUCTION,
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
		}
		else if (Assembler.IsArm64 && Second.Value is DataSectionHandle)
		{
			var handle = Second.Value.To<DataSectionHandle>();

			if (!handle.Address)
			{
				// Example:
				// [S0] => x0
				//
				// adrp x0, S0
				// ldr x0, [x0, :lo12:S0]

				var address = new GetRelativeAddressInstruction(Unit, handle).Execute();

				Second.Value = new ComplexMemoryHandle(address, new Result(new Lower12Bits(handle), Assembler.Format), 1);

				Build(
					ARM64_LOAD_INSTRUCTION,
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

			handle.Address = true;

			// Example:
			// S0 => x0
			//
			// adrp x0, S0
			// add x0, x0, :lo12:S0

			Build(
				GetRelativeAddressInstruction.ARM64_RELATIVE_ADDRESS_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.BIT_LIMIT_64 | ParameterFlag.ALLOW_ADDRESS,
					HandleType.MEMORY
				)
			);

			// Add the lower 12 bits of the handle to the result of the instruction above
			Unit.Append(new AdditionInstruction(Unit, First, new Result(new Lower12Bits(handle), Assembler.Format), Assembler.Format, true), true);

			if (handle.Offset != 0)
			{
				// Add the handle offset
				Unit.Append(new AdditionInstruction(Unit, First, new Result(new ConstantHandle(handle.Offset), Assembler.Format), Assembler.Format, true), true);
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
					AdditionInstruction.SHARED_STANDARD_ADDITION_INSTRUCTION,
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
				X64_LOAD_ADDRESS_INSTRUCTION,
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
					ARM64_LOAD_INSTRUCTION,
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
				SHARED_MOVE_INSTRUCTION,
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
			if (Assembler.IsArm64 && Second.Value is ConstantHandle handle && handle.Bits > 16 && handle.Value is long constant)
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
				SHARED_MOVE_INSTRUCTION,
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

	public void OnPostBuildX64()
	{
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

			Operation = X64_UNSIGNED_CONVERSION;
			return;
		}

		if (Destination.Value.Size == 64 && Source.Value.Size == 32)
		{
			// Example: movsxd rax, ecx (64 <- 32)
			Operation = X64_SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE;
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

		Operation = X64_SIGNED_CONVERSION;
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

		var is_load = Operation == ARM64_LOAD_INSTRUCTION;
		var is_store = Operation == ARM64_STORE_INSTRUCTION;

		// NOTE: When a value is moved to 32-bit register, the higher bits are zeroed out
		if (is_load || is_store)
		{
			var inspected = is_load ? Source! : Destination!;
			var value = is_load ? Destination! : Source!;

			if (inspected.Value!.IsUnsigned)
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

				Operation = inspected.Size!.Bits switch
				{
					8 => is_load ? ARM64_LOAD_UINT8_INSTRUCTION : ARM64_STORE_UINT8_INSTRUCTION,
					16 => is_load ? ARM64_LOAD_UINT16_INSTRUCTION : ARM64_STORE_UINT16_INSTRUCTION,
					32 => is_load ? ARM64_LOAD_INSTRUCTION : ARM64_STORE_INSTRUCTION,
					64 => is_load ? ARM64_LOAD_INSTRUCTION : ARM64_STORE_INSTRUCTION,
					_ => throw new ApplicationException("Could not resolve the size of source value")
				};

				if (inspected.Size!.Bits == 64)
				{
					value.Value!.Format = Size.QWORD.ToFormat();
				}
				else
				{
					value.Value!.Format = Size.DWORD.ToFormat();
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

			Operation = inspected.Size!.Bits switch
			{
				8 => is_load ? ARM64_LOAD_INT8_INSTRUCTION : ARM64_STORE_UINT8_INSTRUCTION,
				16 => is_load ? ARM64_LOAD_INT16_INSTRUCTION : ARM64_STORE_UINT16_INSTRUCTION,
				32 => is_load ? (Destination!.Size!.Bits == 64 ? ARM64_LOAD_INT32_INSTRUCTION : ARM64_LOAD_INSTRUCTION) : ARM64_STORE_INSTRUCTION,
				64 => is_load ? ARM64_LOAD_INSTRUCTION : ARM64_STORE_INSTRUCTION,
				_ => throw new ApplicationException("Could not resolve the size of source value")
			};

			if (inspected.Size!.Bits == 64 || is_load && Destination!.Size!.Bits == 64)
			{
				value.Value!.Format = Size.QWORD.ToFormat();
			}
			else
			{
				value.Value!.Format = Size.DWORD.ToFormat();
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
				8 => ARM64_CONVERT_INT8_TO_INT64,
				16 => ARM64_CONVERT_INT16_TO_INT64,
				32 => ARM64_CONVERT_INT32_TO_INT64,
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

		Operation = SHARED_BITWISE_AND_INSTRUCTION;
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
			if (Source!.IsMemoryAddress)
			{
				return false;
			}

			Destination!.Value = handle;
			return true;
		}

		if (Operation == SHARED_MOVE_INSTRUCTION && handle.Is(HandleType.REGISTER))
		{
			Destination!.Value = handle;
			return true;
		}

		return false;
	}

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.MOVE;
	}
}