package fi.quanfoxes.assembler;

public class DataSectionReference extends Reference {
    private String identifier;

    public DataSectionReference(String identifier, int bytes) {
        super(Size.get(bytes));
        this.identifier = identifier;
    }

    public String getIdentifier() {
        return identifier;
    }

    @Override
    public String use() {
        return String.format("[%s]", identifier);
    }
    
    @Override
    public boolean isComplex() {
        return true;
    }
}