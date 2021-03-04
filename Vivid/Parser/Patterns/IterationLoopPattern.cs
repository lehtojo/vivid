using System.Collections.Generic;

public class IterationLoopPattern : Pattern
{
	public const int PRIORITY = 2;

	public const int LOOP = 0;
	public const int ITERATOR = 1;
	public const int IN = 2;
	public const int ITERATED = 3;
	public const int BODY = 5;

	public const string ITERATOR_FUNCTION = "iterator";
	public const string NEXT_FUNCTION = "next";
	public const string VALUE_FUNCTION = "value";

	// Pattern: loop $i in $object [\n] {...}
	public IterationLoopPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.IDENTIFIER,
		TokenType.KEYWORD,
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.CONTENT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[LOOP].Is(Keywords.LOOP) && tokens[IN].Is(Keywords.IN) && tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	private static Variable GetIterator(Context context, List<Token> tokens)
	{
		var identifier = tokens[ITERATOR].To<IdentifierToken>().Value;

		if (context.IsLocalVariableDeclared(identifier))
		{
			return context.GetVariable(identifier)!;
		}

		var iterator = context.Declare(Types.UNKNOWN, VariableCategory.LOCAL, identifier);
		iterator.Position = tokens[ITERATOR].Position;

		return iterator;
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		var iterator = environment.DeclareHidden(Types.UNKNOWN);
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

		var value = GetIterator(environment, tokens);

		var load = new OperatorNode(Operators.ASSIGN, iterator_position).SetOperands(
			new VariableNode(value, iterator_position),
			new LinkNode(
				new VariableNode(iterator, iterator_position),
				new UnresolvedFunction(VALUE_FUNCTION, iterator_position)
			)
		);

		var context = new Context(environment);
		var steps = new Node { new Node() { initialization }, new Node { condition }, new Node() };

		var body = new ContextNode(context) { load };
		Parser.Parse(context, tokens[BODY].To<ContentToken>().Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).ForEach(n => body.Add(n));

		return new LoopNode(context, steps, body, tokens[LOOP].Position);
	}
}