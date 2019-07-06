package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Ushort extends Number {
    public Ushort(Context context) throws Exception {
        super(context, NumberType.UINT16, 16, "ushort");
    }
}
