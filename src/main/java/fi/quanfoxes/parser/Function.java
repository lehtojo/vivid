package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import fi.quanfoxes.parser.nodes.VariableNode;

public class Function extends Context {
    private String name;
    private int modifiers;

    private Map<String, Variable> parameters = new HashMap<>();
    //private ArrayList<Type> parameters = new ArrayList<>();

    private Type returnType;

    private ArrayList<Node> usages = new ArrayList<>();

    public Function(Context context, String name, int modifiers, Type returnType) throws Exception {
        this.name = name;
        this.modifiers = modifiers;
        this.returnType = returnType;

        super.link(context);
        context.declare(this);
    }

    @Override
    public boolean isLocalVariableDeclared(String name) {
        return parameters.containsKey(name) || super.isLocalVariableDeclared(name);
    }

    @Override
    public boolean isVariableDeclared(String name) {
        return parameters.containsKey(name) || super.isVariableDeclared(name);
    }

    @Override
    public Variable getVariable(String name) throws Exception {
        if (parameters.containsKey(name)) {
            return parameters.get(name);
        }
        
        return super.getVariable(name);
    }

    @Override
    public Collection<Variable> getVariables() {
        return Stream.concat(super.getVariables().stream(), parameters.values().stream()).collect(Collectors.toList());
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
            parameters.put(variable.getName(), variable);

            parameter = (VariableNode)parameter.getNext();
        }
    }

    public List<Type> getParameters() {
        return parameters.values().stream().map(Variable::getType).collect(Collectors.toList());
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