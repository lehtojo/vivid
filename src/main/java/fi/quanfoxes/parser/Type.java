package fi.quanfoxes.parser;

public class Type extends Context {
    private String name;
    private int modifiers;

    private Functions constructors = new Functions();
    private Functions deconstructors = new Functions();

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
    public void addDeconstructor(Function deconstructor) {
        deconstructors.add(deconstructor);
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
    public Functions getDeconstructor() {
        return deconstructors;
    }
}