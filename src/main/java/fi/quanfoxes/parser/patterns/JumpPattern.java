package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.Errors;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.nodes.JumpNode;
import fi.quanfoxes.parser.Label;

public class JumpPattern extends Pattern {
    public static final int PRIORITY = 1;

    private static final int GOTO = 0;
    private static final int LABEL = 1;

    public JumpPattern() {
        super(TokenType.KEYWORD, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(GOTO);
        return keyword.getKeyword() == Keywords.GOTO;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        IdentifierToken name = (IdentifierToken)tokens.get(LABEL);
        
        if (!context.isLabelDeclared(name.getValue())) {
            throw Errors.get(name.getPosition(), String.format("Label '%s' doesn't exist in the current context", name.getValue()));
        }

        Label label = context.getLabel(name.getValue());
        return new JumpNode(label);
	}
}