package fi.quanfoxes.parser;

import java.util.Collection;
import java.util.HashMap;

import fi.quanfoxes.parser.nodes.TypeNode;

public class Context {
    private Context context;

    private HashMap<String, Variable> variables = new HashMap<>();
    private HashMap<String, Functions> functions = new HashMap<>();
    private HashMap<String, Type> types = new HashMap<>();

    protected void update() {
        for (Variable variable : getVariables()) {
            if (variable.isTypeUnresolved()) {
                Resolvable resolvable = (Resolvable)variable.getType();

                try {
                    TypeNode type = (TypeNode)resolvable.resolve(this);
                    variable.setType(type.getType());

                } catch (Exception e) {}
            }
        }

        for (Type type : getTypes()) {
            type.update();
        }

        for (Functions entry : getFunctions()) {
            entry.update();
        }
    }

    public void link(Context context) {
        this.context = context;
        this.update();
    }

    /**
     * Moves all types, functions and variables from the given context to this context, and then destroyes the given context
     * @param context Context to merge with
     */
    public void merge(Context context) {
        types.putAll(context.types);
        functions.putAll(context.functions);
        variables.putAll(context.variables);

        for (Type type : context.types.values()) {
            type.setParent(this);
        }

        for (Functions entry : context.functions.values()) {
            for (Function function : entry.getFunctions()) {
                function.setParent(this);
            }
        }

        for (Variable variable : context.variables.values()) {
            variable.setContext(this);
        }

        update();
    }

    public void setParent(Context context) {
        this.context = context;
    }

    public Context getParent() {
        return context;
    }

    /**
     * Declares new type in this context
     * @param type Type to declare
     * @throws Exception When type with same name already exists in this context this function throws an exception
     */
    public void declare(Type type) throws Exception {
        if (isLocalTypeDeclared(type.getName())) {
            throw new Exception(String.format("Type '%s' already exists in this context", type.getName()));
        }

        types.put(type.getName(), type);
    }

    /**
     * Declares new function in this context
     * @param function Function to declare
     */
    public void declare(Function function) throws Exception {
        Functions entry;

        if (isLocalFunctionDeclared(function.getName())) {
            entry = functions.get(function.getName());
        }
        else {
            functions.put(function.getName(), (entry = new Functions()));
        }

        entry.add(function);
    }

    /**
     * Declares new variable in this context
     * @param variable Variable to declare
     * @throws Exception When variable with same name already exists in this context this function throws an exception
     */
    public void declare(Variable variable) throws Exception {
        if (isLocalVariableDeclared(variable.getName())) {
            throw new Exception(String.format("Variable '%s' already exists in this context", variable.getName()));
        }

        variables.put(variable.getName(), variable);
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

    public Functions getFunction(String name) throws Exception {
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

    /**
     * Returns all variables this context owns
     * @return All variables this context owns
     */
    public Collection<Variable> getVariables() {
        return variables.values();
    }

    /**
     * Returns all functions this context owns
     * @return All functions this context owns
     */
    public Collection<Functions> getFunctions() {
        return functions.values();
    }

    /**
     * Returns all types this context owns
     * @return All types this context owns
     */
    public Collection<Type> getTypes() {
        return types.values();
    }

    /**
     * Returns the type context that this context is part of
     * @return Success: Type context that this context is part of, Failure: null
     */
    public Type getTypeParent() {
        if (this instanceof Type) {
            return (Type)this;
        }
        else if (context == null) {
            return null;
        }

        return (context instanceof Type) ? (Type)context : context.getTypeParent();
    }

    /**
     * Returns the function context that this context is part of
     * @return Success: Function context that this context is part of, Failure: null
     */
    public Function getFunctionParent() {
        if (this instanceof Function) {
            return (Function)this;
        }
        else if (context == null) {
            return null;
        }

        return (context instanceof Function) ? (Function)context : context.getFunctionParent();
    }

    /**
     * Returns whether this context is an instance of a type
     * @return True if this context is an instance of a type, otherwise false
     */
    public boolean isType() {
        return (this instanceof Type);
    }

    /**
     * Returns whether this context is an instance of a function
     * @return True if this context is an instance of a function, otherwise false
     */
    public boolean isFunction() {
        return (this instanceof Function);
    }
}