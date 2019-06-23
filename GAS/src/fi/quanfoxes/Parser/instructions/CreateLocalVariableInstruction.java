package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.DataType;
import fi.quanfoxes.Parser.Instruction;

public class CreateLocalVariableInstruction extends Instruction {
    private DataType dataType;
    private String name;

    public CreateLocalVariableInstruction(DataType dataType, String name) {
        this.dataType = dataType;
        this.name = name;
    }

    public DataType getDataType() {
        return dataType;
    }

    public String getName() {
        return name;
    }
}
