package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Assign {
    private static boolean isVariable(Node node) {
        return node instanceof VariableNode;
    }

    public static Instructions build(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions right = References.value(unit, node.getRight());
        Instructions left = References.write(unit, node.getLeft());
        
        instructions.append(right, left);
        instructions.append(new Instruction("mov", left.getReference(), right.getReference()));

        if (isVariable(node.getLeft())) {
            Variable variable = ((VariableNode)node.getLeft()).getVariable();
            instructions.setReference(Value.getVariable(right.getReference(), variable));
        }

        return instructions;
    }
}