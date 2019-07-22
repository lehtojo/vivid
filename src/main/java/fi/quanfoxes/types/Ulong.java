package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;

public class Ulong extends Number {
    public Ulong() {
        super(NumberType.UINT64, 64, "ulong");
    }
}
