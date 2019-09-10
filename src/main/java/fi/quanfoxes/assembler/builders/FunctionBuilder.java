package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.lexer.Flag;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.FunctionNode;

public class FunctionBuilder {
    
    public static final String HEADER = "%s:" + "\n" +
                                         "push ebp" + "\n" +
                                         "mov ebp, esp";

    public static final String RESERVE = "sub esp, %d";

    public static final String FOOTER = "mov esp, ebp" + "\n" +
                                         "pop ebp" + "\n" +
                                         "ret";

    public static String build(FunctionNode node) {
        Function function = node.getFunction();
        Builder builder = new Builder();

        if (Flag.has(function.getModifiers(), AccessModifier.EXTERNAL)) {
            builder.append("extern %s", function.getFullname());
            return builder.toString();
        }

        if (function.isGlobal()) {
            builder.comment("Represents global function '%s'", function.getName());
        }
        else {
            builder.comment("Member function '%s' of type '%s'", function.getName(), function.getTypeParent().getName());
        }

        // Append the function stack frame
        builder.append(HEADER, function.getFullname());

        // Add instructions for local variables
        int memory = function.getLocalMemorySize();

        if (memory > 0) {
            builder.append(RESERVE, memory);
        }

        Unit unit = new Unit(function.getFullname());

        // Assemble the body of this function
        Node iterator = node.getBody().first();

        while (iterator != null) {
            Instructions instructions = unit.assemble(iterator);

            if (instructions != null) {
                builder.append(instructions.toString());
            }

            unit.step();

            iterator = iterator.next();
        }
        
        // Append the stack frame cleanup
        builder = builder.append(FOOTER);

        return builder.toString();
    }
}