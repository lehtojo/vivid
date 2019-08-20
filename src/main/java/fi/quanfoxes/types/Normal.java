package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Normal extends Number {
    private static final int BYTES = 4;

    public Normal() {
        super(NumberType.INT32, 32, "num");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
