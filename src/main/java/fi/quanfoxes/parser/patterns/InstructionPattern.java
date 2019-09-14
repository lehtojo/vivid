package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.nodes.InstructionNode;

public class InstructionPattern extends Pattern {
    public static final int PRIORITY = 1;

    private static final int INSTRUCTION = 0;

    public InstructionPattern() {
        // Pattern:
        // continue, stop
        super(TokenType.KEYWORD);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(INSTRUCTION);
        return keyword.getKeyword() == Keywords.CONTINUE || keyword.getKeyword() == Keywords.STOP;
    }

    private Keyword getInstruction(List<Token> tokens) {
        KeywordToken instruction = (KeywordToken)tokens.get(INSTRUCTION);
        return instruction.getKeyword();
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
		return new InstructionNode(getInstruction(tokens));
	}
}