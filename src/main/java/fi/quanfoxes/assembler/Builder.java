package fi.quanfoxes.assembler;

public class Builder {
    private StringBuilder builder = new StringBuilder();

    public Builder() {}
    
    public Builder(String text) {
        append(text);
    }

    public Builder comment(String comment) {
        builder = builder.append("; ").append(comment).append("\n");
        return this;
    }

    public Builder comment(String format, Object... args) {
        builder = builder.append("; ").append(String.format(format, args)).append("\n");
        return this;
    }

    public Builder append(String text) {
        builder = builder.append(text).append("\n");
        return this;
    }

    public Builder append(String format, Object... args) {
        builder = builder.append(String.format(format, args)).append("\n");
        return this;
    }

    @Override
    public String toString() {
        return builder.toString() + "\n";
    }
}