package fi.quanfoxes.parser;

import java.util.Collection;
import java.util.HashMap;

public class Context {
    private Context parent;

    private HashMap<String, Variable> variables = new HashMap<>();
    private HashMap<String, Function> functions = new HashMap<>();
    private HashMap<String, Type> types = new HashMap<>();

    public void link(Context context) {
        parent = context;
    }

    public void declare(Variable variable) throws Exception {
        if (isVariableDeclared(variable.getName())) {
            throw new Exception("Variable named " + variable.getName() + " already exists in this context");
        }

        variables.put(variable.getName(), variable);
    }

    public void declare(Type type) throws Exception {
        if (isTypeDeclared(type.getName())) {
            throw new Exception("Type named " + type.getName() + " already exists in this context");
        }

        types.put(type.getName(), type);
    }

    public void declare(Function function) throws Exception {
        if (isFunctionDeclared(function.getName())) {
            throw new Exception("Function named " + function.getName() + " already exists in this context");
        }

        functions.put(function.getName(), function);
    }

    public boolean isVariableDeclared(String name) {
        return variables.containsKey(name) || (parent != null && parent.isVariableDeclared(name));
    }

    public boolean isTypeDeclared(String name) {
        return types.containsKey(name) || (parent != null && parent.isTypeDeclared(name));
    }

    public boolean isFunctionDeclared(String name) {
        return functions.containsKey(name) || (parent != null && parent.isFunctionDeclared(name));
    }

    public Variable getVariable(String name) throws Exception {
        if (variables.containsKey(name)) {
            return variables.get(name);
        }
        else if (parent != null) {
            return parent.getVariable(name);
        }
        else {
            throw new Exception("Couldn't find variable named " + name);
        }
    }

    public Type getType(String name) throws Exception {
        if (types.containsKey(name)) {
            return types.get(name);
        }
        else if (parent != null) {
            return parent.getType(name);
        }
        else {
            throw new Exception("Couldn't find type named " + name);
        }
    }

    public Function getFunction(String name) throws Exception {
        if (functions.containsKey(name)) {
            return functions.get(name);
        }
        else if (parent != null) {
            return parent.getFunction(name);
        }
        else {
            throw new Exception("Couldn't find function named " + name);
        }
    }

    public Collection<Variable> getVariables() {
        return variables.values();
    }

    public Collection<Function> getFunctions() {
        return functions.values();
    }

    public Collection<Type> getTypes() {
        return types.values();
    }
}