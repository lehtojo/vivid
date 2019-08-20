package fi.quanfoxes.types;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Type;

import java.util.Objects;

public abstract class Number extends Type {
    private NumberType type;
    private int bits;

    public Number(NumberType type, int bits, String name) {
        super(name, AccessModifier.PUBLIC);
        this.type = type;
        this.bits = bits;
    }

    public NumberType getNumberType() {
        return type;
    }

    public int getBitCount() {
        return bits;
    }

    public int getBytes() {
        return bits / 8;
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
