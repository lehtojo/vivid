package fi.quanfoxes.Lexer;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;

public class FunctionToken extends Token {
    private String name;
    private List<ContentToken> parameters = new ArrayList<>();

    public FunctionToken(Lexer.TokenArea area) throws Exception {
        super(area.text, TokenType.FUNCTION);

        FunctionTokenAreaData data = (FunctionTokenAreaData)area.data;
        name = area.text.substring(0, data.contentStartIndex);

        String content = area.text.substring(data.contentStartIndex, area.text.length());
        String[] parameters = content.split(",");

        for (String parameter : parameters) {
            this.parameters.add(new ContentToken(parameter));
        }
    }

    public FunctionToken(String full, String name, ContentToken... parameters) {
        super(full, TokenType.FUNCTION);
        this.name = name;
        this.parameters = Arrays.asList(parameters);
    }

    public String getName() {
        return name;
    }

    public List<ContentToken> getParameters() {
        return parameters;
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
