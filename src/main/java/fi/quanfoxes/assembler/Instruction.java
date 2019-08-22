package fi.quanfoxes.assembler;

public class Instruction {
    private String assembly;

    public static Instruction unsafe(String command, Reference left, Reference right) {
        return new Instruction(String.format("%s %s, %s", command, left.peek(), right.peek()));
    }

    public Instruction (String command, Reference left, Reference right) {
        assembly = String.format("%s %s, %s", command, left.use(), right.use());
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