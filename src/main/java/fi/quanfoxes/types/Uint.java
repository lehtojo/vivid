package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Uint extends Number {
    public Uint() {
        super(NumberType.UINT32, 32, "uint");
    }
}
