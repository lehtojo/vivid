package fi.quanfoxes.parser;

import fi.quanfoxes.AccessModifier;

public class Constructor extends Function {
    private static final String IDENTIFIER = "constructor";
    private static final String INDEXED_IDENTIFIER = "constructor_%d";

    private boolean automatic;

    public static Constructor empty(Context context) {
        Constructor constructor = new Constructor(context, AccessModifier.PUBLIC, true);
        constructor.setParameters(new Node());

        return constructor;
    }

    public Constructor(Context context, int modifiers) {
        this(context, modifiers, false);
    }

    public Constructor(Context context, int modifiers, boolean automatic) {
        super(context, modifiers);
        this.automatic = automatic;
    }

    public boolean isDefault() {
        return automatic;
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