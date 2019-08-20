package fi.quanfoxes.assembler;

public class MemoryReference extends Reference {
    private Register register;
    private int alignment;

    public MemoryReference(Register register, int alignment, int size) {
        super(Size.get(size));

        this.register = register;
        this.alignment = alignment;
    }

    @Override
    public String use() {
        if (alignment > 0) {
            return String.format("[%s+%d]", register, alignment);
        }
        else if (alignment < 0) {
            return String.format("[%s%d]", register, alignment);
        }
        else {
            return String.format("[%s]", register);
        }
    }

    @Override
    public boolean isComplex() {
        return true;
    }
}