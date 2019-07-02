package fi.quanfoxes;

import java.util.Objects;

public class Keyword {
    private KeywordType type;
    private String identifier;

    public Keyword(String identifier) {
        this(KeywordType.NORMAL, identifier);
    }

    public Keyword(KeywordType type, String identifier) {
        this.type = type;
        this.identifier = identifier;
    }

    public KeywordType getType() {
        return type;
    }

    public String getIdentifier() {
        return identifier;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof Keyword)) return false;
        Keyword keyword = (Keyword) o;
        return Objects.equals(type, keyword.type) &&
               Objects.equals(identifier, keyword.identifier);
    }

    @Override
    public int hashCode() {
        return Objects.hash(identifier, type);
    }
}
