package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.List;

public class Variable {
    private String name;
    private Type type;
    private VariableType category;
    private int modifiers;
    private int length;

    private Context context;

    private int alignment;

    private List<Node> usages = new ArrayList<>();

    public Variable(Context context, Type type, VariableType category, String name, int modifiers) throws Exception {
        this(context, type, category, name, modifiers, 1);
    }

    public Variable(Context context, Type type, VariableType category, String name, int modifiers, int length) throws Exception {
        this.name = name;
        this.type = type;
        this.category = category;
        this.modifiers = modifiers;
        this.context = context;
        this.length = length;
        
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

    public void setVariableType(VariableType category) {
        this.category = category;
    }

    public VariableType getVariableType() {
        return category;
    }

    public boolean isTypeUnresolved() {
        return type instanceof Resolvable;
    }

    public int getModifiers() {
        return modifiers;
    }

    public int getLength() {
        return length;
    }

    public boolean isArray() {
        return length > 0;
    }

    public void setContext(Context context) {
        this.context = context;
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

    public String getFullname() {
        return name;
    }

    public void addUsage(Node node) {
        usages.add(node);
    }

    public List<Node> getUsages() {
        return usages;
    }
}