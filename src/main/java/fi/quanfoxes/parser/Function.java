package fi.quanfoxes.parser;

import java.util.ArrayList;

import fi.quanfoxes.parser.nodes.VariableNode;

public class Function extends Context {
    private String name;
    private int modifiers;

    private ArrayList<Type> parameters = new ArrayList<>();

    private Type returnType;

    private ArrayList<Node> usages = new ArrayList<>();

    public Function(Context context, String name, int modifiers, Type returnType) throws Exception {
        this.name = name;
        this.modifiers = modifiers;
        this.returnType = returnType;

        super.link(context);
        context.declare(this);
    }

    public String getName() {
        return name;
    }

    public int getModifiers() {
        return modifiers;
    }

    public void setParameters(Node node) {
        VariableNode parameter = (VariableNode)node.getFirst();
        
        while (parameter != null) {
            Variable variable = parameter.getVariable();
            parameters.add(variable.getType());

            parameter = (VariableNode)parameter.getNext();
        }
    }

    public ArrayList<Type> getParameters() {
        return parameters;
    }

    public int getParameterCount() {
        return parameters.size();
    }

    public Type getReturnType() {
        return returnType;
    }

    public void setReturnType(Type type) {
        this.returnType = type;
    }

    public void addUsage(Node node) {
        usages.add(node);
    }

    public ArrayList<Node> getUsages() {
        return usages;
    }
}