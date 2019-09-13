package fi.quanfoxes.assembler;

public class Instruction {
    private String assembly;

    public static Instruction unsafe(String command, Reference left, Reference right, Size size) {
        return new Instruction(String.format("%s %s, %s", command, left.peek(size), right.peek(size)));
    }

    public Instruction (String command, Reference left, Reference right, Size size) {
        assembly = String.format("%s %s, %s", command, left.use(size), right.use(size));
    }

    public Instruction (String command, Reference operand) {
        assembly = String.format("%s %s", command, operand.use(operand.getSize()));
    }

    public Instruction (String command) {
        assembly = command;
    }

    @Override
    public String toString() {
        return assembly;
    }
}