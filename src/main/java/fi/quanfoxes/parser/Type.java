package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.List;

public class Type extends Context {
    private String name;
    private int modifiers;

    private Functions constructors = new Functions();
    private Functions destructors = new Functions();

    private List<Type> supertypes;

    public Type(Context context, String name, int modifiers) throws Exception {
        this(context, name, modifiers, new ArrayList<>());
    }

    public Type(Context context, String name, int modifiers, List<Type> supertypes) throws Exception {
        this.name = name;
        this.modifiers = modifiers;
        this.supertypes = supertypes;

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

    public boolean isSuperFunctionDeclared(String name) {
        return supertypes.stream().anyMatch(t -> t.isLocalFunctionDeclared(name));
    }

    public boolean isSuperVariableDeclared(String name) {
        return supertypes.stream().anyMatch(t -> t.isLocalVariableDeclared(name));
    }

    public Functions getSuperFunction(String name) {
        return supertypes.stream().filter(t -> t.isLocalFunctionDeclared(name))
                                        .map(t -> t.getFunction(name)).findFirst().get();
    }

    public Variable getSuperVariable(String name) {
        return supertypes.stream().filter(t -> t.isLocalVariableDeclared(name))
                                        .map(t -> t.getVariable(name)).findFirst().get();
    }

    @Override
    public boolean isLocalFunctionDeclared(String name) {
        return super.isLocalFunctionDeclared(name) || isSuperFunctionDeclared(name);
    }

    @Override
    public boolean isLocalVariableDeclared(String name) {
        return super.isLocalVariableDeclared(name) || isSuperVariableDeclared(name);
    }

    @Override
    public boolean isFunctionDeclared(String name) {
        return super.isFunctionDeclared(name) || isSuperFunctionDeclared(name);
    }

    @Override
    public boolean isVariableDeclared(String name) {
        return super.isVariableDeclared(name) || isSuperVariableDeclared(name);
    }

    @Override
    public Functions getFunction(String name) {
        if (super.isLocalFunctionDeclared(name)) {
            return super.getFunction(name);
        }
        else if (isSuperFunctionDeclared(name)) {
            return getSuperFunction(name);
        }
        else {
            return super.getFunction(name);
        }
    }

    @Override
    public Variable getVariable(String name) {
        if (super.isLocalVariableDeclared(name)) {
            return super.getVariable(name);
        }
        else if (isSuperVariableDeclared(name)) {
            return getSuperVariable(name);
        }
        else {
            return super.getVariable(name);
        }
    }

    /**
     * Returns the name of the type
     * @return Name of the type
     */
    public String getName() {
        return name;
    }

    /**
     * Returns the access modfiers of the type
     * @return Access modifiers of the type
     */
    public int getModifiers() {
        return modifiers;
    }

    /**
     * Returns all types that this type extends
     * @return All types that this type extends
     */
    public List<Type> getSupertypes() {
        return supertypes;
    }

    /**
     * Declares constructor for this type
     * @param constructor Constructor to declare
     */
    public void addConstructor(Function constructor) {
        constructors.add(constructor);
    }

    /**
     * Declares deconstructor for this type
     * @param constructor Deconstructor to declare
     */
    public void addDestructor(Function destructor) {
        destructors.add(destructor);
    }

    /**
     * Returns the default constructor of this type
     * @return Default constructor of this type
     */
    public Functions getConstructor() {
        return constructors;
    }

    /**
     * Returns the default deconstructor of this type
     * @return Default deconstructor of this type
     */
    public Functions getDestructor() {
        return destructors;
    }
}