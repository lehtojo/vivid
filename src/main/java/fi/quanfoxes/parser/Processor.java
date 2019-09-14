package fi.quanfoxes.parser;

import fi.quanfoxes.parser.nodes.IfNode;
import fi.quanfoxes.parser.nodes.NodeType;

public class Processor {
    /**
     * Connects if statements with else if or else statements
     * @param node Node to start connecting
     */
    private static void connect(IfNode node) {
        Node next = node.next();

        if (next != null) {
            if (next.getNodeType() == NodeType.ELSE_IF_NODE) {
                Processor.connect((IfNode)next);
                node.setSuccessor(next);
            }
            else if (next.getNodeType() == NodeType.ELSE_NODE) {
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

            if (iterator.getNodeType() == NodeType.IF_NODE) {
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