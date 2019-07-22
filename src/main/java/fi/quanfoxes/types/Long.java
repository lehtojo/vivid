package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Long extends Number {
    public Long() {
        super(NumberType.INT64, 64, "long");
    }
}
