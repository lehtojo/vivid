package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Ulong extends Number {
    private static final int BYTES = 8;

    public Ulong() {
        super(NumberType.UINT64, 64, "ulong");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
