package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.Keyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Lexer.IdentifierToken;
import fi.quanfoxes.Lexer.KeywordToken;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.ProcessedToken;
import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.nodes.TypeNode;
import fi.quanfoxes.Parser.nodes.VariableNode;

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

    private TypeNode getProcessedType(Token token) {
        return (TypeNode)((ProcessedToken)token).getNode();
    }

    private TypeNode getTypeByIdentifier(Node parent, Token token) {
        return ((ContextNode)parent).getType(((IdentifierToken)token).getIdentifier());
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        AccessModifierKeyword modifier = getAccessModifier(tokens);
        Token token = tokens.get(TYPE);

        TypeNode type;

        if (token.getType() == TokenType.PROCESSED) {
            type = getProcessedType(token);
        }
        else {
            type = getTypeByIdentifier(parent, token);
        }

        IdentifierToken identifier = (IdentifierToken)tokens.get(NAME);

        ContextNode scope = (ContextNode)parent;
        VariableNode variable = new VariableNode(identifier.getIdentifier(), type);
        scope.declare(variable);

        return variable;
    }
}
