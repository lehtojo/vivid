package fi.quanfoxes.assembler.references;

import fi.quanfoxes.assembler.*;

public class MemoryReference extends Reference {
    private Register register;
    private int alignment;

    public MemoryReference(Register register, int alignment, int size) {
        super(Size.get(size));

        this.register = register;
        this.alignment = alignment;
    }

    public String getContent() {
        if (alignment > 0) {
            return String.format("%s+%d", register, alignment);
        }
        else if (alignment < 0) {
            return String.format("%s%d", register, alignment);
        }
        else {
            return String.format("%s", register);
        }
    }

    @Override
    public String use() {
        return String.format("[%s]", getContent());
    }

    @Override
    public boolean isComplex() {
        return true;
    }

    public static MemoryReference local(Unit unit, int alignment, int size) {
        return new MemoryReference(unit.ebp, -alignment - size, size);
    }

    public static MemoryReference parameter(Unit unit, int alignment, int size) {
        return new MemoryReference(unit.ebp, alignment + 8, size);
    }

    public static MemoryReference member(Register register, int alignment, int size) {
        return new MemoryReference(register, alignment, size);
    }

    public LocationType getType() {
        return LocationType.MEMORY;
    }
}