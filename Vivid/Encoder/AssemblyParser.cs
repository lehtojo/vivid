using System.Collections.Generic;
using System.Linq;
using System;

public class AssemblyParser
{
	public const string TEXT_SECTION = ".text";

	public const string BYTE_SPECIFIER = "byte";
	public const string WORD_SPECIFIER = "word";
	public const string DWORD_SPECIFIER = "dword";
	public const string QWORD_SPECIFIER = "qword";
	public const string XWORD_SPECIFIER = "xword";
	public const string OWORD_SPECIFIER = "yword";
	public const string SECTION_OFFSET_SPECIFIER = "section_offset";

	public const string EXPORT_DIRECTIVE = "export";
	public const string SECTION_DIRECTIVE = "section";
	public const string STRING_DIRECTIVE = "string";
	public const string CHARACTERS_DIRECTIVE = "characters";
	public const string LINE_DIRECTIVE = "loc";
	public const string DEBUG_FILE_DIRECTIVE = "debug_file";
	public const string DEBUG_START_DIRECTIVE = "debug_start";
	public const string DEBUG_FRAME_OFFSET_DIRECTIVE = "debug_frame_offset";
	public const string DEBUG_END_DIRECTIVE = "debug_end";

	public Unit Unit { get; set; }
	public Dictionary<string, RegisterHandle> Registers { get; } = new Dictionary<string, RegisterHandle>();
	public List<Instruction> Instructions { get; } = new List<Instruction>();
	public Dictionary<string, DataEncoderModule> Sections { get; } = new Dictionary<string, DataEncoderModule>();
	public HashSet<string> Exports { get; } = new HashSet<string>();
	public DataEncoderModule? Data { get; set; }
	public string? DebugFile { get; set; } = null;
	public string Section { get; set; } = TEXT_SECTION;

	public AssemblyParser()
	{
		Unit = new Unit();

		// Add every standard register partition as a register handle
		var n = (int)Math.Log2(Assembler.Size.Bytes) + 1;

		for (var i = 0; i < n; i++)
		{
			var size = Size.FromBytes(1 << (n - 1 - i)).ToFormat();

			foreach (var register in Unit.Registers.Where(i => !i.IsMediaRegister))
			{
				var handle = new RegisterHandle(register);
				handle.Format = size;
				Registers.Add(register.Partitions[i], handle);
			}
		}

		// Add every media register as a register handle
		foreach (var register in Unit.MediaRegisters.Where(i => i.IsMediaRegister))
		{
			var handle = new RegisterHandle(register);
			Registers.Add(register.Partitions[1], handle);
		}
	}

	/// <summary>
	/// Handles offset directives: . $allocator $to - $from
	/// </summary>
	private bool ExecuteOffsetAllocator(List<Token> tokens)
	{
		// Pattern: . $allocator $to - $from
		if (tokens.Count < 5 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.IDENTIFIER || !tokens[3].Is(Operators.SUBTRACT) || tokens[4].Type != TokenType.IDENTIFIER) return false;

		var to = tokens[2].To<IdentifierToken>().Value;
		var from = tokens[4].To<IdentifierToken>().Value;

		var bytes = tokens[1].To<IdentifierToken>().Value switch
		{
			BYTE_SPECIFIER => 1, // Pattern: .byte $to - $from
			WORD_SPECIFIER => 2, // Pattern: .word $to - $from
			DWORD_SPECIFIER => 4, // Pattern: .dword $to - $from
			QWORD_SPECIFIER => 8, // Pattern: .qword $to - $from
			XWORD_SPECIFIER => throw Errors.Get(tokens[1].Position, "Please use smaller allocators"),
			OWORD_SPECIFIER => throw Errors.Get(tokens[1].Position, "Please use smaller allocators"),
			_ => throw Errors.Get(tokens[1].Position, "Unknown allocator")
		};

		var offset = new Offset(new TableLabel(from), new TableLabel(to));

		Data!.Offsets.Add(new BinaryOffset(Data.Position, offset, bytes));
		Data!.Zero(bytes);
		return true;
	}

