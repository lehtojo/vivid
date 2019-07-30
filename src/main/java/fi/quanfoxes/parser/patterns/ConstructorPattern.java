package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.Errors;
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
    public static final int PRIORITY = 20;

    private static final int MODIFIER = 0;
    private static final int INIT = 1;
    private static final int PARAMETERS = 2;

    private static final int BODY = 4;

    public ConstructorPattern() {
        // Pattern:
        // [private / protected / public] init (...) [\n] {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
              TokenType.KEYWORD, /* init */
              TokenType.CONTENT | TokenType.OPTIONAL, /* (...) */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.CONTENT); /*  {...} */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    private KeywordToken getInitializeKeyword(List<Token> tokens) {
        return (KeywordToken)tokens.get(INIT);
    }

    private ContentToken getParameters(List<Token> tokens) {
        return (ContentToken)tokens.get(PARAMETERS);
    }

    private ContentToken getBody(List<Token> tokens) {
        return (ContentToken)tokens.get(BODY);
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken modifier = (KeywordToken)tokens.get(MODIFIER);

        if (modifier != null && modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        KeywordToken init = getInitializeKeyword(tokens);

        if (init.getKeyword() != Keywords.INIT) {
            return false;
        }

        ContentToken parameters = getParameters(tokens);

        if (parameters != null && parameters.getParenthesisType() != ParenthesisType.PARENTHESIS) {
            return false;
        }

        ContentToken body = getBody(tokens);

        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

    private int getModifiers(List<Token> tokens) {
        int modifiers = AccessModifier.PUBLIC;

        KeywordToken token = (KeywordToken)tokens.get(MODIFIER);

        if (token != null) {
            AccessModifierKeyword modifier = (AccessModifierKeyword)token.getKeyword();
            modifiers = modifier.getModifier();
        }

        return modifiers;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = getModifiers(tokens);
        ContentToken parameters = getParameters(tokens);
        List<Token> body = getBody(tokens).getTokens();

        if (!context.isType()) {
            throw Errors.get(tokens.get(0).getPosition(), "Constructor must be inside of a type");
        }
        
        Type type = (Type)context;

        Function constructor = new Function(context, modifiers);

        if (parameters != null) {
            constructor.setParameters(Singleton.getContent(context, parameters));
        }

        type.addConstructor(constructor);

        return new FunctionNode(constructor, body);
	}
}