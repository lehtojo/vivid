package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.nodes.OperatorNode;

public class Classic {

    private static Instructions build(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.REGISTER, ReferenceType.READ);

        instructions.append(new Instruction(instruction, operands[0], operands[1]));
        instructions.setReference(Value.getOperation(operands[0].getRegister(), Size.DWORD));

        return instructions;
    }

    private static Instructions divide(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.REGISTER, ReferenceType.REGISTER);

        Reference left = operands[0];
        Reference right = operands[1];

        if (left.getRegister() != unit.eax && right.getRegister() == unit.eax) {
            instructions.append(Memory.exchange(unit, left.getRegister(), right.getRegister()));
        }
        else if (left.getRegister() != unit.eax) {
            instructions.append(Memory.move(unit, left, Reference.from(unit.eax)));
        }

        instructions.append(Memory.clear(unit, unit.edx, true));
        
        instructions.append(new Instruction(instruction, right));
        instructions.setReference(Value.getOperation(unit.eax, Size.DWORD));

        return instructions;
    }

    public static Instructions power(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Reference[] operands = References.get(unit, instructions, node.getLeft(), node.getRight(), ReferenceType.READ, ReferenceType.READ);

        Reference left = operands[0];
        Reference right = operands[1];

        Instructions call = Call.build(unit, null, "function_ipow", Size.DWORD, left, right);
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
            return Classic.divide(unit, "idiv", node);
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

        return null;
    }
}