package fi.quanfoxes.Lexer;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class FunctionToken extends Token {
    private String name;
    private List<ContentToken> parameters = new ArrayList<>();

    public FunctionToken(Lexer.TokenArea area) throws Exception {
        super(area.text, TokenType.FUNCTION);

        FunctionTokenAreaData data = (FunctionTokenAreaData)area.data;
        name = area.text.substring(0, data.contentStartIndex);

        int start = data.contentStartIndex + 1;
        int end = data.contentStartIndex + area.text.length() - 1;

        String content = area.text.substring(start, end);
        String[] parameters = content.split(",");

        for (String parameter : parameters) {
            this.parameters.add(new ContentToken(parameter));
        }
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
