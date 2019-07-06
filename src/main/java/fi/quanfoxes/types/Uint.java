package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Uint extends Number {
    public Uint(Context context) throws Exception {
        super(context, NumberType.UINT32, 32, "uint");
    }
}
