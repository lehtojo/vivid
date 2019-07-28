package fi.quanfoxes.lexer;

import java.util.List;
import java.util.Objects;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;

public class FunctionToken extends Token {
    private IdentifierToken name;
    private ContentToken parameters;

    /**
     * Creates a function token with name and parameters
     * @param name
     * @param parameters
     */
    public FunctionToken(IdentifierToken name, ContentToken parameters) {
        super(TokenType.FUNCTION);

        this.name = name;
        this.parameters = parameters;
    }

    /**
     * Returns the name of the function
     * @return Name of the function
     */
    public String getName() {
        return name.getValue();
    }

    /**
     * Parses the parameters with the given context
     * @param context Context used to parse parameters
     * @return Parameters in node tree form
     * @throws Exception Various reasons related to the parsing of the parameters
     */
    public Node getParameters(Context context) throws Exception {
        Node node = new Node();

        for (int i = 0; i < parameters.getSectionCount(); i++) {
            List<Token> tokens = parameters.getTokens(i);
            Parser.parse(node, context, tokens);
        }

        return node;
    }

    @Override
    public String getText() {
        return name.getText() + parameters.getText();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        if (!super.equals(o)) return false;
        FunctionToken that = (FunctionToken) o;
        return Objects.equals(name, that.name) &&
                Objects.equals(parameters, that.parameters);
    }

    @Override
    public int hashCode() {
        return Objects.hash(name, parameters);
    }
}
