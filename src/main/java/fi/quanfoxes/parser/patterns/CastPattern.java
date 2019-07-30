package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.CastNode;

public class CastPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int OBJECT = 0;
    private static final int CAST = 1;
    private static final int TYPE = 2;

    public CastPattern() {
        // Pattern:
        // ... -> Type / Type.Subtype
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.DYNAMIC, /* ... */
              TokenType.OPERATOR, /* -> */
              TokenType.IDENTIFIER | TokenType.DYNAMIC); /* Type / Type.Subtype */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        OperatorToken cast = (OperatorToken)tokens.get(CAST);
        return cast.getOperator() == Operators.CAST;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        Node object = Singleton.parse(context, tokens.get(OBJECT));
        Node type = Singleton.parse(context, tokens.get(TYPE));

        return new CastNode(object, type);
	}
}