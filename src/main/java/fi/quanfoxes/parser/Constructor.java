package fi.quanfoxes.parser;

import fi.quanfoxes.AccessModifier;

public class Constructor extends Function {
    private static final String IDENTIFIER = "constructor";
    private static final String INDEXED_IDENTIFIER = "constructor_%d";

    public static Constructor empty(Context context) {
        Constructor constructor = new Constructor(context, AccessModifier.PUBLIC);
        constructor.setParameters(new Node());

        return constructor;
    }

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