	/// <summary>
	/// Handles symbol reference allocators: . $allocator $symbol
	/// </summary>
	private bool ExecuteSymbolReferenceAllocator(List<Token> tokens)
	{
		// Pattern: . $allocator $symbol
		if (tokens.Count < 3 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.IDENTIFIER) return false;

		var symbol = tokens[2].To<IdentifierToken>().Value;
		var allocator = tokens[1].To<IdentifierToken>().Value;

		var bytes = allocator switch
		{
			BYTE_SPECIFIER => throw Errors.Get(tokens[1].Position, "Only 32-bit and 64-bit symbol references are currently supported"),
			WORD_SPECIFIER => throw Errors.Get(tokens[1].Position, "Only 32-bit and 64-bit symbol references are currently supported"),
			DWORD_SPECIFIER => 4, // Pattern: .dword $symbol
			QWORD_SPECIFIER => 8, // Pattern: .qword $symbol
			XWORD_SPECIFIER => throw Errors.Get(tokens[1].Position, "Only 32-bit and 64-bit symbol references are currently supported"),
			OWORD_SPECIFIER => throw Errors.Get(tokens[1].Position, "Only 32-bit and 64-bit symbol references are currently supported"),
			_ => throw Errors.Get(tokens[1].Position, "Unknown allocator")
		};

		var relocation_type = allocator switch
		{
			DWORD_SPECIFIER => BinaryRelocationType.ABSOLUTE32,
			QWORD_SPECIFIER => BinaryRelocationType.ABSOLUTE64,
			SECTION_OFFSET_SPECIFIER => BinaryRelocationType.SECTION_RELATIVE,
			_ => BinaryRelocationType.ABSOLUTE64,
		};

		Data!.Relocations.Add(new BinaryRelocation(Data.GetLocalOrCreateExternalSymbol(symbol), Data.Position, 0, relocation_type, bytes));
		Data.Zero(bytes);
		return true;
	}

	/// <summary>
	/// Executes the specified directive, if it represents a section directive.
	/// Section directive switches the active section.
	/// </summary>
	private bool ExecuteSectionDirective(List<Token> tokens)
	{
		if (tokens.Count < 3 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.IDENTIFIER) return false;

		// Pattern: .section $section
		if (tokens[1].To<IdentifierToken>().Value != SECTION_DIRECTIVE) return false;

		// Switch the active section
		var section = '.' + tokens[2].To<IdentifierToken>().Value;

		if (section == TEXT_SECTION)
		{
			// Save the current data section, if it is not saved already
			if (Data != null && !Sections.ContainsKey(Section))
			{
				Sections[Section] = Data;
			}

			Data = null;
			Section = section;
			return true;
		}

		Section = section;

		// All non-text sections are data sections, create a new data section if no previous data section has the specified name
		if (Sections.TryGetValue(section, out var saved))
		{
			Data = saved;
			return true;
		}

		Data = new DataEncoderModule();
		Data.Name = Section;
		Sections[Section] = Data;
		return true;
	}

	/// <summary>
	/// Executes the specified directive, if it exports a symbol.
	/// </summary>
	private bool ExecuteExportDirective(List<Token> tokens)
	{
		if (tokens.Count < 3 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.IDENTIFIER) return false;

		// Pattern: .export $symbol
		if (tokens[1].To<IdentifierToken>().Value != EXPORT_DIRECTIVE) return false;

		var name = tokens[2].To<IdentifierToken>().Value;

		if (Data == null)
		{
			Instructions.Add(new LabelInstruction(Unit, new Label(name)));
		}
		else
		{
			Data.CreateLocalSymbol(name, Data.Position);
		}

		Exports.Add(name);
		return true;
	}

