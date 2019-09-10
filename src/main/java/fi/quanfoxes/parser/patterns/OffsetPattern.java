package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.lexer.ParenthesisType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.OperatorNode;

public class OffsetPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int OBJECT = 0;
    private static final int INDEX = 1; 

    public OffsetPattern() {
        // Function / Variable / (...) [Function / Variable / Number / (...)]
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.DYNAMIC, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        ContentToken index = (ContentToken)tokens.get(INDEX);

        if (index.getParenthesisType() != ParenthesisType.BRACKETS) {
            return false;
        }

        return !index.isEmpty();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        Node object = Singleton.parse(context, tokens.get(OBJECT));
        Node index = Singleton.parse(context, tokens.get(INDEX));

        return new OperatorNode(Operators.EXTENDER).setOperands(object, index);
    }
}