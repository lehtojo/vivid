package fi.quanfoxes.types;

import fi.quanfoxes.Lexer.NumberType;

public class Short extends Number {
    public Short() {
        super(NumberType.INT16, 16, "short");
    }
}
