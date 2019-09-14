package fi.quanfoxes.assembler.references;

import fi.quanfoxes.assembler.*;

public class RegisterReference extends Reference {
    private Register register;

    public RegisterReference(Register register, Size size) {
        super(size);
        this.register = register;
    }

    public RegisterReference(Register register) {
        super(Size.DWORD);
        this.register = register;
    }

    @Override
    public boolean isRegister() {
        return true;
    }

    @Override
    public Register getRegister() {
        return register;
    }

    @Override
    public String use(Size size) {
        return register.getPartition(size);
    }

    public LocationType getType() {
        return LocationType.REGISTER;
    }
}