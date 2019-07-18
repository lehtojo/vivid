package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Tiny extends Number {
    public Tiny() {
        super(NumberType.INT8, 8, "tiny");
    }
}
