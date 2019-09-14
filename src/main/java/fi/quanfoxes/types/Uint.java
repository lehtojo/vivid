package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Uint extends Number {
    private static final int BYTES = 4;

    public Uint() {
        super(NumberType.UINT32, 32, "uint");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
