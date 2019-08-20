package fi.quanfoxes.assembler;

public class MemberReference extends Reference {
    private int alignment;
    private boolean write;

    public MemberReference(int alignment, int bytes, boolean write) {
        super(Size.get(bytes));
        this.alignment = alignment;
        this.write = write;
    }

    @Override
    public String use() {
        if (alignment == 0) {
            return write ? "[edi]" : "[esi]"; 
        }

        return String.format("[%s+%d]", write ? "edi" : "esi", alignment);
    }

    @Override
    public boolean isComplex() {
        return true;
    }
}