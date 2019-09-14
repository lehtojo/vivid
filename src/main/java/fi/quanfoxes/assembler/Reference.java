package fi.quanfoxes.assembler;

import fi.quanfoxes.assembler.references.*;

public abstract class Reference {
    protected Size size;

    public Reference (Size size) {
        this.size = size;
    }

    public abstract String use(Size size);
    public abstract LocationType getType();

    public String peek(Size size) {
        return use(size);
    }

    public boolean isComplex() {
        return false;
    }

    public boolean isRegister() {
        return false;
    }

    public Register getRegister() {
        return null;
    }

    public Size getSize() {
        return size;
    }
}