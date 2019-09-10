package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.FunctionNode;

public class Call {
    public static Instructions build(Unit unit, FunctionNode node) {
        return Call.build(unit, null, node);
    }

    public static Instructions build(Unit unit, Reference object, String function, Size result, Reference... parameters) {
        Instructions instructions = new Instructions();
        Evacuation evacuation = Memory.evacuate(unit);

        // When the function is for example in the middle of an expression, some critical values must be saved before the call
        if (evacuation.necessary()) {
            evacuation.start(instructions);
        }

        int memory = 0;

        // Pass the function parameters
        for (int i = parameters.length - 1; i >= 0; i--) {
            Reference parameter = parameters[i];
            instructions.append(new Instruction("push", parameter));

            memory += parameter.getSize().getBytes();
        }

        // Pass the object pointer
        if (object != null) {
            instructions.append(new Instruction("push", object));
            memory += object.getSize().getBytes();
        }

        // Unit must be reset since function may affect the registers
        unit.reset();

        // Call the function
        instructions.append(new Instruction(String.format("call %s", function)));
        instructions.setReference(Value.getOperation(unit.eax, result));

        // Remove parameters from the stack, if needed
        if (memory > 0) {
            instructions.append("add esp, %d", memory);
        }

        // Restore saved values from stack, if needed
        if (evacuation.necessary()) {
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

        // When the function is for example in the middle of an expression, some critical values must be saved before the call
        if (evacuation.necessary()) {
            evacuation.start(instructions);
        }

        int memory = 0;

        Node iterator = parameters.last();
        
        // Pass the function parameters
        while (iterator != null) {
            Instructions parameter = References.read(unit, iterator);
            instructions.append(parameter);
            instructions.append(new Instruction("push", parameter.getReference()));

            memory += parameter.getReference().getSize().getBytes();

            iterator = iterator.previous();
        }

        // Pass the object pointer
        if (function.isMember()) {
            if (object != null) {
                instructions.append(new Instruction("push", object));
            }
            else {
                Register register = unit.getObjectPointer();

                if (register != null) {
                    instructions.append(new Instruction("push", Reference.from(register)));
                }
                else {
                    instructions.append(new Instruction("push", References.getObjectPointer(unit)));
                }            
            }

            memory += References.getObjectPointer(unit).getSize().getBytes();
        }    

        // Unit must be reset since function may affect the registers
        unit.reset();

        // Get the return type
        Type result = function.getReturnType();

        // Call the function
        instructions.append(new Instruction(String.format("call %s", function.getFullname())));

        if (result != null) {
            instructions.setReference(Value.getOperation(unit.eax, Size.get(result.getSize())));
        }

        // Remove parameters from the stack, if needed
        if (memory > 0) {
            instructions.append("add esp, %d", memory);
        }

        // Restore saved values from stack, if needed
        if (evacuation.necessary()) {
            evacuation.restore(unit, instructions);
        }

        return instructions;
    } 
}