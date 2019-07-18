package fi.quanfoxes.parser;

public class Type extends Context {
    private String name;
    private int modifiers;

    public Type(Context context, String name, int modifiers) throws Exception {
        this.name = name;
        this.modifiers = modifiers;

        super.link(context);
        context.declare(this);
    }

    public Type(String name, int modifiers) {
        this.name = name;
        this.modifiers = modifiers;
    }

    public Type(Context context) {
        super.link(context);
    }

    public String getName() {
        return name;
    }

    public int getModifiers() {
        return modifiers;
    }
}