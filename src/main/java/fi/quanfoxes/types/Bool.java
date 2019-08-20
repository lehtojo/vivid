package fi.quanfoxes.types;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.parser.Type;

public class Bool extends Type {
    private static final int BYTES = 1;

    public Bool() {
        super("bool", AccessModifier.PUBLIC);
    }

    @Override
    public int getSize() {
        return BYTES;
    }
}