package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.references.*;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.StringNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class References {
    public static enum ReferenceType {
        DIRECT,
        READ,
        VALUE,
        REGISTER
    }

    public static MemoryReference getObjectPointer(Unit unit) {
        return MemoryReference.parameter(unit, 0, 4);
    }

    private static Instructions getFunctionReference(Unit unit, FunctionNode node, ReferenceType type) {

        if (type == ReferenceType.DIRECT) {
            System.err.println("ERROR: Writable function return values aren't supported");
        }
        
        return Call.build(unit, node);
    }

    private static Instructions getLinkReference(Unit unit, LinkNode node, ReferenceType type) {
        return Link.build(unit, node, type);
    }

    /**
     * Returns a reference to a variable
     * @param node Variable as node
     * @param type Type of reference to get
     * @return Reference to a variable
     */
    private static Instructions getVariableReference(Unit unit, VariableNode node, ReferenceType type) {
        Variable variable = node.getVariable();

        if (type == ReferenceType.DIRECT) {
            unit.reset(variable);
        }
        else {
            Reference cache = unit.cached(variable);

            if (cache != null) {

                // Variable may be cached in stack
                if (!cache.isRegister()) {
                    Instructions move = Memory.toRegister(unit, cache);
                    return move.setReference(Value.getVariable(move.getReference(), variable));
                }

                return Instructions.reference(cache);
            }
        }

        Instructions instructions = new Instructions();
        Reference reference;

        switch (variable.getVariableType()) {
            
            case GLOBAL: {
                reference = ManualReference.global(variable.getFullname(), Size.get(variable.getType().getSize()));
                break;
            }

            case LOCAL: {
                reference = MemoryReference.local(unit, variable.getAlignment(), variable.getType().getSize());
                break;
            }

            case PARAMETER: {
                reference = MemoryReference.parameter(unit, variable.getAlignment(), variable.getType().getSize());
                break;
            }

            case MEMBER: {
                Register register = unit.getObjectPointer();

                if (register == null) {
                    Instructions move = Memory.getObjectPointer(unit, type);
                    instructions.append(move);

                    register = move.getReference().getRegister();
                }

                reference = MemoryReference.member(register, variable.getAlignment(), variable.getType().getSize());
                break;
            }

            default: {
                return null;
            }
        }

        if ((type == ReferenceType.VALUE || type == ReferenceType.REGISTER) && reference.isComplex()) {
            Instructions move = Memory.toRegister(unit, reference);
            instructions.append(move);

            return instructions.setReference(Value.getVariable(move.getReference(), variable));
        }
        
        return instructions.setReference(reference);
    }

    private static Instructions getNumberReference(Unit unit, NumberNode node, ReferenceType type) {
        Size size = Size.get(node.getType().getSize());

        switch (type) {
            case DIRECT:
                return Instructions.reference(new AddressReference(node.getValue()));
            case VALUE:
            case READ:
                return Instructions.reference(new NumberReference(node.getValue(), size));
            case REGISTER:
                Instructions instructions = Memory.toRegister(unit, new NumberReference(node.getValue(), size));
                instructions.setReference(Value.getNumber(instructions.getReference().getRegister(), size));
                return instructions;
        }

        return null;
    }

    private static Instructions getStringReference(Unit unit, StringNode node, ReferenceType type) {
        switch (type) {
            
            case DIRECT: {
                return Instructions.reference(ManualReference.string(node.getIdentifier(), true));
            }

            case READ: {
                return Instructions.reference(ManualReference.string(node.getIdentifier(), false));
            }

            case VALUE:
            case REGISTER: {
                Reference reference = ManualReference.string(node.getIdentifier(), false);
                Instructions instructions = Memory.toRegister(unit, reference);

                return instructions.setReference(Value.getString(instructions.getReference().getRegister()));
            }

            default: return null;
        }
    }

    public static Instructions get(Unit unit, Node node, ReferenceType type) {
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
        else if (node instanceof StringNode) {
            return getStringReference(unit, (StringNode)node, type);
        }
        else {

            if (type == ReferenceType.DIRECT) {
                System.out.println("Warning: Complex writable reference requested");
            }

            return unit.assemble(node);
        }
    }

    public static Instructions direct(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.DIRECT);
    }

    public static Instructions read(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.READ);
    }

    public static Instructions value(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.VALUE);
    }

    public static Instructions register(Unit unit, Node node) {
        return References.get(unit, node, ReferenceType.REGISTER);
    }

    /**
     * Returns whether node is primitive (Requires only building references)
     */
    private static boolean isPrimitive(Node node) {
        return node instanceof VariableNode || node instanceof StringNode || node instanceof NumberNode;
    }

    /**
     * Returns references to both given nodes
     * @param program Program to append the instructions for referencing the nodes
     * @param a Node a
     * @param b Node b
     * @param at Node a's reference type
     * @param bt Node a'b reference type
     * @return References to both of the nodes
     */
    public static Reference[] get(Unit unit, Instructions program, Node a, Node b, ReferenceType at, ReferenceType bt) {
        Reference[] references = new Reference[2];

        if (!isPrimitive(a)) {
            Instructions instructions = References.get(unit, a, at);
            references[0] = instructions.getReference();

            program.append(instructions);
        }

        if (!isPrimitive(b)) {
            Instructions instructions = References.get(unit, b, bt);
            references[1] = instructions.getReference();

            program.append(instructions);
        }

        if (references[0] == null) {
            Instructions instructions = References.get(unit, a, at);
            references[0] = instructions.getReference();

            program.append(instructions);
        }
        
        if (references[1] == null) {
            Instructions instructions = References.get(unit, b, bt);
            references[1] = instructions.getReference();

            program.append(instructions);
        }

        return references;
    }
}