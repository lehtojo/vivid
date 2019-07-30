package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;

public class SingletonPattern extends Pattern {
    public static final int PRIORITY = 1;

    public SingletonPattern() {
        // Pattern:
        // Identifier / Function
        super(TokenType.IDENTIFIER | TokenType.FUNCTION | TokenType.NUMBER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        return true;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
		return Singleton.parse(context, tokens.get(0));
	}
}