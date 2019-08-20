package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Long extends Number {
    private static final int BYTES = 8;

    public Long() {
        super(NumberType.INT64, 64, "long");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
