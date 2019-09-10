package fi.quanfoxes.assembler.references;

import fi.quanfoxes.assembler.*;

public class NumberReference extends Reference {
    private Number number;

    public NumberReference(Number number, Size size) {
        super(size);
        this.number = number;
    }

    public Number getNumberValue() {
        return number;
    }

    @Override
    public String use() {
        return String.format("%s %s", size.getIdentifier(), number.toString());
    }

    public LocationType getType() {
        return LocationType.NUMBER;
    }
}