package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Normal extends Number {
    public Normal() {
        super(NumberType.INT32, 32, "num");
    }
}
