package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Short extends Number {
    private static final int BYTES = 2;

    public Short() {
        super(NumberType.INT16, 16, "short");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
