package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.*;
import fi.quanfoxes.Parser.nodes.*;

import java.util.List;

public class DeclareLocalVariablePattern extends Pattern {
    private static final int PRIORITY = 17;

    private static final int TYPE = 0;
    private static final int IDENTIFIER = 1;

    public DeclareLocalVariablePattern() {
        // Pattern:
        // (Datatype / Processed datatype) (Name)
        // Examples:
        // long file_size
        // MyType awesome_type
        // MyType.MySubtype awesome_subtype
        super(TokenType.IDENTIFIER | TokenType.PROCESSED, TokenType.IDENTIFIER);
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

        return true;
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        ContextNode context = (ContextNode)parent;
        Token token = tokens.get(TYPE);

        TypeNode type;

        if (token.getType() == TokenType.PROCESSED) {
            ProcessedToken program = (ProcessedToken)token;
            type = (TypeNode)program.getNode();
        }
        else {
            IdentifierToken identifier = (IdentifierToken)token;
            type = context.getType(identifier.getIdentifier());

            if (type == null) {
                throw new Exception("Type is not recognized");
            }
        }

        IdentifierToken identifier = (IdentifierToken)tokens.get(IDENTIFIER);

        VariableNode variable = new VariableNode(identifier.getIdentifier(), type);
        context.declare(variable);

        return variable;
    }
}
