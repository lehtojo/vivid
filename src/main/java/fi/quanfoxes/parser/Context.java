package fi.quanfoxes.parser;

import java.util.Collection;
import java.util.HashMap;

public class Context {
    private Context context;

    private HashMap<String, Variable> variables = new HashMap<>();
    private HashMap<String, Function> functions = new HashMap<>();
    private HashMap<String, Type> types = new HashMap<>();

    protected void onLink() {
        for (Variable variable : getVariables()) {
            if (variable.isTypeUnresolved()) {
                UnresolvedType unresolved = (UnresolvedType)variable.getType();

                try {
                    Type type = unresolved.resolve();
                    variable.setType(type);

                } catch (Exception e) {
                    // Resolve function fails when it cannot find the type
                    // Since there are usually multiple linkages, resolve can fail multiple times until the type is found
                }
            }
        }
    }

    public void link(Context context) {
        this.context = context;
        this.onLink();

        for (Type type : getTypes()) {
            type.onLink();
        }

        for (Function function : getFunctions()) {
            function.onLink();
        }
    }

    public Context getParent() {
        return context;
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

    public boolean isLocalTypeDeclared(String name) {
        return types.containsKey(name); 
    }

    public boolean isLocalFunctionDeclared(String name) {
        return functions.containsKey(name);
    }

    public boolean isLocalVariableDeclared(String name) {
        return variables.containsKey(name);
    }

    public boolean isVariableDeclared(String name) {
        return variables.containsKey(name) || (context != null && context.isVariableDeclared(name));
    }

    public boolean isTypeDeclared(String name) {
        return types.containsKey(name) || (context != null && context.isTypeDeclared(name));
    }

    public boolean isFunctionDeclared(String name) {
        return functions.containsKey(name) || (context != null && context.isFunctionDeclared(name));
    }

    public Variable getVariable(String name) throws Exception {
        if (variables.containsKey(name)) {
            return variables.get(name);
        }
        else if (context != null) {
            return context.getVariable(name);
        }
        else {
            throw new Exception("Couldn't find variable named " + name);
        }
    }

    public Type getType(String name) throws Exception {
        if (types.containsKey(name)) {
            return types.get(name);
        }
        else if (context != null) {
            return context.getType(name);
        }
        else {
            throw new Exception("Couldn't find type named " + name);
        }
    }

    public Function getFunction(String name) throws Exception {
        if (functions.containsKey(name)) {
            return functions.get(name);
        }
        else if (context != null) {
            return context.getFunction(name);
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

    public Type findParentType() {
        return (context instanceof Type) ? (Type)context : context.findParentType();
    }

    public Function findParentFunction() {
        return (context instanceof Function) ? (Function)context : context.findParentFunction();
    }

    public boolean isType() {
        return (this instanceof Type);
    }

    public boolean isFunction() {
        return (this instanceof Function);
    }
}