using System.Collections.Generic;

public class InstructionPattern : Pattern
{
	public const int PRIORITY = 1;

	private const int INSTRUCTION = 0;

	// Pattern:
	// continue, stop
	public InstructionPattern() : base(TokenType.KEYWORD) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[INSTRUCTION];
		return keyword.Keyword == Keywords.CONTINUE || keyword.Keyword == Keywords.STOP;
	}

	private Keyword GetInstruction(List<Token> tokens)
	{
		KeywordToken instruction = (KeywordToken)tokens[INSTRUCTION];
		return instruction.Keyword;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return new InstructionNode(GetInstruction(tokens));
	}
}