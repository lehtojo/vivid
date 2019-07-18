package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Normal extends Number {
    public Normal() {
        super(NumberType.INT32, 32, "num");
    }
}
