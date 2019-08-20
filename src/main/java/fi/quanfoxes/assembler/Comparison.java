package fi.quanfoxes.assembler;

import java.util.HashMap;
import java.util.Map;

import fi.quanfoxes.lexer.ComparisonOperator;
import fi.quanfoxes.lexer.Operator;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Comparison {
    private static final Map<Operator, String> jumps = new HashMap<>();

    static {
        jumps.put(Operators.GREATER_THAN, "jg");
        jumps.put(Operators.GREATER_OR_EQUAL, "jge");
        jumps.put(Operators.LESS_THAN, "jl");
        jumps.put(Operators.LESS_OR_EQUAL, "jle");
        jumps.put(Operators.EQUALS, "je");
        jumps.put(Operators.NOT_EQUALS, "jne");
    }

    private static boolean isPrimitive(Node node) {
        return node instanceof VariableNode || node instanceof NumberNode;
    }

    private static String getComparisonJump(ComparisonOperator operator) {
        return jumps.get(operator);
    }
    
    public static Instructions jump(Unit unit, OperatorNode node, boolean invert, String label) {
        Instructions instructions = new Instructions();

        Instructions left = null;
        Instructions right = null;

        if (isPrimitive(node.getLeft()) && isPrimitive(node.getRight())) {
            left = References.value(unit, node.getLeft());
            right = References.read(unit, node.getRight());

            instructions.append(left, right);
        }
        else {

            if (!isPrimitive(node.getRight())) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (!isPrimitive(node.getLeft())) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }

            if (right == null) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (left == null) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }
        }

        instructions.append(new Instruction("cmp", left.getReference(), right.getReference()));
        
        ComparisonOperator operator = (ComparisonOperator)node.getOperator();

        if (invert) {
            operator = operator.getCounterpart();
        }

        instructions.append("%s %s", getComparisonJump(operator), label);

        return instructions;
    }
}