package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Ushort extends Number {
    public Ushort() {
        super(NumberType.UINT16, 16, "ushort");
    }
}