	/// <summary>
	/// Executes the specified directive, if it controls debug information.
	/// </summary>
	private bool ExecuteDebugDirective(List<Token> tokens)
	{
		if (tokens.Count < 2 || tokens[1].Type != TokenType.IDENTIFIER) return false;

		var directive = tokens[1].To<IdentifierToken>().Value;

		if (directive == LINE_DIRECTIVE)
		{
			// Pattern: .line $file $line $character
			if (tokens.Count < 5 || tokens[2].Type != TokenType.NUMBER || tokens[3].Type != TokenType.NUMBER || tokens[4].Type != TokenType.NUMBER) return false;

			var file = (long)tokens[2].To<NumberToken>().Value;
			var line = (long)tokens[3].To<NumberToken>().Value;
			var character = (long)tokens[4].To<NumberToken>().Value;

			Instructions.Add(new AppendPositionInstruction(Unit, new Position(null, (int)line, (int)character)));
			return true;
		}

		if (directive == DEBUG_FILE_DIRECTIVE)
		{
			// Pattern: .debug_file $file
			if (tokens.Count < 3 || tokens[2].Type != TokenType.STRING) return false;

			DebugFile = tokens[2].To<StringToken>().Text;
			return true;
		}

		if (directive == DEBUG_START_DIRECTIVE)
		{
			// Pattern: .debug_start $symbol
			if (tokens.Count < 3 || tokens[2].Type != TokenType.IDENTIFIER) return false;

			var symbol = tokens[2].To<IdentifierToken>().Value;

			var instruction = new Instruction(Unit, InstructionType.DEBUG_START);
			var handle = new DataSectionHandle(symbol);
			instruction.Parameters.Add(new InstructionParameter(handle, ParameterFlag.NONE));
			Instructions.Add(instruction);
			return true;
		}

		if (directive == DEBUG_FRAME_OFFSET_DIRECTIVE)
		{
			// Pattern: .debug_start $symbol
			if (tokens.Count < 3 || tokens[2].Type != TokenType.NUMBER) return false;

			var offset = (long)tokens[2].To<NumberToken>().Value;

			var instruction = new Instruction(Unit, InstructionType.DEBUG_FRAME_OFFSET);
			var handle = new ConstantHandle(offset);
			instruction.Parameters.Add(new InstructionParameter(handle, ParameterFlag.NONE));
			Instructions.Add(instruction);
			return true;
		}

		if (directive == DEBUG_END_DIRECTIVE)
		{
			// Pattern: .debug_end
			Instructions.Add(new Instruction(Unit, InstructionType.DEBUG_END));
			return true;
		}

		return false;
	}

	/// <summary>
	/// Executes the specified directive, if it allocates some primitive type such as byte or word.
	/// </summary>
	private bool ExecuteConstantAllocator(List<Token> tokens)
	{
		if (tokens.Count < 3 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.NUMBER) return false;

		var directive = tokens[1].To<IdentifierToken>().Value;
		var value = 0L;

		// If the number is a decimal, load its raw bits as an integer value
		if (tokens[2].To<NumberToken>().Format.IsDecimal())
		{
			value = BitConverter.DoubleToInt64Bits((double)tokens[2].To<NumberToken>().Value);
		}
		else
		{
			value = (long)tokens[2].To<NumberToken>().Value;
		}

		switch (directive)
		{
			case BYTE_SPECIFIER: { Data!.Write(value); break; } // Pattern: .byte $value
			case WORD_SPECIFIER: { Data!.WriteInt16(value); break; } // Pattern: .word $value
			case DWORD_SPECIFIER: { Data!.WriteInt32(value); break; } // Pattern: .dword $value
			case QWORD_SPECIFIER: { Data!.WriteInt64(value); break; } // Pattern: .qword $value
			case XWORD_SPECIFIER:
			case OWORD_SPECIFIER: throw Errors.Get(tokens[1].Position, "Please use smaller allocators");
			default: return false;
		}

		return true;
	}

