package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.NodeType;
import fi.quanfoxes.parser.nodes.StringNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

import fi.quanfoxes.assembler.builders.*;

public class Assembler {
    private static final String SECTION_TEXT = "section .text" + "\n" +
                                               "" + "\n" +
                                               "global _start" + "\n" +
                                               "_start:" + "\n" +
                                               "call function_run" + "\n" +
                                               "" + "\n" +
                                               "mov eax, 1" + "\n" +
                                               "mov ebx, 0" + "\n" +
                                               "int 80h" + "\n" +
                                               "" + "\n" +
                                               "extern function_allocate" + "\n" +
                                               "extern function_integer_power" + "\n\n";

    private static final String SECTION_DATA = "section .data";

    public static String build(Node root, Context context) {
        Builder text = new Builder(SECTION_TEXT);
        Builder data = Assembler.data(root);

        Node iterator = root.first();

        while (iterator != null) {
            if (iterator.getNodeType() == NodeType.TYPE_NODE) {
                text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator.getNodeType() == NodeType.FUNCTION_NODE) {
                text.append(Functions.build((FunctionNode)iterator));
            }
            else if (iterator.getNodeType() == NodeType.VARIABLE_NODE) {
                data.append(Assembler.build((VariableNode)iterator));
            }

            iterator = iterator.next();
        }

        return text +  "\n" + data + "\n";
    }

    private static Builder data(Node root) {
        Builder bss = new Builder(SECTION_DATA);
        Assembler.data(root, bss, 1);
        return bss;
    }

    private static int data(Node root, Builder builder, int i) {
        Node iterator = root.first();
        
        while (iterator != null) {
            if (iterator.getNodeType() == NodeType.STRING_NODE) {
                String label = "S" + String.valueOf(i++);
                builder.append(Strings.build((StringNode)iterator, label));
            }
            else {
                i = Assembler.data(iterator, builder, i);
            }

            iterator = iterator.next();
        }

        return i;
    }

    private static String build(TypeNode node) {
        StringBuilder text = new StringBuilder();
        Node iterator = node.first();

        while (iterator != null) {
            if (iterator.getNodeType() == NodeType.TYPE_NODE) {
                text = text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator.getNodeType() == NodeType.FUNCTION_NODE) {
                text = text.append(Functions.build((FunctionNode)iterator));
            }

            iterator = iterator.next();
        }

        return text.toString();
    }

    private static final String DATA = "%s %s 0";

    private static String build(VariableNode node) {
        Variable variable = node.getVariable();
        String operand = Size.get(variable.getType().getSize()).getDataIdentifier();

        return String.format(DATA, variable.getFullname(), operand);
    }
}