using System.Collections.Generic;

public class IterationLoopPattern : Pattern
{
	public const int LOOP = 0;
	public const int ITERATOR = 1;
	public const int IN = 2;
	public const int ITERATED = 3;
	public const int BODY = 5;

	public const string ITERATOR_FUNCTION = "iterator";
	public const string NEXT_FUNCTION = "next";
	public const string VALUE_FUNCTION = "value";

	// Pattern: loop $name in $object [\n] {...}
	public IterationLoopPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.IDENTIFIER,
		TokenType.KEYWORD,
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.PARENTHESIS
	)
	{ Priority = 2; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[LOOP].Is(Keywords.LOOP) && tokens[IN].Is(Keywords.IN) && tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	private static Variable GetIterator(Context context, List<Token> tokens)
	{
		var identifier = tokens[ITERATOR].To<IdentifierToken>().Value;
		var iterator = context.Declare(null, VariableCategory.LOCAL, identifier);
		iterator.Position = tokens[ITERATOR].Position;
		return iterator;
	}

	public override Node? Build(Context environment, ParserState state, List<Token> tokens)
	{
		var iterator = environment.DeclareHidden(null);
		var iterator_position = tokens[ITERATOR].Position;

		iterator.Position = iterator_position;

		var iterated = Singleton.Parse(environment, tokens[ITERATED]);

		var initialization = new OperatorNode(Operators.ASSIGN, iterator_position).SetOperands(
			new VariableNode(iterator, iterator_position),
			new LinkNode(iterated, new UnresolvedFunction(ITERATOR_FUNCTION, iterator_position))
		);

		var condition = new LinkNode(new VariableNode(iterator, iterator_position), new UnresolvedFunction(NEXT_FUNCTION, iterator_position))
		{
			Position = tokens[IN].Position
		};

		var steps_context = new Context(environment);
		var body_context = new Context(steps_context);

		var value = GetIterator(body_context, tokens);

		var load = new OperatorNode(Operators.ASSIGN, iterator_position).SetOperands(
			new VariableNode(value, iterator_position),
			new LinkNode(
				new VariableNode(iterator, iterator_position),
				new UnresolvedFunction(VALUE_FUNCTION, iterator_position)
			)
		);

		var steps = new Node { new Node() { initialization }, new Node { condition }, new Node() };

		var token = tokens[BODY].To<ParenthesisToken>();
		var body = new ScopeNode(body_context, token.Position, token.End, false) { load };

		Parser.Parse(body_context, token.Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).ForEach(i => body.Add(i));

		return new LoopNode(steps_context, steps, body, tokens[LOOP].Position);
	}
}