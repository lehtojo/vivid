package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.assembler.references.RegisterReference;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.OperatorNode;

public class Classic {

    private static Type getOperationType(OperatorNode node) {
        try {
            return (Type)node.getContext();
        }
        catch (Exception e) {
            System.err.println("Error: " + e.toString());
            System.exit(-1);
            return null;
        }
    }

    private static Instructions build(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();
        
        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.REGISTER, ReferenceType.READ);

        Type type = getOperationType(node);
        Size size = Size.get(type.getSize());

        instructions.append(new Instruction(instruction, operands[0], operands[1], size));
        instructions.setReference(Value.getOperation(operands[0].getRegister(), size));

        return instructions;
    }

    private static Instructions divide(Unit unit, String instruction, OperatorNode node, boolean remainder) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.REGISTER, ReferenceType.REGISTER);

        Reference left = operands[0];
        Reference right = operands[1];

        if (left.getRegister() != unit.eax && right.getRegister() == unit.eax) {
            instructions.append(Memory.exchange(unit, left.getRegister(), right.getRegister()));
        }
        else if (left.getRegister() != unit.eax) {
            instructions.append(Memory.move(unit, left, new RegisterReference(unit.eax)));
        }

        instructions.append(Memory.clear(unit, unit.edx, true));
        
        Type type = getOperationType(node);
        Size size = Size.get(type.getSize());

        instructions.append(new Instruction(instruction, right));
        instructions.setReference(Value.getOperation(remainder ? unit.edx : unit.eax, size));

        if (remainder) {
            unit.eax.reset();
        }

        return instructions;
    }

    public static Instructions power(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.READ, ReferenceType.READ);

        Reference left = operands[0];
        Reference right = operands[1];

        Instructions call = Call.build(unit, null, "function_integer_power", Size.DWORD, left, right);
        instructions.append(call);
        
        return instructions.setReference(call.getReference());
    }

    public static Instructions build(Unit unit, OperatorNode node) {
        if (node.getOperator() == Operators.ADD) {
            return Classic.build(unit, "add", node);
        }
        else if (node.getOperator() == Operators.SUBTRACT) {
            return Classic.build(unit, "sub", node);
        }
        else if (node.getOperator() == Operators.MULTIPLY) {
            return Classic.build(unit, "imul", node);
        }
        else if (node.getOperator() == Operators.DIVIDE) {
            return Classic.divide(unit, "idiv", node, false);
        }
        else if (node.getOperator() == Operators.POWER) {
            return Classic.power(unit, node);
        }
        else if (node.getOperator() == Operators.AND) {
            return Classic.build(unit, "and", node);
        }
        else if (node.getOperator() == Operators.EXTENDER) {
            return Arrays.build(unit, node);
        }
        else if (node.getOperator() == Operators.MODULUS) {
            return Classic.divide(unit, "idiv", node, true);
        }

        return null;
    }
}