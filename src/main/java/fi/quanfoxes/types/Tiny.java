package fi.quanfoxes.types;

import fi.quanfoxes.Lexer.NumberType;

public class Tiny extends Number {
    public Tiny() {
        super(NumberType.INT8, 8, "tiny");
    }
}
