package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Ushort extends Number {
    private static final int BYTES = 2;

    public Ushort() {
        super(NumberType.UINT16, 16, "ushort");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
