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

public class MoveInstruction : DualParameterInstruction
{
	public const string LOAD_ADDRESS_INSTRUCTION = "lea";
	public const string MOVE_INSTRUCTION = "mov";
	public const string UNSIGNED_CONVERSION = "movzx";
	public const string SIGNED_CONVERSION = "movsx";
	public const string SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE = "movsxd";

	public const string SINGLE_PRECISION_MOVE = "movss";
	public const string DOUBLE_PRECISION_MOVE = "movsd";

	public const string CONVERT_SINGLE_PRECISION_TO_INTEGER = "cvttss2si";
	public const string CONVERT_DOUBLE_PRECISION_TO_INTEGER = "cvttsd2si";

	public const string CONVERT_INTEGER_TO_SINGLE_PRECISION = "cvtsi2ss";
	public const string CONVERT_INTEGER_TO_DOUBLE_PRECISION = "cvtsi2sd";

	public const string MEDIA_REGISTER_BITWISE_XOR = "pxor";

	public const string CLEAR_INSTRUCTION = "xor";

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
				// Examples:
				// 0 => xmm0
				// pxor xmm0, xmm0
				if (Numbers.IsZero(Second.Value.To<ConstantHandle>().Value))
				{
					Build(
						MEDIA_REGISTER_BITWISE_XOR,
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
							flags_second | ParameterFlag.HIDDEN | ParameterFlag.ALLOW_64_BIT_CONSTANT,
							HandleType.CONSTANT
						)
					);

					return;
				}

				// Ensure the source value is in decimal format
				Second.Value.To<ConstantHandle>().Convert(Format.DECIMAL);
				Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
				Second.Format = Format.DECIMAL;

				instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

				// Examples:
				// movsd xmm0, qword [C1] (C1: 10.0)

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
			else
			{
				instruction = Assembler.IsTargetX86 ? CONVERT_INTEGER_TO_SINGLE_PRECISION : CONVERT_INTEGER_TO_DOUBLE_PRECISION;

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
				// Ensure the source value is in integer format
				Second.Value.To<ConstantHandle>().Convert(First.Format);
				Second.Format = First.Format;

				instruction = MOVE_INSTRUCTION;

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
				instruction = Assembler.IsTargetX86 ? CONVERT_SINGLE_PRECISION_TO_INTEGER : CONVERT_DOUBLE_PRECISION_TO_INTEGER;

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

					Build(
						MOVE_INSTRUCTION,
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
					Build(
						MOVE_INSTRUCTION,
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

					Build(
						MOVE_INSTRUCTION,
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
					instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

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
		}
		else if (First.IsMediaRegister && Second.IsConstant && Numbers.IsZero(Second.Value.To<ConstantHandle>().Value))
		{
			// Examples:
			// 0 => xmm0
			// pxor xmm0, xmm0

			Build(
				MEDIA_REGISTER_BITWISE_XOR,
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
					flags_second | ParameterFlag.HIDDEN | ParameterFlag.ALLOW_64_BIT_CONSTANT,
					HandleType.CONSTANT
				)
			);
		}
		else
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

			if (Second.IsConstant)
			{
				// Move the source value to the data section so that it can be loaded to a media register
				Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
			}

			if (First.IsMemoryAddress)
			{
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
			else
			{
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
						HandleType.MEDIA_REGISTER,
						HandleType.MEMORY
					)
				);
			}
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
			// Example: xor rax, rax
			Build(
				CLEAR_INSTRUCTION,
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
			// Examples:
			// mov [rsp+8], 314159
			// mov [rdi], -1
			Build(
				MOVE_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER,
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
		else if (Assembler.IsTargetX64 && Second.Value is DataSectionHandle handle && handle.Address)
		{
			// Examples (64-bit mode only):
			// mov rax, function_f_S0 => lea rax, [rip+function_f_S0]

			Second.Value.To<DataSectionHandle>().Address = false;

			Build(
				LOAD_ADDRESS_INSTRUCTION,
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
		else if (Second.IsExpression)
		{
			// Examples:
			// lea rcx, [rbx+16]
			// lea rax, [rcx*4+rbx-1]
			Build(
				LOAD_ADDRESS_INSTRUCTION,
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
		else
		{
			// Examples:
			// mov rax, 7
			// mov rbx, rax
			// mov rcx, [rsp+8]
			Build(
				MOVE_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second | ParameterFlag.ALLOW_64_BIT_CONSTANT,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}
	}

	public override void OnPostBuild()
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

			Operation = UNSIGNED_CONVERSION;
			return;
		}

		if (Destination.Value.Size == 64 && Source.Value.Size == 32)
		{
			// Example: movsxd rax, ecx (64 <- 32)
			Operation = SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE;
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

		Operation = SIGNED_CONVERSION;
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