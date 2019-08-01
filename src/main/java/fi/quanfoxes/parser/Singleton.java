package fi.quanfoxes.parser;

import java.util.List;

import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.FunctionToken;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.NumberToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.UnresolvedFunction;
import fi.quanfoxes.parser.nodes.UnresolvedIdentifier;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Singleton {
    /**
     * Tries to build identifier into a node
     * @param context Context to use for linking indentifier
     * @param id Identifier to link
     * @return Identifier built into a node
     */
    public static Node getIdentifier(Context context, IdentifierToken id) throws Exception {
        if (context.isVariableDeclared(id.getValue())) {
            return new VariableNode(context.getVariable(id.getValue()));
        }
        else if (context.isTypeDeclared(id.getValue())) {
            return new TypeNode(context.getType(id.getValue()));
        }
        else {
            return new UnresolvedIdentifier(id.getValue());
        }
    }

    /**
     * Tries to find function or constructor by name 
     * @param context Context to look for the function
     * @param name    Name of the function
     * @return Success: Function / Constructor, Failure: null
     */
    public static Function getFunctionByName(Context context, String name, List<Type> parameters) {
        Functions functions;

        if (context.isTypeDeclared(name)) {
            functions = context.getType(name).getConstructor();
        }
        else if (context.isFunctionDeclared(name)) {         
            functions = context.getFunction(name);
        }
        else {
            return null;
        }

        return functions.get(parameters);
    }

    /**
     * Tries to build function into a node
     * @param environment Context to parse function parameters
     * @param primary Context to find the function by name
     * @param info Function in token form
     * @return Function built into a node
     */
    public static Node getFunction(Context environment, Context primary, FunctionToken info) throws Exception {
        Node parameters = info.getParameters(environment);
        List<Type> types = Resolver.getTypes(parameters);

        Function function = getFunctionByName(primary, info.getName(), types);

        if (function != null) {
            return new FunctionNode(function).setParameters(parameters);
        }
        else {
            return new UnresolvedFunction(info.getName()).setParameters(parameters);
        }
    }

    /**
     * Tries to build number into a node
     * @param number Number in token form
     * @return Number built into a node
     */
    public static Node getNumber(NumberToken number) {
        return new NumberNode(number.getNumberType(), number.getNumber());
    }

    /**
     * Tries to build content into a node
     * @param context Context to parse content
     * @param content Content in token form
     * @return Content built into a node
     */
    public static Node getContent(Context context, ContentToken content) throws Exception {
        Node node = new ContentNode(); 

        for (int i = 0; i < content.getSectionCount(); i++) {
            List<Token> tokens = content.getTokens(i);
            Parser.parse(node, context, tokens);
        }

        return node;
    }

    public static Node parse(Context context, Token token) throws Exception {
        return Singleton.parse(context, context, token);
    }

    public static Node parse(Context environment, Context primary, Token token) throws Exception {
        switch(token.getType()) {
            case TokenType.IDENTIFIER:
                return getIdentifier(primary, (IdentifierToken)token);
            case TokenType.FUNCTION:
                return getFunction(environment, primary, (FunctionToken)token);
            case TokenType.NUMBER:
                return getNumber((NumberToken)token);
            case TokenType.CONTENT:
                return getContent(primary, (ContentToken)token);
            case TokenType.DYNAMIC:
                return ((DynamicToken)token).getNode();
        }

        return null;
    }

    public static Node getUnresolved(Context environment, Token token) throws Exception {
        switch(token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;
                return new UnresolvedIdentifier(id.getValue());
            case TokenType.FUNCTION:
                FunctionToken function = (FunctionToken)token;
                return new UnresolvedFunction(function.getName())
                                .setParameters(function.getParameters(environment));
        }
        
        throw new Exception(String.format("Couldn't create unresolved token (%d)", token.getType()));
    }
}