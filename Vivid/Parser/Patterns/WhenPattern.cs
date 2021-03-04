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
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Ensure the keyword is the when keyword
		if (!tokens.First().Is(Keywords.WHEN))
		{
			return false;
		}

		if (!tokens[VALUE].Is(ParenthesisType.PARENTHESIS))
		{
			return false;
		}

		return tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	/// <summary>
	/// Returns whether the section is an else section that is whether it should be executed when other conditions in when statement fail
	/// </summary>
	private bool IsElseSection(List<Token> section)
	{
		var i = section.FindIndex(i => i.Is(Operators.IMPLICATION));

		// Every section should have at least one implication token, but return false for now since the build function handles these issues
		if (i == -1)
		{
			return false;
		}

		// If there is an else keyword before the first implication token, it means this section is an else section
		return section.Take(i).Any(i => i.Is(Keywords.ELSE));
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var result = new InlineNode(tokens.First().Position);

		// Load the inspected value into a variable
		var source = Singleton.Parse(context, tokens[VALUE]);
		var type = source.TryGetType();

		var inspected_variable = context.DeclareHidden(type);

		var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(inspected_variable),
			source
		);

		result.Add(initialization);

		var result_variable = context.DeclareHidden(null);

		initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(result_variable),
			new NumberNode(Parser.Format, 0L)
		);

		// Initialize the result variable
		result.Add(initialization);

		// Determine all the sections of the body by splitting the tokens by commas
		var body = tokens[BODY].To<ContentToken>();
		var sections = body.GetSections();
		var is_first = true;

		var else_section = sections.FirstOrDefault(IsElseSection);

		if (else_section != null)
		{
			// Remove the else section since it is added last and it doesn't have a condition
			sections.Remove(else_section);

			// Remove all other else sections since they won't execute
			sections.RemoveAll(IsElseSection);
		}

		foreach (var section in sections)
		{
			if (!section.Exists(i => i.Is(Operators.IMPLICATION)))
			{
				throw Errors.Get(body.Position, "Could not understand the sections");
			}

			var section_tokens = section.TakeWhile(i => !i.Is(Operators.IMPLICATION)).ToList();
			var section_tokens_count = section_tokens.Count;
			var value = Parser.Parse(context, section_tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

			if (!value.Any())
			{
				throw Errors.Get(body.Position, "One of the sections has an empty value as condition");
			}

			var section_condition = new OperatorNode(Operators.EQUALS).SetOperands(
				new VariableNode(inspected_variable),
				value.Last!
			);

			section_tokens = section.Skip(section_tokens_count + 1).ToList();

			if (!section_tokens.Any())
			{
				throw Errors.Get(body.Position, "One of the sections has an empty body");
			}

			// If the first token represents a block, the body of the section is the tokens inside it
			if (section_tokens.First().Is(ParenthesisType.CURLY_BRACKETS))
			{
				section_tokens = section_tokens.First().To<ContentToken>().Tokens;
			}

			var section_body = Parser.Parse(context, section_tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

			if (!section_body.Any())
			{
				throw Errors.Get(body.Position, "One of the sections has an empty body");
			}

			// Create an assignment which stores the last value in the body into the result variable
			var result_assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(result_variable),
				section_body.Last!.Clone()
			);

			// Replace the last value with the assignment since the assignment has the last value
			section_body.Last!.Replace(result_assignment);

			var section_context = new Context(context);

			if (is_first)
			{
				result.Add(new IfNode(section_context, section_condition, section_body));

				is_first = false;
				continue;
			}

			result.Add(new ElseIfNode(section_context, section_condition, section_body));
		}

		// Finally, add the section which executes when other conditions fail, if one exists
		if (else_section != null)
		{
			var implication_index = else_section.FindIndex(i => i.Is(Operators.IMPLICATION));
			var section_tokens = else_section.Skip(implication_index + 1).ToList();

			if (!section_tokens.Any())
			{
				throw Errors.Get(body.Position, "Else section has an empty body");
			}

			// If the first token represents a block, the body of the section is the tokens inside it
			if (section_tokens.First().Is(ParenthesisType.CURLY_BRACKETS))
			{
				section_tokens = section_tokens.First().To<ContentToken>().Tokens;
			}

			var section_body = Parser.Parse(context, section_tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

			if (!section_body.Any())
			{
				throw Errors.Get(body.Position, "Else section has an empty body");
			}

			// Create an assignment which stores the last value in the body into the result variable
			var result_assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(result_variable),
				section_body.Last!.Clone()
			);

			// Replace the last value with the assignment since the assignment has the last value
			section_body.Last!.Replace(result_assignment);

			var section_context = new Context(context);

			result.Add(new ElseNode(section_context, section_body));
		}

		result.Add(new VariableNode(result_variable));

		return result;
	}
}
