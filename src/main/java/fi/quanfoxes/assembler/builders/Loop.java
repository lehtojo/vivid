package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.LoopNode;

public class Loop {
    private static Instructions getForeverLoop(Unit unit, String start, Node node) {
        Instructions instructions = new Instructions();
    
        Instructions body = unit.assemble(node);
        instructions.append(body);

        instructions.append("jmp %s", start);

        unit.reset();

        return instructions;
    }

    public static Instructions build(Unit unit, LoopNode node) {
        Instructions instructions = new Instructions();

        unit.reset();

        String start = unit.getLabel();
        instructions.label(start);

        if (node.isForever()) {
            Instructions body = Loop.getForeverLoop(unit, start, node.getBody());
            instructions.append(body);

            return instructions;
        }

        String end = unit.getLabel();

        Instructions condition = Comparison.jump(unit, (OperatorNode)node.getCondition(), true, end);
        instructions.append(condition);

        unit.step();

        Instructions body = unit.assemble(node.getBody());
        instructions.append(body);

        instructions.append("jmp %s", start);
        instructions.label(end);

        unit.reset();
        
        return instructions;
    }
}