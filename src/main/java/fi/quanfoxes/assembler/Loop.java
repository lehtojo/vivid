package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.WhileNode;

public class Loop {
    public static Instructions build(Unit unit, WhileNode node) {
        Instructions instructions = new Instructions();

        String start = unit.getLabel();
        String end = unit.getLabel();

        instructions.label(start);

        Instructions condition = Comparison.jump(unit, (OperatorNode)node.getCondition(), true, end);
        instructions.append(condition);

        Instructions body = unit.assemble(node.getBody());
        instructions.append(body);

        instructions.append("jmp %s", start);
        instructions.label(end);

        unit.reset();
        
        return instructions;
    }
}