package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Byte extends Number {
    public Byte(Context context) throws Exception {
        super(context, NumberType.UINT8, 8, "byte");
    }
}
