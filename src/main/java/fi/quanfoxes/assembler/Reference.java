package fi.quanfoxes.assembler;

public abstract class Reference {
    protected Size size;

    public Reference (Size size) {
        this.size = size;
    }

    public abstract String use();

    public String peek() {
        return use();
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

    public static RegisterReference from(Register register) {
        return new RegisterReference(register, Size.get(register.getSize()));
    }
}