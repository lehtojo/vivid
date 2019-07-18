package fi.quanfoxes.lexer;

import java.util.ArrayList;
import java.util.Objects;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.patterns.VariablePattern;

public class FunctionToken extends Token {
    private IdentifierToken name;
    private ContentToken parameters;

    public FunctionToken(IdentifierToken name, ContentToken parameters) {
        super(TokenType.FUNCTION);

        this.name = name;
        this.parameters = parameters;
    }

    public String getName() {
        return name.getValue();
    }

    public Node getParameters(Context context) throws Exception {
        Node node = new Node();

        for (int i = 0; i < parameters.getSectionCount(); i++) {
            ArrayList<Token> tokens = parameters.getTokens(i);
            Parser.parse(node, context, tokens, VariablePattern.PRIORITY);
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
