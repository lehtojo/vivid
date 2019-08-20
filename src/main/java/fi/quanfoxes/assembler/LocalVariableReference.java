package fi.quanfoxes.assembler;

public class LocalVariableReference extends Reference {
    private int alignment;

    public LocalVariableReference(int alignment, int bytes) {
        super(Size.get(bytes));
        this.alignment = alignment;
    }

    @Override
    public String use() {
        return String.format("[ebp-%d]", alignment + 4);
    }

    @Override
    public boolean isComplex() {
        return true;
    }
}