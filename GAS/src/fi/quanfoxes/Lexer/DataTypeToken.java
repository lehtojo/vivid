package fi.quanfoxes.Lexer;

import fi.quanfoxes.DataType;
import fi.quanfoxes.DataTypeDatabase;

import java.util.Objects;

public class DataTypeToken extends Token {
    private DataType dataType;

    public DataTypeToken(Lexer.TokenArea area) {
        super(TokenType.DATA_TYPE);
        dataType = DataTypeDatabase.get(area.text);
    }

    public DataTypeToken(String name) {
        super(TokenType.DATA_TYPE);
        this.dataType = DataTypeDatabase.get(name);
    }

    public DataType getDataType() {
        return dataType;
    }

    @Override
    public String getText() {
        return dataType.getName();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof DataTypeToken)) return false;
        if (!super.equals(o)) return false;
        DataTypeToken that = (DataTypeToken) o;
        return Objects.equals(dataType, that.dataType);
    }

    @Override
    public int hashCode() {
        return Objects.hash(dataType);
    }
}
