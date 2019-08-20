package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.FunctionToken;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.UnresolvedType;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.Function;

public class ExternalFunctionPattern extends Pattern {
    private static final int PRIORITY = 20;

    private static final int IMPORT = 0;
    private static final int TYPE = 1;
    private static final int HEAD = 2;
    
    public ExternalFunctionPattern() {
        super(TokenType.KEYWORD, /* import */
              TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.DYNAMIC, /* func / Type / Type.Subtype */
              TokenType.FUNCTION | TokenType.IDENTIFIER, /* name (...) */
              TokenType.END); /* \n */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(IMPORT);

        if (keyword.getKeyword() != Keywords.IMPORT) {
            return false;
        }

        Token token = tokens.get(TYPE);

        switch (token.getType()) {
            case TokenType.KEYWORD:
                return ((KeywordToken)token).getKeyword() == Keywords.FUNC;
            case TokenType.IDENTIFIER:
                return true;             
            case TokenType.DYNAMIC:
                Node node = ((DynamicToken)token).getNode();
                return (node instanceof LinkNode) || (node instanceof TypeNode);
        }

        return false;
    }

    private Type getReturnType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(TYPE);

        switch (token.getType()) {

            case TokenType.KEYWORD:
                return Types.UNKNOWN;

            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;
                
                if (context.isTypeDeclared(id.getValue())) {
                    return context.getType(id.getValue());
                }
                else {
                    return new UnresolvedType(context, id.getValue());
                }

             case TokenType.DYNAMIC:
                DynamicToken dynamic = (DynamicToken)token;
                Node node = dynamic.getNode();

                if (node instanceof TypeNode) {
                    TypeNode type = (TypeNode)node;
                    return type.getType();
                }
                else if (node instanceof Resolvable) {
                    return new UnresolvedType(context, (Resolvable)node);
                }
        }

        throw new Exception("INTERNAL_ERROR");
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {  
        Function function;

        Type result = getReturnType(context, tokens);    
        Token token = tokens.get(HEAD);

        if (token instanceof FunctionToken) {
            FunctionToken head = (FunctionToken)token;
            function = new Function(context, head.getName(), AccessModifier.EXTERNAL, result);
            function.setParameters(head.getParameters(function));
        }
        else {
            IdentifierToken name = (IdentifierToken)token;
            function = new Function(context, name.getValue(), AccessModifier.EXTERNAL, result);
        }

        return new FunctionNode(function);
    }
}