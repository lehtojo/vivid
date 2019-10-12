package fi.quanfoxes.parser;

public class Aligner {

    private static final int MEMBER_FUNCTION_PARAMETER_OFFSET = 4;
    private static final int GLOBAL_FUNCTION_PARAMETER_OFFSET = 0;

    /**
     * Aligns all variables and parameters recursively in the given context
     * @param context Context to process
     */
    public static void align(Context context) {
        // Align types and their subtypes
        for (Type type : context.getTypes()) {
            Aligner.align(type);
        }

        // Align function variables and parameters
        for (Functions functions : context.getFunctions()) {
            for (Function function : functions.getFunctions()) {
                Aligner.align(function, GLOBAL_FUNCTION_PARAMETER_OFFSET);
            }
        }
    }

    /**
     * Aligns member variables, functions and subtypes of the given type
     * @param type Type to align
     */
    private static void align(Type type) {
        int position = 0;

        // Align member variables
        for (Variable variable : type.getVariables()) {
            variable.setAlignment(position);
            position += variable.getType().getSize();
        }

        // Align member functions
        for (Functions functions : type.getFunctions()) {
            for (Function function : functions.getFunctions()) {
                Aligner.align(function, MEMBER_FUNCTION_PARAMETER_OFFSET);
            }
        }

        // Align constructors
        for (Function constructor : type.getConstructor().getFunctions()) {
            Aligner.align(constructor, MEMBER_FUNCTION_PARAMETER_OFFSET);
        }

        // Align subtypes
        for (Type subtype : type.getTypes()) {
            Aligner.align(subtype);
        }
    }

    /**
     * Aligns function variables
     * @param function Function to align
     * @param offset Parameter offset in stack
     */
    private static void align(Function function, int offset) {
        int position = offset;

        // Align parameters
        for (Variable variable : function.getParameters()) {
            if (variable.getVariableType() == VariableType.PARAMETER) {
                variable.setAlignment(position);
                position += variable.getType().getSize();
            }
        }

        position = 0;

        // Align local variables
        for (Variable variable : function.getLocals()) {
            if (variable.getVariableType() == VariableType.LOCAL) {
                variable.setAlignment(position);
                position += variable.getType().getSize();
            }
        }
    }
}