	/// <summary>
	/// Allocates a string, if the specified tokens represent a string allocator
	/// </summary>
	private bool ExecuteStringAllocator(List<Token> tokens)
	{
		if (tokens.Count < 3 || tokens[1].Type != TokenType.IDENTIFIER || tokens[2].Type != TokenType.STRING) return false;

		switch (tokens[1].To<IdentifierToken>().Value)
		{
			case STRING_DIRECTIVE:
			// Pattern: .string '...'
			Data!.String(tokens[2].To<StringToken>().Text);
			break;

			case CHARACTERS_DIRECTIVE:
			// Pattern: .characters '...'
			Data!.String(tokens[2].To<StringToken>().Text, false);
			break;

			default: return false;
		}

		return true;
	}

	/// <summary>
	/// Applies a directive if the specified tokens represent a directive.
	/// Pattern: . $directive $1 $2 ... $n
	/// </summary>
	private bool ParseDirective(List<Token> tokens)
	{
		// Directives start with a dot
		if (!tokens.First().Is(Operators.DOT)) return false;

		// The second token must be the identifier of the directive
		if (tokens.Count == 1 || !tokens[1].Is(TokenType.IDENTIFIER, TokenType.KEYWORD)) return false;

		if (ExecuteSectionDirective(tokens)) return true;
		if (ExecuteExportDirective(tokens)) return true;
		if (ExecuteDebugDirective(tokens)) return true;

		// The executors below are only executed if we are in the data section
		if (Data == null) return false;

		if (ExecuteOffsetAllocator(tokens)) return true;
		if (ExecuteSymbolReferenceAllocator(tokens)) return true;
		if (ExecuteConstantAllocator(tokens)) return true;
		if (ExecuteStringAllocator(tokens)) return true;

		return false;
	}

	/// <summary>
	/// Forms a label if the specified tokens represent a label.
	/// Pattern: $name :
	/// </summary>
	private bool ParseLabel(List<Token> tokens)
	{
		// Labels must begin with an identifier
		if (!tokens.First().Is(TokenType.IDENTIFIER)) return false;

		// Labels must end with a colon
		if (tokens.Count == 1 || !tokens[1].Is(Operators.COLON)) return false;

		var name = tokens[0].To<IdentifierToken>().Value;

		if (Data == null)
		{
			Instructions.Add(new LabelInstruction(Unit, new Label(name)));
		}
		else
		{
			Data.CreateLocalSymbol(name, Data.Position);
		}

		return true;
	}

	/// <summary>
	/// Tries to form a instruction parameter handle from the specified tokens starting at the specified offset.
	/// Instrucion parameters are registers, memory addresses and numbers for instance.
	/// </summary>
	private Handle ParseInstructionParameter(List<Token> all, int i)
	{
		var parameter = all[i];

		if (parameter.Type == TokenType.IDENTIFIER)
		{
			var value = parameter.To<IdentifierToken>().Value;

			// Return a register handle, if the token represents one
			if (Registers.TryGetValue(value, out var register)) return register;

			// If the identifier represents a size specifier, determine how many bytes it represents
			var bytes = value switch
			{
				BYTE_SPECIFIER => 1,
				WORD_SPECIFIER => 2,
				DWORD_SPECIFIER => 4,
				QWORD_SPECIFIER => 8,
				XWORD_SPECIFIER => 16,
				OWORD_SPECIFIER => 32,
				_ => 0
			};

			// If the variable 'bytes' is positive, it means the current identifier is a size specified and a memory address should follow it
			if (bytes > 0)
			{
				// Ensure the next token represents a memory address
				if (++i >= all.Count || all[i].Type != TokenType.CONTENT) throw Errors.Get(parameter.Position, "Expected a memory address after this size specifier");

				var memory_address = ParseInstructionParameter(all, i);
				memory_address.Format = Size.FromBytes(bytes).ToFormat();

				return memory_address;
			}

			// Since the identifier is not a register or a size specifier, it must be a symbol
			return new DataSectionHandle(value, true);
		}

		if (parameter.Type == TokenType.NUMBER)
		{
			var number = parameter.To<NumberToken>();
			return new ConstantHandle(number.Value, number.Format);
		}

		if (parameter.Type == TokenType.CONTENT)
		{
			var tokens = parameter.To<ContentToken>().Tokens;

			if (tokens.Count == 1)
			{
				// Patterns: $register / $symbol / $number
				var value = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);
				return new MemoryHandle(Unit, value, 0);
			}
			else if (tokens.Count == 2)
			{
				// Patterns: - $number
				var offset = 0L;

				// Ensure the last operator is a plus or minus operator
				// Also handle the negation of the integer offset.
				if (tokens[0].Is(Operators.SUBTRACT))
				{
					offset = -(long)tokens[1].To<NumberToken>().Value;
				}
				else if (tokens[0].Is(Operators.ADD))
				{
					offset = (long)tokens[1].To<NumberToken>().Value;
				}
				else
				{
					throw Errors.Get(tokens[0].Position, "Expected the first token to be a plus or minus operator");
				}

				return new MemoryHandle(Unit, new Result(new ConstantHandle(offset), Assembler.Format), 0);
			}
			else if (tokens.Count == 3)
			{
				// Patterns: $register + $register / $register + $number / $register - $number / $symbol + $number
				if (tokens[1].Is(Operators.ADD))
				{
					var first = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);
					var second = new Result(ParseInstructionParameter(tokens, 2), Assembler.Format);

					return new ComplexMemoryHandle(second, first, 1);
				}

				if (tokens[1].Is(Operators.SUBTRACT))
				{
					var first = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);
					var second = new Result(new ConstantHandle(-(long)tokens[2].To<NumberToken>().Value), Assembler.Format);

					return new ComplexMemoryHandle(second, first, 1);
				}

