package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Tiny extends Number {
    private static final int BYTES = 1;
    
    public Tiny() {
        super(NumberType.INT8, 8, "tiny");
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}
