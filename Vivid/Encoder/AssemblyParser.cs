using System.Collections.Generic;
using System.Linq;
using System;

public class AssemblyParser
{
	public Unit Unit { get; set; }
	public Dictionary<string, RegisterHandle> Registers { get; set; } = new Dictionary<string, RegisterHandle>();
	public List<Instruction> Instructions { get; set; } = new List<Instruction>();
	public DataEncoderModule Data { get; set; } = new DataEncoderModule();

	public AssemblyParser()
	{
		Unit = new Unit();

		var n = (int)Math.Log2(Assembler.Size.Bytes) + 1;

		for (var i = 0; i < n; i++)
		{
			var size = Size.FromBytes(1 << (n - 1 - i)).ToFormat();

			foreach (var register in Unit.StandardRegisters)
			{
				var handle = new RegisterHandle(register);
				handle.Format = size;
				Registers.Add(register.Partitions[i], handle);
			}
		}

		foreach (var register in Unit.MediaRegisters)
		{
			var handle = new RegisterHandle(register);
			Registers.Add(register.Partitions[0], handle);
		}
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

		// TODO: Apply directive
		return true;
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

		Instructions.Add(new LabelInstruction(Unit, new Label(tokens[0].To<IdentifierToken>().Value)));
		return true;
	}

	private Handle ParseParameter(List<Token> all, int i)
	{
		var parameter = all[i];

		if (parameter.Type == TokenType.IDENTIFIER)
		{
			// Return a register handle, if the token represents one
			if (Registers.TryGetValue(parameter.To<IdentifierToken>().Value, out var handle)) return handle;

			return new DataSectionHandle(parameter.To<IdentifierToken>().Value, true);
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
				var value = new Result(ParseParameter(tokens, 0), Assembler.Format);
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
				else if (!tokens[0].Is(Operators.ADD))
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
					var first = new Result(ParseParameter(tokens, 0), Assembler.Format);
					var second = new Result(ParseParameter(tokens, 2), Assembler.Format);

					return new ComplexMemoryHandle(first, second, 1);
				}

				// Pattern: $register * $number
				if (tokens[1].Is(Operators.MULTIPLY) && tokens[2].Type == TokenType.NUMBER)
				{
					var first = new Result(new ConstantHandle(0L), Assembler.Format);
					var second = new Result(ParseParameter(tokens, 0), Assembler.Format);
					var stride = (long)tokens[2].To<NumberToken>().Value;

					return new ComplexMemoryHandle(first, second, (int)stride);
				}
			}
			else if (tokens.Count == 5)
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
				else if (!tokens[3].Is(Operators.ADD))
				{
					offset = (long)tokens[4].To<NumberToken>().Value;
				}
				else
				{
					throw Errors.Get(tokens[3].Position, "Expected the second last token to be a plus or minus operator");
				}

				var first = new Result(ParseParameter(tokens, 0), Assembler.Format);

				// Patterns: $register + $register + $number / $register + $register - $number
				if (tokens[1].Is(Operators.ADD))
				{
					var second = new Result(ParseParameter(tokens, 2), Assembler.Format);

					return new ComplexMemoryHandle(first, second, 1, (int)offset);
				}

				// Patterns: $register * $number + $register / $register * $number + $number / $register * $number - $number
				if (tokens[1].Is(Operators.MULTIPLY))
				{
					var stride = (long)tokens[2].To<NumberToken>().Value;
					var second = new Result(ParseParameter(tokens, 4), Assembler.Format);

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
				else if (!tokens[5].Is(Operators.ADD))
				{
					offset = (long)tokens[6].To<NumberToken>().Value;
				}
				else
				{
					throw Errors.Get(tokens[5].Position, "Expected the second last token to be a plus or minus operator");
				}

				// Patterns: $register * $number + $register + $number
				var first = new Result(ParseParameter(tokens, 0), Assembler.Format);
				var stride = (long)tokens[2].To<NumberToken>().Value;
				var second = new Result(ParseParameter(tokens, 4), Assembler.Format);

				return new ComplexMemoryHandle(second, first, (int)stride, (int)offset);
			}
		}

		throw new NotSupportedException("Can not understand the token");
	}

	public bool ParseInstruction(List<Token> tokens)
	{
		if (tokens[0].Type != TokenType.IDENTIFIER) return false;
		var operation = tokens[0].To<IdentifierToken>().Value;

		var parameters = new List<InstructionParameter>();
		var position = 1; // Start after the operation identifier

		while (position < tokens.Count)
		{
			// Parse the next parameter
			var parameter = ParseParameter(tokens, position);
			parameters.Add(new InstructionParameter(parameter, ParameterFlag.NONE));

			// Try to find the next comma, which marks the start of the next parameter
			position = tokens.FindIndex(position + 1, i => i.Is(Operators.COMMA)) + 1;
			if (position == 0) break;
		}

		var instruction = new Instruction(Unit, InstructionType.NORMAL);
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

			if (ParseLabel(tokens)) continue;
			if (ParseInstruction(tokens)) continue;
			if (ParseDirective(tokens)) continue;
		}
	}
}