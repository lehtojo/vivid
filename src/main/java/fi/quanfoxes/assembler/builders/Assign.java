package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Assign {
    private static boolean isVariable(Node node) {
        return node instanceof VariableNode;
    }

    /**
     * Builds an assign operation into instructions
     * @param node Assign node
     * @return Assign instructions
     */
    public static Instructions build(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions right = References.value(unit, node.getRight());
        Instructions left = References.direct(unit, node.getLeft());
        
        instructions.append(right, left);
        instructions.append(new Instruction("mov", left.getReference(), right.getReference(), left.getReference().getSize()));

        if (isVariable(node.getLeft())) {
            
            Variable variable = ((VariableNode)node.getLeft()).getVariable();
            instructions.setReference(Value.getVariable(right.getReference(), variable));
        }

        return instructions;
    }
}