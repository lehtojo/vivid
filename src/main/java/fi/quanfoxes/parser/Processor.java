package fi.quanfoxes.parser;

import fi.quanfoxes.parser.nodes.ElseNode;
import fi.quanfoxes.parser.nodes.IfNode;

public class Processor {
    /**
     * Connects if statements with else if or else statements
     * @param node Node to start connecting
     */
    private static void connect(IfNode node) {
        Node next = node.next();

        if (next != null) {
            if (next instanceof IfNode) {
                Processor.connect((IfNode)next);
                node.setSuccessor(next);
            }
            else if (next instanceof ElseNode) {
                node.setSuccessor(next);
            }
        }
    }

    /**
     * Looks for unconnected statements and connects them
     * @param node Node tree to scan
     */
    private static void conditionals(Node node) {
        Node iterator = node.first();

        while (iterator != null) {
            Node next = iterator.next();

            if (iterator instanceof IfNode) {
                Processor.connect((IfNode)iterator);
            }

            Processor.conditionals(iterator);

            iterator = next;
        }
    }
    
    public static void process(Node node) {
        Processor.conditionals(node);
    }
}