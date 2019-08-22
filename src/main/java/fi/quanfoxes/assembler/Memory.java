package fi.quanfoxes.assembler;

import java.util.ArrayList;
import java.util.List;

public class Memory {
    public static Instructions relocate(Unit unit, Value value) {
        if (unit.isAnyRegisterAvailable() || unit.isAnyRegisterUncritical()) {
            Register register = unit.getNextRegister();

            Reference destination = Reference.from(register);

            Instructions instructions = new Instructions();
            instructions.append(Instruction.unsafe("mov", destination, value));
            instructions.setReference(destination);

            register.attach(value);

            return instructions;
        }

        System.out.println("Error: Too complex relocation");
        return null;
    }

    public static Instructions move(Unit unit, Reference source, Reference destination) {
        Instructions instructions = new Instructions();

        if (destination.isRegister()) {
            Register register = destination.getRegister();

            if (register.isReserved()) {
                Value value = register.getValue();
                instructions.append(Memory.relocate(unit, value));

                register.reset();
            }
        }

        instructions.append(Instruction.unsafe("mov", destination, source));
        instructions.setReference(destination);

        return instructions;
    }

    public static Instructions toRegister(Unit unit, Reference reference) {
        Instructions instructions = new Instructions();
        Register register = unit.getNextRegister();

        if (register.isReserved()) {
            Value value = register.getValue();
            instructions.append(Memory.relocate(unit, value));
        }

        Reference destination = Reference.from(register);
        instructions.append(Instruction.unsafe("mov", destination, reference));
        instructions.setReference(destination);

        return instructions;
    }

    public static Instruction exchange(Unit unit, Register a, Register b) {
        a.exchange(b);
        return new Instruction(String.format("xchg %s, %s", a, b));
    }

    public static class Evacuation {
        public List<Value> values = new ArrayList<>();

        public void add(Value value) {
            values.add(value);
        }

        public void start(Instructions instructions) {
            for (Value value : values) {
                instructions.append(new Instruction(String.format("push %s", value.getRegister())));
            }
        }

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

        public boolean isNecessary() {
            return values.size() > 0;
        }
    }

    public static Evacuation evacuate(Unit unit) {
        Evacuation evacuation = new Evacuation();

        for (Register register : unit.getRegisters()) {
            if (register.isCritical()) {
                evacuation.add(register.getValue());
            }
        }

        return evacuation;
    }

	public static Instructions clear(Unit unit, Reference reference) {
        Instructions instructions = new Instructions();

		if (reference.isRegister()) {
            Register register = reference.getRegister();
            Value value = register.getValue();
            
            instructions.append(Memory.relocate(unit, value));
        }

        return instructions;
	}
}