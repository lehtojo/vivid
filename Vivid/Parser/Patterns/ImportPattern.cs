using System.Collections.Generic;
using System.Linq;

public class ImportPattern : Pattern
{
	private const int PRIORITY = 20;

	private const int IMPORT = 0;
	private const int OBJECT = 1;
	private const int COLON = 2;

	// Pattern 1: import $name (...) [: $type]
	// Pattern 2: import $1.$2. ... .$n
	public ImportPattern() : base
	(
		TokenType.KEYWORD
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var token = tokens[IMPORT].To<KeywordToken>();

		if (token.Keyword != Keywords.IMPORT) return false;

		var next = Peek(state);

		// Pattern: import $1.$2. ... .$n
		if (next != null && next.Is(TokenType.IDENTIFIER))
		{
			return Common.ConsumeType(state);
		}

		// Pattern: import $name (...) [: $type]
		if (!Consume(state, TokenType.FUNCTION)) return false;

		// Try to consume the return type
		if (Try(state, () => Consume(state, out Token? token, TokenType.OPERATOR) && token!.Is(Operators.COLON)))
		{
			return Common.ConsumeType(state);
		}

		// There is no return type, so add an empty token
		state.Formatted.Add(new Token(TokenType.NONE));
		return true;
	}

	private static bool IsFunctionImport(List<Token> tokens)
	{
		return tokens[OBJECT].Is(TokenType.FUNCTION);
	}

	private static void ImportFunction(Context environment, List<Token> tokens)
	{
		var header = tokens[OBJECT].To<FunctionToken>();
		var return_type = Types.UNIT;

		if (tokens[COLON].Is(Operators.COLON))
		{
			return_type = Common.ReadType(environment, new Queue<Token>(tokens.Skip(COLON + 1))) ?? throw Errors.Get(tokens[COLON].Position, "Could not resolve the return type");
		}

		var function = new Function(environment, Modifier.DEFAULT | Modifier.EXTERNAL, header.Name) { Position = header.Position };

		var parameters = header.GetParameters(function);
		function.Parameters.AddRange(parameters);

		var implementation = new FunctionImplementation(function, parameters, return_type, environment);
		function.Implementations.Add(implementation);

		implementation.Implement(function.Blueprint);

		environment.Declare(function);
	}

	private static void ImportNamespace(Context environment, List<Token> tokens)
	{
		var import = Common.ReadType(environment, new Queue<Token>(tokens.Skip(1)));
		if (import == null) throw Errors.Get(tokens.First().Position, "Could not resolve the import");
		environment.Imports.Add(import);
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		if (IsFunctionImport(tokens))
		{
			ImportFunction(environment, tokens);
		}
		else
		{
			ImportNamespace(environment, tokens);
		}

		return null;
	}
}