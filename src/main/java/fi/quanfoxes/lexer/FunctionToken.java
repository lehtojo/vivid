package fi.quanfoxes.lexer;

import java.util.Objects;

public class FunctionToken extends Token {
    private IdentifierToken name;
    private ContentToken parameters;

    public FunctionToken(IdentifierToken name, ContentToken parameters) {
        super(TokenType.FUNCTION);

        this.name = name;
        this.parameters = parameters;
    }

    public String getName() {
        return name.getIdentifier();
    }

    public ContentToken getParameters() {
        return parameters;
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
