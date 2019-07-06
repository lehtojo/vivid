package fi.quanfoxes.parser;

public class Variable {
    private String name;
    private Type type;
    private int modifiers;

    private Context context;

    private int alignment;

    public Variable(Context context, Type type, String name, int modifiers) throws Exception {
        this.name = name;
        this.type = type;
        this.modifiers = modifiers;
        this.context = context;
        
        context.declare(this);
    }

    public String getName() {
        return name;
    }

    public Type getType() {
        return type;
    }

    public int getModifiers() {
        return modifiers;
    }

    public Context getContext() {
        return context;
    }

    public void setAlignment(int alignment) {
        this.alignment = alignment;
    }

    public int getAlignment() {
        return alignment;
    }
}