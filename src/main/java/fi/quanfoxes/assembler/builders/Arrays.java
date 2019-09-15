package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.Types;
import fi.quanfoxes.assembler.Instructions;
import fi.quanfoxes.assembler.Memory;
import fi.quanfoxes.assembler.Reference;
import fi.quanfoxes.assembler.Register;
import fi.quanfoxes.assembler.Size;
import fi.quanfoxes.assembler.Unit;
import fi.quanfoxes.assembler.Value;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.assembler.references.MemoryReference;
import fi.quanfoxes.assembler.references.NumberReference;
import fi.quanfoxes.assembler.references.RegisterReference;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.OperatorNode;

public class Arrays {
    /**
     * Converts reference to string format that is compatible with lea instruction
     * @param reference Reference to convert to string
     * @return Reference represented in string format
     */
    private static String toString(Reference reference) {
        switch (reference.getType()) {

            case MEMORY: {
                MemoryReference memory = (MemoryReference)reference;
                return memory.getContent();
            }

            case NUMBER: {
                NumberReference number = (NumberReference)reference;
                return number.getNumberValue().toString();
            }

            case REGISTER: {
                RegisterReference register = (RegisterReference)reference;
                return register.peek(reference.getSize());
            }

            case VALUE: {
                Value value = (Value)reference;
                return Arrays.toString(value.getReference());
            }

            default: {
                System.err.println("ERROR: Unsupported array usage");
            }
        }
        
        return "";
    }
 
    /**
     * Combines two references into one lea calculation
     * @param object Object to offset in memory
     * @param index Index of the element
     * @param stride Element size in bytes
     * @return Memory calculation for lea instruction
     */
    private static String combine(Reference object, Reference index, int stride) {
        return String.format("[%s+%s*%d]", toString(object), toString(index), stride);
    }

    /**
     * Returns the stride between elements of the given type
     * @param type Type of the elements
     * @return Stride between the elements in an array
     */
    private static Size getStride(Type type) {
        return type == Types.LINK ? Size.BYTE : Size.get(type.getSize());
    }

    public static Instructions build(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.VALUE, ReferenceType.VALUE);

        Reference left = operands[0];
        Reference right = operands[1];

        Register register = unit.getNextRegister();

        Type type = Types.UNKNOWN;

        try {
            type = (Type)node.getContext();
        }
        catch (Exception e) {
            System.err.println("ERROR: Couldn't resolve array operation return type");
            return null;
        }

        Size stride = getStride(type);

        instructions.append(Memory.clear(unit, register, false));
        instructions.append("lea %s, %s", register, Arrays.combine(left, right, stride.getBytes()));

        register.attach(Value.getOperation(register, stride));

        instructions.setReference(new MemoryReference(register, 0, stride));

        return instructions;
    }
}