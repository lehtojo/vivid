package fi.quanfoxes.types;

import fi.quanfoxes.Lexer.NumberType;

public class Ushort extends Number {
    public Ushort() {
        super(NumberType.UINT16, 16, "ushort");
    }
}
