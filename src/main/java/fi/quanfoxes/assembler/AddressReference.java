package fi.quanfoxes.assembler;

public class AddressReference extends Reference {
    private Number number;

    public AddressReference(Number number) {
        super(Size.get(4));
        this.number = number;
    }

    @Override
    public String use() {
        return String.format("[%d]", number.longValue());
    }

    @Override
    public boolean isComplex() {
        return true;
    }
}