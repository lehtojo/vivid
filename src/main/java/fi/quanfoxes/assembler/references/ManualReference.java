package fi.quanfoxes.assembler.references;

import fi.quanfoxes.assembler.Reference;
import fi.quanfoxes.assembler.Size;

public class ManualReference extends Reference {
    private String identifier;
    private boolean point;

    public ManualReference(String identifier, Size size, boolean point) {
        super(size);
        this.identifier = identifier;
        this.point = point;
    }

    @Override
    public String use() {
        return point ? String.format("[%s]", identifier) : identifier;
    }

    @Override
    public boolean isComplex() {
        return point;
    }

    public static ManualReference string(String name, boolean write) {
        return new ManualReference(name, Size.DWORD, write);
    }

    public static ManualReference label(String name) {
        return new ManualReference(name, Size.DWORD, false);
    }

    public static ManualReference global(String name, Size size) {
        return new ManualReference(name, size, true);
    }

    public static ManualReference number(Number value, Size size) {
        return new ManualReference(value.toString(), size, false);
    }

    public LocationType getType() {
        return LocationType.MANUAL;
    }
}