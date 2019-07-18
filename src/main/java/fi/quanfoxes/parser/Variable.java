package fi.quanfoxes.parser;

import java.util.ArrayList;

public class Variable {
    private String name;
    private Type type;
    private int modifiers;

    private Context context;

    private int alignment;

    private ArrayList<Node> usages = new ArrayList<>();

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

    public void setType(Type type) {
        this.type = type;
    }

    public Type getType() {
        return type;
    }

    public boolean isTypeUnresolved() {
        return type instanceof UnresolvedType;
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

    public void addUsage(Node node) {
        usages.add(node);
    }

    public ArrayList<Node> getUsages() {
        return usages;
    }
}