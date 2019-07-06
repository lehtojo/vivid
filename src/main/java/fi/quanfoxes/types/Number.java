package fi.quanfoxes.types;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.Context;

import java.util.Objects;

public abstract class Number extends Type {
    private NumberType type;
    private int bits;

    public Number(Context context, NumberType type, int bits, String name) throws Exception {
        super(context, name);
        this.type = type;
        this.bits = bits;
    }

    public NumberType getNumberType() {
        return type;
    }

    public int getBitCount() {
        return bits;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        if (!super.equals(o)) return false;
        Number number = (Number) o;
        return bits == number.bits &&
                type == number.type;
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), type, bits);
    }
}
