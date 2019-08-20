package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Assembler {
    private static final String SECTION_TEXT = "section .text" + "\n" +
                                               "" + "\n" +
                                               "global _start" + "\n" +
                                               "_start:" + "\n" +
                                               "call function_run" + "\n" +
                                               "" + "\n" +
                                               "mov eax, 1" + "\n" +
                                               "mov ebx, 0" + "\n" +
                                               "int 80h" + "\n\n";

    private static final String SECTION_DATA = "section .data";

    public static String build(Node root, Context context) {
        Builder text = new Builder(SECTION_TEXT);
        Builder data = new Builder(SECTION_DATA);

        Node iterator = root.first();

        while (iterator != null) {
            if (iterator instanceof TypeNode) {
                text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator instanceof FunctionNode) {
                text.append(FunctionBuilder.build((FunctionNode)iterator));
            }
            else if (iterator instanceof VariableNode) {
                data.append(Assembler.build((VariableNode)iterator));
            }

            iterator = iterator.next();
        }

        return text.toString() +  "\n" + data.toString();
    }

    public static String build(TypeNode node) {
        StringBuilder text = new StringBuilder();
        Node iterator = node.first();

        while (iterator != null) {
            if (iterator instanceof TypeNode) {
                text = text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator instanceof FunctionNode) {
                text = text.append(FunctionBuilder.build((FunctionNode)iterator));
            }

            iterator = iterator.next();
        }

        return text.toString();
    }

    private static final String DATA = "%s %s 0";

    public static String build(VariableNode node) {
        Variable variable = node.getVariable();
        String operand = Size.get(variable.getType().getSize()).getDataIdentifier();

        return String.format(DATA, variable.getFullname(), operand);
    }
}