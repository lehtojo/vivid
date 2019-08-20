package fi.quanfoxes.assembler;

public class ParameterReference extends Reference {
    private int alignment;

    public ParameterReference(int alignment, int bytes) {
        super(Size.get(bytes));
        this.alignment = alignment;
    }

    @Override
    public String use() {
        return String.format("[ebp+%d]", alignment + 8);
    }

    @Override
    public boolean isComplex() {
        return true;
    }
}