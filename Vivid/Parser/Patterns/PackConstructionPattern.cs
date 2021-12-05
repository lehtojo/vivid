using System.Collections.Generic;
using System.Linq;

public class PackConstructionPattern : Pattern
{
	public const int PRIORITY = 19;

	public PackConstructionPattern() : base
	(
		TokenType.CONTENT
	) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens.First().To<ContentToken>().Type != ParenthesisType.CURLY_BRACKETS) return false;

		// The pack must have members
		if (tokens.First().To<ContentToken>().Tokens.Count == 0) return false;

		// Now, we must ensure this really is a pack construction.
		// The tokens must be in the form of: { $member-1 : $value-1, $member-2 : $value-2, ... }
		var sections = tokens.First().To<ContentToken>().GetSections();

		foreach (var section in sections.Select(i => i.Where(j => j.Type != TokenType.END).ToArray()))
		{
			// Empty sections do not matter, they can be ignored
			if (section.Length == 0) continue;

			// Verify the section begins with a member name, a colon and some token.
			if (section.Length < 3) return false;

			// The first token must be an identifier.
			if (section[0].Type != TokenType.IDENTIFIER) return false;

			// The second token must be a colon.
			if (!section[1].Is(Operators.COLON)) return false;
		}

		return true;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		// We know that this is a pack construction.
		// The tokens must be in the form of: { $member-1 : $value-1, $member-2 : $value-2, ... }
		var sections = tokens.First().To<ContentToken>().GetSections();

		var members = new List<string>();
		var arguments = new List<Node>();

		// Parse all the member values
		foreach (var section in sections.Select(i => i.Where(j => j.Type != TokenType.END).ToArray()))
		{
			// Empty sections do not matter, they can be ignored
			if (section.Length == 0) continue;

			var member = section[0].To<IdentifierToken>().Value;
			var value = Parser.Parse(context, section.Skip(2).ToList(), Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).First;

			// Ensure the member has a value
			if (value == null) throw Errors.Get(tokens.First().Position, "There must be a value after colon in pack construction");

			members.Add(member);
			arguments.Add(value);
		}

		return new PackConstructionNode(members, arguments, tokens.First().Position);
	}
}