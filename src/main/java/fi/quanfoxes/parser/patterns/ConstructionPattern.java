package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.ConstructionNode;
import fi.quanfoxes.parser.nodes.LinkNode;

public class ConstructionPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int NEW = 0;
    private static final int CONSTRUCTOR = 1;

    public ConstructionPattern() {
        // Pattern:
        // new Type() / new Type.Subtype()
        // Examples:
        // new Type(...)
        // new Type.Subtype(...)
        super(TokenType.KEYWORD, TokenType.FUNCTION | TokenType.PROCESSED);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(NEW);

        if (keyword.getKeyword() != Keywords.NEW) {
            return false;
        }

        Token token = tokens.get(CONSTRUCTOR);

        if (token.getType() == TokenType.PROCESSED) {
            ProcessedToken processed = (ProcessedToken)token;
            return processed.getNode() instanceof LinkNode;
        }

        return true;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(CONSTRUCTOR);
        Node constructor = Singleton.parse(context, token);
        
        return new ConstructionNode(constructor);
	}
}