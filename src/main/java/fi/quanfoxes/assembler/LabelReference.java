package fi.quanfoxes.assembler;

public class LabelReference extends Reference {
    private String label;
    private boolean point;

    public LabelReference(String label, boolean point) {
        super(Size.DWORD);
        this.label = label;
        this.point = point;
    }

    @Override
    public String use() {
        return point ? String.format("[%s]", label) : label;
    }

    @Override
    public boolean isComplex() {
        return point;
    }
}