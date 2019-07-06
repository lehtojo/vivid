package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.Keyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

import java.util.List;

public class MemberVariablePattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int MODIFIER = 0;
    private static final int TYPE = 1;
    private static final int NAME = 2;

    public MemberVariablePattern() {
        // [private / protected / public] (Datatype / Processed datatype) (Name)
        // Examples:
        // private tiny group_size
        // protected MyType my_type
        // public MyType.MySubtype my_subtype
        // num number
        super(TokenType.KEYWORD, TokenType.IDENTIFIER | TokenType.PROCESSED, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        Keyword keyword = ((KeywordToken)tokens.get(MODIFIER)).getKeyword();

        if (keyword.getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.PROCESSED) {
            Node node = ((ProcessedToken)token).getNode();
            return (node instanceof TypeNode);
        }

        return true;
    }

    private AccessModifierKeyword getAccessModifier(List<Token> tokens) {
        return (AccessModifierKeyword)((KeywordToken)tokens.get(MODIFIER)).getKeyword();
    }

    private Type getType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.PROCESSED) {
            ProcessedToken processed = (ProcessedToken)token;
            TypeNode type = (TypeNode)processed.getNode();

            return type.getType();
        }
        else {
            IdentifierToken identifier = (IdentifierToken)token;
            return context.getType(identifier.getIdentifier());
        }
    }

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getIdentifier();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        AccessModifierKeyword modifier = getAccessModifier(tokens);
        Type type = getType(context, tokens);
        String name = getName(tokens);

        Variable variable = new Variable(context, type, name, modifier.getModifier());

        return new VariableNode(variable);
    }
}
