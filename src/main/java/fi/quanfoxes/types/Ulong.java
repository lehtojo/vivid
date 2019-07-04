package fi.quanfoxes.types;

import fi.quanfoxes.Lexer.NumberType;

public class Ulong extends Number {
    public Ulong() {
        super(NumberType.UINT64, 64, "ulong");
    }
}
