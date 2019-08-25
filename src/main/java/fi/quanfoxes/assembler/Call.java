package fi.quanfoxes.assembler;

import fi.quanfoxes.assembler.Memory.Evacuation;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.FunctionNode;

public class Call {
    public static Instructions build(Unit unit, FunctionNode node) {
        return Call.build(unit, null, node);
    }

    public static Instructions build(Unit unit, Reference object, String function, Reference... parameters) {
        Instructions instructions = new Instructions();
        Evacuation evacuation = Memory.evacuate(unit);

        if (evacuation.isNecessary()) {
            evacuation.start(instructions);
        }

        int memory = 0;

        for (int i = parameters.length - 1; i >= 0; i--) {
            Reference parameter = parameters[i];
            instructions.append(new Instruction("push", parameter));
            memory += parameter.getSize().getBytes();
        }

        if (object != null) {
            instructions.append(new Instruction("push", object));
            memory += 4;
        }

        unit.reset();

        instructions.append(new Instruction(String.format("call %s", function)));
        instructions.setReference(Value.getOperation(Reference.from(unit.eax)));

        if (memory > 0) {
            instructions.append("add esp, %d", memory);
        }

        if (evacuation.isNecessary()) {
            evacuation.restore(unit, instructions);
        }

        return instructions;
    }

    public static Instructions build(Unit unit, Reference object, FunctionNode node) {
        return Call.build(unit, object, node.getFunction(), node);
    }

    public static Instructions build(Unit unit, Reference object, Function function, Node parameters) {
        Instructions instructions = new Instructions();
        Evacuation evacuation = Memory.evacuate(unit);

        if (evacuation.isNecessary()) {
            evacuation.start(instructions);
        }

        int memory = 0;

        Node iterator = parameters.last();
        
        while (iterator != null) {
            Instructions parameter = References.read(unit, iterator);
            instructions.append(parameter);
            instructions.append(new Instruction("push", parameter.getReference()));

            memory += parameter.getReference().getSize().getBytes();

            iterator = iterator.previous();
        }

        if (function.isMember()) {
            if (object != null) {
                instructions.append(new Instruction("push", object));
            }
            else {
                instructions.append(new Instruction("push", References.OBJECT_POINTER));
            }

            memory += 4;
        }    

        unit.reset();

        instructions.append(new Instruction(String.format("call %s", function.getFullname())));
        instructions.setReference(Value.getOperation(Reference.from(unit.eax)));

        if (memory > 0) {
            instructions.append("add esp, %d", memory);
        }

        if (evacuation.isNecessary()) {
            evacuation.restore(unit, instructions);
        }

        return instructions;
    } 
}