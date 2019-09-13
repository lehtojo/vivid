package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.*;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.UnresolvedType;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.util.ArrayList;
import java.util.List;

public class ExtendedTypePattern extends Pattern {
    public static final int PRIORITY = 21;

    private static final int MODIFIER = 0;
    private static final int TYPE = 1;
    private static final int NAME = 2;
    private static final int EXTENDER = 3;
    private static final int SUPERTYPES = 4;
    private static final int BODY = 6;

    public ExtendedTypePattern() {
        // [private / protected / public] type ... : ... / (...) [\n] {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
              TokenType.KEYWORD, /* type */
              TokenType.IDENTIFIER, /* ... */
              TokenType.OPERATOR, /* : */
              TokenType.IDENTIFIER | TokenType.CONTENT, /* ... / (...) */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.CONTENT); /* {..} */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken modifier = (KeywordToken)tokens.get(MODIFIER);

        if (modifier != null && modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        KeywordToken type = (KeywordToken)tokens.get(TYPE);
        
        if (type.getKeyword() != Keywords.TYPE) {
            return false;
        }

        OperatorToken extender = (OperatorToken)tokens.get(EXTENDER);

        if (extender.getOperator() != Operators.EXTENDER) {
            return false;
        }

        Token token = tokens.get(SUPERTYPES);

        if (token instanceof ContentToken) {
            ContentToken supertypes = (ContentToken)token;

            if (supertypes.getParenthesisType() != ParenthesisType.BRACKETS) {
                return false;
            }
        }

        ContentToken body = (ContentToken)tokens.get(BODY);
        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getValue();
    }

    private List<Token> getBody(List<Token> tokens) {
        return ((ContentToken)tokens.get(BODY)).getTokens();
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

    private Type getType(Context environment, Node node) throws Exception {
        if (node instanceof Contextable) {
            Contextable contextable = (Contextable)node;
            Context context = contextable.getContext();

            return (Type)context;
        }
        else if (node instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)node;
            return new UnresolvedType(environment, resolvable);
        }

        return null;
    }

    private List<Type> getSupertypes(Context environment, List<Token> tokens) throws Exception {
        List<Type> types = new ArrayList<>();
        Node node = Singleton.parse(environment, tokens.get(SUPERTYPES));

        if (node instanceof ContentNode) {
            Node iterator = node.first();

            while (iterator != null) {
                Type type = getType(environment, iterator);
                types.add(type);

                iterator = iterator.next();
            }
        }
        else {
            Type type = getType(environment, node);
            types.add(type);
        }

        return types;
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = getModifiers(tokens);

        // Get type name and its body
        String name = getName(tokens);
        List<Token> body = getBody(tokens);

        // Get all supertypes declared for this type
        List<Type> supertypes = getSupertypes(context, tokens);

        // Create this type and parse its possible subtypes
        Type type = new Type(context, name, modifiers, supertypes);
        Parser.parse(type, body, PRIORITY);

        return new TypeNode(type, body);
    }
} 