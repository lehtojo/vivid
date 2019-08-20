package fi.quanfoxes.assembler;

public class Instructions {
    private StringBuilder builder = new StringBuilder();
    private Reference reference;

    public Instructions comment(String comment) {
        builder = builder.append("; ").append(comment).append("\n");
        return this;
    }

    public Instructions label(String name) {
        builder = builder.append(name).append(":\n");
        return this;
    }

    public Instructions append(String raw) {
        builder = builder.append(raw).append("\n");
        return this;
    }

    public Instructions append(String format, Object... args) {
        builder = builder.append(String.format(format, args)).append("\n");
        return this;
    }

    public Instructions append(Instruction instruction) {
        builder = builder.append(instruction).append("\n");
        return this;
    }

    public Instructions append(Instructions...instructions) {
        for (Instructions i : instructions) {
            builder = builder.append(i);
        }

        return this;
    }

    public Instructions setReference(Reference reference) {
        this.reference = reference;
        return this;
    }
    
    public Reference getReference() {
        return reference;
    }

    @Override
    public String toString() {
        return builder.toString();
    }
}