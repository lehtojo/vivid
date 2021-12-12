using System.Collections.Generic;
using System.Linq;

public class ImportPattern : Pattern
{
	public const string CPP_LANGUAGE_TAG_1 = "cpp";
	public const string CPP_LANGUAGE_TAG_2 = "c++";
	public const string VIVID_LANGUAGE_TAG = "vivid";

	private const int PRIORITY = 20;

	private const int IMPORT = 0;
	private const int LANGUAGE = 1;
	private const int FUNCTION = 2;
	private const int COLON = 3;

	private const int TYPE_START = 1;

	// Pattern 1: import ['$language'] $name (...) [: $type]
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

		// Pattern: import ['$language'] $name (...) [: $type]
		// Optionally consume a language identifier
		Consume(state, TokenType.STRING | TokenType.OPTIONAL);

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

	/// <summary>
	/// Return whether the captured tokens represent a function import instead of namespace import
	/// </summary>
	private static bool IsFunctionImport(List<Token> tokens)
	{
		return !tokens[TYPE_START].Is(TokenType.IDENTIFIER);
	}

	/// <summary>
	/// Imports the function contained in the specified tokens
	/// </summary>
	private static void ImportFunction(Context environment, List<Token> tokens)
	{
		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var language = FunctionLanguage.VIVID;

		if (tokens[LANGUAGE].Is(TokenType.STRING))
		{
			language = tokens[LANGUAGE].To<StringToken>().Text.ToLowerInvariant() switch
			{
				CPP_LANGUAGE_TAG_1 => FunctionLanguage.CPP,
				CPP_LANGUAGE_TAG_2 => FunctionLanguage.CPP,
				VIVID_LANGUAGE_TAG => FunctionLanguage.VIVID,
				_ => FunctionLanguage.OTHER
			};
		}

		var return_type = Primitives.CreateUnit();

		// If the colon operator is present, it means there is a return type in the tokens
		if (tokens[COLON].Is(Operators.COLON))
		{
			return_type = Common.ReadType(environment, new Queue<Token>(tokens.Skip(COLON + 1))) ?? throw Errors.Get(tokens[COLON].Position, "Can not resolve the return type");
		}

		var function = new Function(environment, Modifier.DEFAULT | Modifier.IMPORTED, descriptor.Name, descriptor.Position, null);
		function.Language = language;

		var parameters = descriptor.GetParameters(function);
		function.Parameters.AddRange(parameters);

		var implementation = new FunctionImplementation(function, parameters, return_type, environment);
		implementation.IsImported = true;
		function.Implementations.Add(implementation);

		implementation.Implement(function.Blueprint);

		environment.Declare(function);
	}

	/// <summary>
	/// Imports the namespace contained in the specified tokens
	/// </summary>
	private static void ImportNamespace(Context environment, List<Token> tokens)
	{
		var import = Common.ReadType(environment, new Queue<Token>(tokens.Skip(1)));
		if (import == null) throw Errors.Get(tokens.First().Position, "Can not resolve the import");
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