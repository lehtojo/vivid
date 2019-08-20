package fi.quanfoxes.parser;

public class Constructor extends Function {
    private static final String IDENTIFIER = "constructor";
    private static final String INDEXED_IDENTIFIER = "constructor_%d";

    public Constructor(Context context, int modifiers) {
        super(context, modifiers);
    }

    @Override
    public String getIdentifier() {
        int index = getIndex();

        if (index != -1) {
            return String.format(INDEXED_IDENTIFIER, index);
        }
        else {
            return IDENTIFIER;
        }
    }
}