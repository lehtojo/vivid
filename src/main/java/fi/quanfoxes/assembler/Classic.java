package fi.quanfoxes.assembler;

import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Classic {
    private static boolean isPrimitive(Node node) {
        return node instanceof VariableNode || node instanceof NumberNode;
    }

    private static Instructions build(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions left = null;
        Instructions right = null;

        if (isPrimitive(node.getLeft()) && isPrimitive(node.getRight())) {
            left = References.value(unit, node.getLeft());
            right = References.read(unit, node.getRight());

            instructions.append(left, right);
        }
        else {

            if (!isPrimitive(node.getRight())) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (!isPrimitive(node.getLeft())) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }

            if (right == null) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (left == null) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }
        }

        instructions.append(new Instruction(instruction, left.getReference(), right.getReference()));
        instructions.setReference(Value.getOperation(left.getReference()));

        return instructions;
    }

    private static Instructions divide(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions left = null;
        Instructions right = null;

        if (isPrimitive(node.getLeft()) && isPrimitive(node.getRight())) {
            left = References.value(unit, node.getLeft());
            right = References.read(unit, node.getRight());

            instructions.append(left, right);
        }
        else {

            if (!isPrimitive(node.getRight())) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (!isPrimitive(node.getLeft())) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }

            if (right == null) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (left == null) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }
        }

        if (left.getReference().getRegister() != unit.eax && right.getReference().getRegister() == unit.eax) {
            instructions.append(Memory.exchange(unit, left.getReference().getRegister(), right.getReference().getRegister()));
        }
        else if (left.getReference().getRegister() != unit.eax) {
            instructions.append(Memory.move(unit, left.getReference(), Reference.from(unit.eax)));
        }      

        if (right.getReference().getRegister() != unit.edx && unit.edx.isCritical()) {
            instructions.append("push edx");
            instructions.append("xor edx, edx");
            instructions.append(new Instruction(instruction, right.getReference()));
            instructions.append("pop edx");
        }
        else {
            instructions.append("xor edx, edx");
            instructions.append(new Instruction(instruction, right.getReference()));
        }
        
        instructions.setReference(Value.getOperation(Reference.from(unit.eax)));

        return instructions;
    }

    public static Instructions power(Unit unit, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions left = null;
        Instructions right = null;

        if (isPrimitive(node.getLeft()) && isPrimitive(node.getRight())) {
            left = References.read(unit, node.getLeft());
            right = References.read(unit, node.getRight());

            instructions.append(left, right);
        }
        else {

            if (!isPrimitive(node.getRight())) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (!isPrimitive(node.getLeft())) {
                left = References.read(unit, node.getLeft());
                instructions.append(left);
            }

            if (right == null) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (left == null) {
                left = References.read(unit, node.getLeft());
                instructions.append(left);
            }
        }

        Instructions call = Call.build(unit, null, "function_ipow", left.getReference(), right.getReference());
        instructions.append(call);
        instructions.setReference(call.getReference());

        return instructions;
    }

    /*private static Instructions multiply(Unit unit, String instruction, OperatorNode node) {
        Instructions instructions = new Instructions();

        Instructions left = null;
        Instructions right = null;

        if (isPrimitive(node.getLeft()) && isPrimitive(node.getRight())) {
            left = References.value(unit, node.getLeft());
            right = References.read(unit, node.getRight());

            instructions.append(left, right);
        }
        else {
            
            if (!isPrimitive(node.getRight())) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (!isPrimitive(node.getLeft())) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }

            if (right == null) {
                right = References.read(unit, node.getRight());
                instructions.append(right);
            }
            if (left == null) {
                left = References.value(unit, node.getLeft());
                instructions.append(left);
            }
        }

        Reference reference = Reference.from(unit.eax);

        if (left.getReference().getRegister() == unit.eax) {
            reference = right.getReference();
        }
        else if (right.getReference().isRegister() && right.getReference().getRegister() == unit.eax) {
            reference = left.getReference();
        }
        else {
            instructions.append(Memory.move(unit, left.getReference(), reference));
            reference = right.getReference();
        }

        if (unit.edx.isCritical()) {
            instructions.append(Memory.relocate(unit, unit.edx.getValue()));
        }

        instructions.append(new Instruction(instruction, reference));
        instructions.setReference(Value.getOperation(Reference.from(unit.eax)));

        return instructions;
    }*/

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

        return null;
    }
}