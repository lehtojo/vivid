package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.Keyword;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.*;
import fi.quanfoxes.parser.nodes.*;

import java.util.List;

public class VariablePattern extends Pattern {
    public static final int PRIORITY = 17;

    private static final int TYPE = 0;
    private static final int NAME = 1;

    public VariablePattern() {
        // Pattern:
        // (Datatype / Processed datatype) (Name)
        // Examples:
        // long file_size
        // MyType awesome_type
        // MyType.MySubtype awesome_subtype
        // var awesome
        super(TokenType.IDENTIFIER | TokenType.KEYWORD | TokenType.PROCESSED, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.PROCESSED) {
            ProcessedToken program = (ProcessedToken)token;
            return program.getNode() instanceof TypeNode;
        }
        else if (token.getType() == TokenType.KEYWORD) {
            Keyword keyword = ((KeywordToken)token).getKeyword();
            return keyword == Keywords.VAR;
        }

        return true;
    }

    private Type getType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.PROCESSED) {
            ProcessedToken processed = (ProcessedToken)token;
            TypeNode type = (TypeNode)processed.getNode();

            return type.getType();
        }
        else if (token.getType() == TokenType.KEYWORD) {
            return Types.UNKNOWN;
        }
        else {
            IdentifierToken identifier = (IdentifierToken)token;
            return context.getType(identifier.getValue());
        }
    }

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getValue();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        Type type = getType(context, tokens);
        String name = getName(tokens);

        Variable variable = new Variable(context, type, name, AccessModifier.PUBLIC);
       
        return new VariableNode(variable);
    }
}
