package fi.quanfoxes.parser;

public class Aligner {
    public static void align(Context context) {
        for (Type type : context.getTypes()) {
            Aligner.align(type);
        }

        for (Functions functions : context.getFunctions()) {
            for (Function function : functions.getFunctions()) {
                Aligner.align(function, 0);
            }
        }
    }

    public static void align(Type type) {
        int position = 0;

        for (Variable variable : type.getVariables()) {
            variable.setAlignment(position);
            position += variable.getType().getSize();
        }

        for (Functions functions : type.getFunctions()) {
            for (Function function : functions.getFunctions()) {
                Aligner.align(function, 4);
            }
        }

        for (Function constructor : type.getConstructor().getFunctions()) {
            Aligner.align(constructor, 4);
        }

        for (Type subtype : type.getTypes()) {
            Aligner.align(subtype);
        }
    }

    public static void align(Function function, int offset) {
        int position = offset;

        for (Variable variable : function.getParameters()) {
            variable.setAlignment(position);
            position += variable.getType().getSize();
        }

        position = 0;

        for (Variable variable : function.getLocals()) {
            if (variable.getVariableType() == VariableType.LOCAL) {
                variable.setAlignment(position);
                position += variable.getType().getSize();
            }
        }
    }
}