package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Tiny extends Number {
    public Tiny(Context context) throws Exception {
        super(context, NumberType.INT8, 8, "tiny");
    }
}
