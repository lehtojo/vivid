package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.references.RegisterReference;
import fi.quanfoxes.parser.nodes.ReturnNode;

public class Return {
    public static Instructions build(Unit unit, ReturnNode node) {
        Instructions instructions = new Instructions();

        Instructions object = References.read(unit, node.first());
        instructions.append(object);

        Reference reference = object.getReference();

        if (reference.getRegister() != unit.eax) {
            instructions.append(Memory.move(unit, reference, new RegisterReference(unit.eax)));
        }

        instructions.append(FunctionBuilder.FOOTER);

        return instructions.setReference(Value.getOperation(unit.eax, reference.getSize()));
    }
}