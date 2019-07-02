package fi.quanfoxes;

import java.util.Objects;

public class AccessModifierKeyword extends Keyword {
    private int modifier;

    public AccessModifierKeyword(String identifier, int modifier) {
        super(KeywordType.ACCESS_MODIFIER, identifier);
        this.modifier = modifier;
    }

    public int getModifier() {
        return modifier;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        if (!super.equals(o)) return false;
        AccessModifierKeyword that = (AccessModifierKeyword) o;
        return modifier == that.modifier;
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), modifier);
    }
}
