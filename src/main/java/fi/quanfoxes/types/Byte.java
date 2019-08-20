package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Byte extends Number {
    private static final int BYTES = 1;

    public Byte() {
        super(NumberType.UINT8, 8, "byte");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
