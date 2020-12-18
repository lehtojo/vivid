using System.Collections.Generic;

public class ImportPattern : Pattern
{
	private const int PRIORITY = 20;

	private const int IMPORT = 0;
	private const int HEADER = 1;
	private const int OPERATOR = 2;
	private const int RETURN_TYPE = 3;

	// import a-z (...): Type
	// import a-z (...)
	public ImportPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.FUNCTION,
		TokenType.OPERATOR | TokenType.OPTIONAL,
		TokenType.IDENTIFIER | TokenType.OPTIONAL
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var keyword = tokens[IMPORT].To<KeywordToken>();

		if (keyword.Keyword != Keywords.IMPORT)
		{
			return false;
		}

		if (tokens[OPERATOR].Type == TokenType.NONE && tokens[RETURN_TYPE].Type == TokenType.NONE)
		{
			return true;
		}

		if (tokens[OPERATOR].Type != TokenType.NONE && tokens[RETURN_TYPE].Type != TokenType.NONE)
		{
			return Equals(tokens[OPERATOR].To<OperatorToken>().Operator, Operators.COLON);
		}

		return false;
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		var function_context = new Context();
		function_context.Link(environment);

		var header = tokens[HEADER].To<FunctionToken>();
		var parameters = header.GetParameters(function_context);
		var return_type = Types.UNIT;

		if (tokens[RETURN_TYPE].Type != TokenType.NONE)
		{
			return_type = new UnresolvedType(environment, tokens[RETURN_TYPE].To<IdentifierToken>().Value);
		}

		var function = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL,
			header.Name,
			return_type,
			parameters.ToArray()
		);

		function.Position = header.Position;

		function.Merge(function_context);
		environment.Declare(function);

		return null;
	}
}