package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Byte extends Number {
    public Byte() {
        super(NumberType.UINT8, 8, "byte");
    }
}
