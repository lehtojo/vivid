package fi.quanfoxes.assembler;

import java.util.ArrayList;
import java.util.List;

public class Evacuation {
    public List<Value> values = new ArrayList<>();

    /**
     * Adds value to evacuation list
     * @param value Value to evacuate
     */
    public void add(Value value) {
        values.add(value);
    }

    /**
     * Appends instructions to evacuate values
     * @param instructions Where instructions should be appended
     */
    public void start(Instructions instructions) {
        for (Value value : values) {
            instructions.append(new Instruction(String.format("push %s", value.getRegister())));
        }
    }

    /**
     * Restores evacuated values to registers
     * @param instructions Where instructions should be appended
     */
    public void restore(Unit unit, Instructions instructions) {
        for (int i = values.size() - 1; i >= 0; i--) {
            Value value = values.get(i);
            Register register = value.getRegister();

            if (!register.isCritical()) {
                register.attach(value);
            }
            else {
                register = unit.getNextRegister();
                register.attach(value);
            }

            instructions.append(new Instruction(String.format("pop %s", register)));
        }
    }

    /**
     * @return True if the evacuation should be executed, otherwise false
     */
    public boolean necessary() {
        return values.size() > 0;
    }
}