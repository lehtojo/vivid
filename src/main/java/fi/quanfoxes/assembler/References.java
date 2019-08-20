package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class References {
    public static final Reference OBJECT_POINTER = new ParameterReference(0, 4);

    public enum ReferenceType {
        WRITE,
        READ,
        VALUE
    }

    private static Instructions getFunctionReference(Unit unit, FunctionNode node, ReferenceType type) {

        if (type == ReferenceType.WRITE) {
            System.out.println("Error: Writable function return values aren't supported yet");
        }

        return Call.build(unit, node);
    }

    private static Instructions getLinkReference(Unit unit, LinkNode node, ReferenceType type) {
        Instructions instructions = Link.build(unit, node, type);

        if (type == ReferenceType.VALUE) {
            Instructions move = Memory.toRegister(unit, instructions.getReference());
            instructions.append(move);

            return instructions.setReference(Value.getOperation(move.getReference()));
        }

        return instructions;
    }

    private static Instructions getVariableReference(Unit unit, VariableNode node, ReferenceType type) {
        Variable variable = node.getVariable();

        if (type == ReferenceType.WRITE) {
            unit.reset(variable);
        }

        if (type == ReferenceType.VALUE || type == ReferenceType.READ) {
            Register register = unit.contains(variable);

            if (register != null) {
                return new Instructions().setReference(register.getValue());
            }
        }

        Instructions instructions = new Instructions();
        Reference reference;

        switch (variable.getVariableType()) {
            
            case GLOBAL: {
                reference = new DataSectionReference(variable.getFullname(), variable.getType().getSize());
                break;
            }

            case LOCAL: {
                reference = new LocalVariableReference(variable.getAlignment(), variable.getType().getSize());
                break;
            }

            case PARAMETER: {
                reference = new ParameterReference(variable.getAlignment(), variable.getType().getSize());
                break;
            }

            case MEMBER: {
                Register register = type == ReferenceType.WRITE ? unit.edi : unit.esi;

                if (!unit.isObjectPointerLoaded(register)) {
                    instructions.append(Memory.move(unit, OBJECT_POINTER, Reference.from(register)));
                    instructions.setReference(Value.getObjectPointer(register));
                }

                reference = new MemberReference(variable.getAlignment(), variable.getType().getSize(), type == ReferenceType.WRITE);
                break;
            }

            default: {
                return null;
            }
        }

        if (type == ReferenceType.VALUE && reference.isComplex()) {
            Instructions move = Memory.toRegister(unit, reference);
            instructions.append(move);

            return instructions.setReference(Value.getVariable(move.getReference(), variable));
        }
        
        return instructions.setReference(reference);
    }

    private static Instructions getNumberReference(Unit unit, NumberNode node, ReferenceType type) {
        switch (type) {
            case WRITE:
                return new Instructions().setReference(new AddressReference(node.getValue()));
            case READ:
                return new Instructions().setReference(new NumberReference(node.getValue()));
            case VALUE:
                Instructions instructions = Memory.toRegister(unit, new NumberReference(node.getValue()));
                instructions.setReference(Value.getNumber(instructions.getReference().getRegister()));
                return instructions;
        }

        return null;
    }

    private static Instructions get(Unit unit, Node node, ReferenceType type) {
        if (node instanceof FunctionNode) {
            return getFunctionReference(unit, (FunctionNode)node, type);
        }
        else if (node instanceof LinkNode) {
            return getLinkReference(unit, (LinkNode)node, type);
        }
        else if (node instanceof VariableNode) {
            return getVariableReference(unit, (VariableNode)node, type);
        }
        else if (node instanceof NumberNode) {
            return getNumberReference(unit, (NumberNode)node, type);
        }
        else if (node instanceof ContentNode) {
            return get(unit, node.first(), type);
        }
        else {

            if (type == ReferenceType.WRITE) {
                System.out.println("Warning: Too complex write requested");
            }

            return unit.assemble(node);
        }
    }

    public static Instructions write(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.WRITE);
    }

    public static Instructions read(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.READ);
    }

    public static Instructions value(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.VALUE);
    }
}