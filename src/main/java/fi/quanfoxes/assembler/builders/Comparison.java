package fi.quanfoxes.assembler.builders;

import java.util.HashMap;
import java.util.Map;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.lexer.ComparisonOperator;
import fi.quanfoxes.lexer.Operator;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.nodes.OperatorNode;

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

    private static String getComparisonJump(ComparisonOperator operator) {
        return jumps.get(operator);
    }
    
    public static Instructions jump(Unit unit, OperatorNode node, boolean invert, String label) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.VALUE, ReferenceType.READ);

        Reference left = operands[0];
        Reference right = operands[1];

        instructions.append(new Instruction("cmp", left, right, left.getSize()));
        
        ComparisonOperator operator = (ComparisonOperator)node.getOperator();
        
        if (invert) {
            operator = operator.getCounterpart();
        }

        instructions.append("%s %s", getComparisonJump(operator), label);

        return instructions;
    }
}