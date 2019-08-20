package fi.quanfoxes.assembler;

public class Instruction {
    private String assembly;

    public static Instruction unsafe(String command, Reference left, Reference right) {
        if (left.isComplex()) {
            return new Instruction(String.format("%s %s %s, %s", command, left.getSize(), left.peek(), right.peek()));
        }
        else if (right.isComplex()) {
            return new Instruction(String.format("%s %s, %s %s", command, left.peek(), right.getSize(), right.peek()));
        } 
        else {
            return new Instruction(String.format("%s %s, %s", command, left.peek(), right.peek()));
        }
    }

    public Instruction (String command, Reference left, Reference right) {
        if (left.isComplex()) {
            assembly = String.format("%s %s %s, %s", command, left.getSize(), left.use(), right.use());
        }
        else if (right.isComplex()) {
            assembly = String.format("%s %s, %s %s", command, left.use(), right.getSize(), right.use());
        } 
        else {
            assembly = String.format("%s %s, %s", command, left.use(), right.use());
        }
    }

    public Instruction (String command, Reference operand) {
        if (operand.isComplex()) {
            assembly = String.format("%s %s %s", command, operand.getSize(), operand.use());
        }
        else {
            assembly = String.format("%s %s", command, operand.use());
        }
    }

    public Instruction (String command) {
        assembly = command;
    }

    @Override
    public String toString() {
        return assembly;
    }
}