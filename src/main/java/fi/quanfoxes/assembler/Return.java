package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.nodes.ReturnNode;

public class Return {
    public static Instructions build(Unit unit, ReturnNode node) {
        Instructions instructions = new Instructions();

        Instructions reference = References.read(unit, node.first());
        instructions.append(reference);

        if (reference.getReference().getRegister() != unit.eax) {
            instructions.append(Memory.toRegister(unit, reference.getReference()));
        }

        return instructions.setReference(Value.getOperation(Reference.from(unit.eax)));
    }
}