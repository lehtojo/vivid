package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.ConstructionNode;

public class Construction {
    public static Instructions build(Unit unit, ConstructionNode node) {
        Instructions instructions = new Instructions();

        Type type = node.getType();
        Instructions allocation = Call.build(unit, null, "function_allocate", new NumberReference(type.getContentSize()));

        instructions.append(allocation);
        instructions.setReference(allocation.getReference());

        Instructions constructor = Call.build(unit, allocation.getReference(), node.getConstructor(), node.getParameters());
        instructions.append(constructor);

        return instructions;
    }
}