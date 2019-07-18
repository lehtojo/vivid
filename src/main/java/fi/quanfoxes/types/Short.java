package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Short extends Number {
    public Short() {
        super(NumberType.INT16, 16, "short");
    }
}
