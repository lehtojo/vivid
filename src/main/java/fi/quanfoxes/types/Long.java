package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;

public class Long extends Number {
    public Long(Context context) throws Exception {
        super(context, NumberType.INT64, 64, "long");
    }
}
