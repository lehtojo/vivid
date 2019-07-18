package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Ulong extends Number {
    public Ulong() {
        super(NumberType.UINT64, 64, "ulong");
    }
}