				// Pattern: $register * $number
				if (tokens[1].Is(Operators.MULTIPLY) && tokens[2].Type == TokenType.NUMBER)
				{
					var first = new Result(new ConstantHandle(0L), Assembler.Format);
					var second = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);
					var stride = (long)tokens[2].To<NumberToken>().Value;

					return new ComplexMemoryHandle(first, second, (int)stride);
				}
			}
			else if (tokens.Count == 5)
			{
				var first = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);

				// Patterns: $register + $register + $number / $register + $register - $number
				if (tokens[1].Is(Operators.ADD))
				{
					// Ensure the last token is a number
					if (tokens[4].Type != TokenType.NUMBER)
					{
						throw Errors.Get(tokens[4].Position, "Expected the last token to be an integer number");
					}

					var offset = 0L;

					// Ensure the last operator is a plus or minus operator
					// Also handle the negation of the integer offset.
					if (tokens[3].Is(Operators.SUBTRACT))
					{
						offset = -(long)tokens[4].To<NumberToken>().Value;
					}
					else if (tokens[3].Is(Operators.ADD))
					{
						offset = (long)tokens[4].To<NumberToken>().Value;
					}

					var second = new Result(ParseInstructionParameter(tokens, 2), Assembler.Format);

					return new ComplexMemoryHandle(first, second, 1, (int)offset);
				}

				// Patterns: $register * $number + $register / $register * $number + $number / $register * $number - $number
				if (tokens[1].Is(Operators.MULTIPLY))
				{
					var stride = (long)tokens[2].To<NumberToken>().Value;
					var second = new Result(ParseInstructionParameter(tokens, 4), Assembler.Format);

					return new ComplexMemoryHandle(second, first, (int)stride, 0);
				}
			}
			else if (tokens.Count == 7)
			{
				// Ensure the last token is a number
				if (tokens[6].Type != TokenType.NUMBER)
				{
					throw Errors.Get(tokens[4].Position, "Expected the last token to be an integer number");
				}

				var offset = 0L;

				// Ensure the last operator is a plus or minus operator
				// Also handle the negation of the integer offset.
				if (tokens[5].Is(Operators.SUBTRACT))
				{
					offset = -(long)tokens[6].To<NumberToken>().Value;
				}
				else if (tokens[5].Is(Operators.ADD))
				{
					offset = (long)tokens[6].To<NumberToken>().Value;
				}
				else
				{
					throw Errors.Get(tokens[5].Position, "Expected the second last token to be a plus or minus operator");
				}

				// Patterns: $register * $number + $register + $number
				var first = new Result(ParseInstructionParameter(tokens, 0), Assembler.Format);
				var stride = (long)tokens[2].To<NumberToken>().Value;
				var second = new Result(ParseInstructionParameter(tokens, 4), Assembler.Format);

				return new ComplexMemoryHandle(second, first, (int)stride, (int)offset);
			}
		}

		if (parameter.Is(Operators.SUBTRACT))
		{
			if (i + 1 >= all.Count) throw Errors.Get(all[i].Position, "Expected an integer number");

			// Parse the number and negate it
			var number = ParseInstructionParameter(all, i + 1);
			number.To<ConstantHandle>().Value = -(long)number.To<ConstantHandle>().Value;

			return number;
		}

		throw Errors.Get(all[i].Position, "Can not understand");
	}

	/// <summary>
	/// Returns whether the specified operation represents a jump instruction
	/// </summary>
	private static bool IsJump(string operation)
	{
		return operation == global::Instructions.X64.JUMP ||
			operation == global::Instructions.X64.JUMP_ABOVE ||
			operation == global::Instructions.X64.JUMP_ABOVE_OR_EQUALS ||
			operation == global::Instructions.X64.JUMP_BELOW ||
			operation == global::Instructions.X64.JUMP_BELOW_OR_EQUALS ||
			operation == global::Instructions.X64.JUMP_EQUALS ||
			operation == global::Instructions.X64.JUMP_GREATER_THAN ||
			operation == global::Instructions.X64.JUMP_GREATER_THAN_OR_EQUALS ||
			operation == global::Instructions.X64.JUMP_LESS_THAN ||
			operation == global::Instructions.X64.JUMP_LESS_THAN_OR_EQUALS ||
			operation == global::Instructions.X64.JUMP_NOT_EQUALS ||
			operation == global::Instructions.X64.JUMP_NOT_ZERO ||
			operation == global::Instructions.X64.JUMP_ZERO;
	}

	/// <summary>
	/// Tries to create an instruction from the specified tokens
	/// </summary>
	public bool ParseInstruction(List<Token> tokens)
	{
		if (tokens[0].Type != TokenType.IDENTIFIER) return false;
		var operation = tokens[0].To<IdentifierToken>().Value;

		var parameters = new List<InstructionParameter>();
		var position = 1; // Start after the operation identifier

		while (position < tokens.Count)
		{
			// Parse the next parameter
			var parameter = ParseInstructionParameter(tokens, position);
			parameters.Add(new InstructionParameter(parameter, ParameterFlag.NONE));

			// Try to find the next comma, which marks the start of the next parameter
			position = tokens.FindIndex(position + 1, i => i.Is(Operators.COMMA)) + 1;
			if (position == 0) break;
		}

		var instruction = new Instruction(Unit, IsJump(operation) ? InstructionType.JUMP : InstructionType.NORMAL);
		instruction.Parameters.AddRange(parameters);
		instruction.Operation = operation;

		Instructions.Add(instruction);
		return true;
	}

	public void Parse(string assembly)
	{
		var lines = assembly.Split('\n');

		foreach (var line in lines)
		{
			// Tokenize the current line
			var tokens = Lexer.GetTokens(line);

			// Skip empty lines
			if (!tokens.Any()) continue;

			// Parse directives here, because all sections have some directives
			if (ParseDirective(tokens)) continue;

			// Parse labels here, because all sections have labels
			if (ParseLabel(tokens)) continue;

			if (Section == TEXT_SECTION)
			{
				if (ParseInstruction(tokens)) continue;
			}

			#warning Enable in the future
			//throw Errors.Get(tokens.First().Position, "Can not understand");
		}

		// Save the current data section, if it is not saved already
		if (Data != null && !Sections.ContainsKey(Section))
		{
			Sections[Section] = Data;
		}
	}

	public void Reset()
	{
		Instructions.Clear();
		Sections.Clear();

		Data?.Reset();
		Data = null;

		Section = TEXT_SECTION;
	}
}