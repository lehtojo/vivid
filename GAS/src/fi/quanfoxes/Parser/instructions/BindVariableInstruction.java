package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Parser.Instruction;

public class BindVariableInstruction extends Instruction {
    private String name;

    public BindVariableInstruction(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }
}
