package fi.quanfoxes.assembler;

public class NumberReference extends Reference {
    private Number number;

    public NumberReference(Number number) {
        super(Size.DWORD);
        this.number = number;
    }

    public Number getNumberValue() {
        return number;
    }

    @Override
    public String use() {
        return number.toString();
    }
}