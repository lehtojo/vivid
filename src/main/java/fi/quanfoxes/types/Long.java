package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Long extends Number {
    public Long() {
        super(NumberType.INT64, 64, "long");
    }
}
