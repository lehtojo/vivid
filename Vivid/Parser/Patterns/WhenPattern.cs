using System.Collections.Generic;
using System.Linq;

public class WhenPattern : Pattern
{
	public const int PRIORITY = 19;

	public const int VALUE = 1;
	public const int BODY = 3;

	// Pattern: when(...) [\n] {...}
	public WhenPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.CONTENT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Ensure the keyword is the when keyword
		if (!tokens.First().Is(Keywords.WHEN)) return false;
		if (!tokens[VALUE].Is(ParenthesisType.PARENTHESIS)) return false;

		return tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context environment, PatternState state, List<Token> all)
	{
		var position = all.First().Position;

		// Load the inspected value into a variable
		var inspected_value = Singleton.Parse(environment, all[VALUE]);
		var inspected_value_variable = environment.DeclareHidden(inspected_value.TryGetType());
		
		var load = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(inspected_value_variable, position),
			inspected_value
		);

		var tokens = all[BODY].To<ContentToken>().Tokens;

		Parser.RemoveLineEndingDuplications(tokens);
		Parser.CreateFunctionTokens(tokens);

		if (!tokens.Any()) throw Errors.Get(position, "When-statement can not be empty");

		const int IF_STATEMENT = 0;
		const int ELSE_IF_STATEMENT = 1;
		const int ELSE_STATEMENT = 2;

		var sections = new List<Node>();
		var type = IF_STATEMENT;

		while (tokens.Any())
		{
			// Remove all line-endings from the start
			tokens.RemoveRange(0, tokens.TakeWhile(i => i.Is(TokenType.END) || i.Is(Operators.COMMA)).Count());
			if (!tokens.Any()) break;

			// Find the heavy arrow operator, which marks the start of the executable body, every section must have one
			var index = tokens.FindIndex(i => i.Is(Operators.HEAVY_ARROW));
			if (index < 0) throw Errors.Get(tokens.First().Position, "All sections in when-statements must have a heavy arrow operator");
			if (index == 0) throw Errors.Get(tokens.First().Position, "Section condition can not be empty");

			var arrow = tokens[index];

			// Take out the section condition
			var condition_tokens = tokens.GetRange(0, index);
			var condition = (Node?)null;

			tokens.RemoveRange(0, index + 1);

			if (!condition_tokens.First().Is(Keywords.ELSE))
			{	
				// Insert an equals-operator to the condition, if it does start with a keyword or an operator
				if (!condition_tokens.First().Is(TokenType.KEYWORD, TokenType.OPERATOR))
				{
					condition_tokens.Insert(0, new OperatorToken(Operators.EQUALS, condition_tokens.First().Position));
				}

				condition_tokens.Insert(0, new IdentifierToken(inspected_value_variable.Name, position));
				condition = Parser.Parse(environment, condition_tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
			}
			else
			{
				type = ELSE_STATEMENT;
			}

			if (!tokens.Any()) throw Errors.Get(position, "Missing section body");

			var context = new Context(environment);
			var body = (Node?)null;

			if (tokens.First().Is(ParenthesisType.CURLY_BRACKETS))
			{
				var parenthesis = tokens.Pop()!.To<ContentToken>();
				body = Parser.Parse(context, parenthesis.Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
			}
			else
			{
				// Consume the section body
				state = new PatternState(tokens);
				var result = new List<Token>();
				if (!Common.ConsumeBlock(context, state, result)) throw Errors.Get(arrow.Position, "Could not understand the section body");

				body = Parser.Parse(context, result, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

				// Remove the consumed tokens
				tokens.RemoveRange(0, state.End);
			}

			// Finish the when-statement, when an else-section is encountered
			if (type == ELSE_STATEMENT)
			{
				sections.Add(new ElseNode(context, body, arrow.Position, null));
				break;
			}

			// If the section is not an else-section, the condition must be present
			if (condition == null) throw Errors.Get(arrow.Position, "Missing section condition");
			
			// Add the conditional section
			if (type == IF_STATEMENT)
			{
				sections.Add(new IfNode(context, condition, body, arrow.Position, null));
				type = ELSE_IF_STATEMENT;
			}
			else
			{
				sections.Add(new ElseIfNode(context, condition, body, arrow.Position, null));
			}
		}

		return new InlineNode(position) { load, new WhenNode(new VariableNode(inspected_value_variable, position), sections, position) };
	}
}
