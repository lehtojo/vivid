package fi.quanfoxes.assembler.references;

import fi.quanfoxes.assembler.*;

public class AddressReference extends Reference {
    private Number number;

    public AddressReference(Number number) {
        super(Size.DWORD);
        this.number = number;
    }

    @Override
    public String use(Size size) {
        return String.format("%s [%d]", size, number.longValue());
    }

    @Override
    public boolean isComplex() {
        return true;
    }

    public LocationType getType() {
        return LocationType.ADDRESS;
    }
}