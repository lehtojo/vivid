package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.ParenthesisType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.FunctionNode;

public class ConstructorPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int MODIFIED_CONSTRUCTOR_LENGTH = 4;

    private static final int MODIFIER = 0;

    private static final int INIT = 0;
    private static final int PARAMETERS = 1;
    private static final int BODY = 2;

    public ConstructorPattern() {
        // Pattern:
        // [private / protected / public] init (...) {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, TokenType.KEYWORD, TokenType.CONTENT, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    private AccessModifierKeyword getAccessModifier(List<Token> tokens) {
        return (AccessModifierKeyword)((KeywordToken)tokens.get(MODIFIER)).getKeyword();
    }

    private KeywordToken getInitializeKeyword(List<Token> tokens, int start) {
        return (KeywordToken)tokens.get(start + INIT);
    }

    private ContentToken getParameters(List<Token> tokens, int start) {
        return (ContentToken)tokens.get(start + PARAMETERS);
    }

    private ContentToken getBody(List<Token> tokens, int start) {
        return (ContentToken)tokens.get(start + BODY);
    }

    @Override
    public boolean passes(List<Token> tokens) {
        int start = 0;

        if (tokens.size() == MODIFIED_CONSTRUCTOR_LENGTH) {
            KeywordToken modifier = (KeywordToken)tokens.get(MODIFIER);

            if (modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
                return false;
            }

            start = 1;
        }

        KeywordToken init = getInitializeKeyword(tokens, start);

        if (init.getKeyword() != Keywords.INIT) {
            return false;
        }

        ContentToken parameters = getParameters(tokens, start);

        if (parameters.getParenthesisType() != ParenthesisType.PARENTHESIS) {
            return false;
        }

        ContentToken body = getBody(tokens, start);

        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = AccessModifier.PUBLIC;
        int start = 0;

        if (tokens.size() == MODIFIED_CONSTRUCTOR_LENGTH) {
            modifiers = getAccessModifier(tokens).getModifier();
            start = 1;
        }

        ContentToken parameters = getParameters(tokens, start);
        List<Token> body = getBody(tokens, start).getTokens();

        Type type = context.getTypeParent();

        Function constructor = new Function(context, modifiers);
        constructor.setParameters(Singleton.getContent(context, parameters));

        type.addConstructor(constructor);

        return new FunctionNode(constructor, body);
	}
}