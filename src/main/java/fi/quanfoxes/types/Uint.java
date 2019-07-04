package fi.quanfoxes.types;

import fi.quanfoxes.Lexer.NumberType;

public class Uint extends Number {
    public Uint() {
        super(NumberType.UINT32, 32, "uint");
    }
}
