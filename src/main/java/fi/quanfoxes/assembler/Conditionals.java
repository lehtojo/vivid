package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.IfNode;
import fi.quanfoxes.parser.nodes.OperatorNode;

public class Conditionals {
    /**
     * Builds an if statement node into instructions
     * @param unit Unit used to assemble
     * @param root If statements represented in node form
     * @param end Label that is used as an exit from the if statement's body
     * @return If statement built into instructions
     */
    private static Instructions build(Unit unit, IfNode root, String end) {
        Instructions instructions = new Instructions();
        String next = root.getSuccessor() != null ? unit.getLabel() : end;     

        Node condition = root.getCondition();

        // Assemble the condition
        if (condition instanceof OperatorNode) {
            OperatorNode operator = (OperatorNode)condition;

            Instructions jump = Comparison.jump(unit, operator, true, next);
            instructions.append(jump);
        }

        Instructions successor = null;

        // Assemble potential successor
        if (root.getSuccessor() != null) {
            Node node = root.getSuccessor();

            if (node instanceof IfNode) {
                successor = Conditionals.build(unit, (IfNode)node, end);
            }
            else {
                successor = unit.assemble(node);
            }
        }

        // Clone the unit since if statements may have multiple sections that don't affect each other
        Unit clone = unit.clone();

        Instructions body = clone.assemble(root.getBody());
        instructions.append(body);

        // Merge all assembled sections together
        if (successor != null) {
            instructions.append("jmp %s", end);
            instructions.label(next);
            instructions.append(successor); 
        }

        return instructions;
    }

    /**
     * Builds an if statement into instructions
     * @param unit Unit used to assemble
     * @param node If statement represented in node form
     * @return If statement built into instructions
     */
	public static Instructions start(Unit unit, IfNode node) {
        String end = unit.getLabel();

        Instructions instructions = Conditionals.build(unit, node, end);
        instructions.append("%s: ", end);

        unit.reset();

        return instructions;
	}
}