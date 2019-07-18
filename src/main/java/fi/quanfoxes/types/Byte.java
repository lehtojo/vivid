package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Byte extends Number {
    public Byte() {
        super(NumberType.UINT8, 8, "byte");
    }
}
