package fi.quanfoxes.assembler;

import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.assembler.references.MemoryReference;
import fi.quanfoxes.assembler.references.RegisterReference;

public class Memory {
    /**
     * Relocates a value to different register or to stack
     * @param value Value to relocate
     * @return Instructions for relocating the value
     */
    public static Instructions relocate(Unit unit, Value value) {
        if (!value.isCritical()) {
            System.err.println("BUG: Uncritical values shouldn't be relocated!");
            return new Instructions();
        }

        if (unit.isAnyRegisterUncritical()) {
            Register register = unit.getNextRegister();

            Reference destination = new RegisterReference(register, value.getSize());

            Instructions instructions = new Instructions();
            instructions.append(Instruction.unsafe("mov", destination, value, value.getSize()));
            instructions.setReference(destination);

            register.attach(value);

            return instructions;
        }

        System.out.println("Error: Too complex relocation");
        return null;
    }

    /**
     * Moves value from source to destination. If destination contains a value, it's relocated only if it's critical
     * @return Instructions for moving data from source to destination
     */
    public static Instructions move(Unit unit, Reference source, Reference destination) {
        Instructions instructions = new Instructions();

        if (destination.isRegister()) {
            Register register = destination.getRegister();

            if (register.isCritical()) {
                Value value = register.getValue();
                instructions.append(Memory.relocate(unit, value));

                register.reset();
            }
        }

        instructions.append(Instruction.unsafe("mov", destination, source, source.getSize()));
        instructions.setReference(destination);

        return instructions;
    }

    /**
     * Moves data from reference to some register. If no register is available, some register will be cleared and used
     * @return Instructions for moving data from reference to some register
     */
    public static Instructions toRegister(Unit unit, Reference reference) {
        Instructions instructions = new Instructions();
        Register register = unit.getNextRegister();

        if (register.isCritical()) {
            Value value = register.getValue();
            instructions.append(Memory.relocate(unit, value));
        }

        Reference destination = new RegisterReference(register, reference.getSize());
        instructions.append(Instruction.unsafe("mov", destination, reference, destination.getSize()));
        instructions.setReference(destination);

        return instructions;
    }

    /**
     * Exchanges the values of two registers
     * @return Instructions for exchanging value between registers
     */
    public static Instruction exchange(Unit unit, Register a, Register b) {
        a.exchange(b);
        return new Instruction(String.format("xchg %s, %s", a, b));
    }

    /**
     * Creates a register evacuation based on the given unit
     * @param unit Unit to evacuate
     * @return Instructions for evacuating the registers of the given unit
     */
    public static Evacuation evacuate(Unit unit) {
        Evacuation evacuation = new Evacuation();
    
        for (Register register : unit.getRegisters()) {
            if (register.isCritical()) {
                evacuation.add(register.getValue());
            }
        }
    
        return evacuation;
    }

    /**
     * Clears the register using xor instruction. If the register contains a value, it's relocated only if it's critical
     * @param register Register to clear
     * @return Instructions for clearing the register properly
     */
	public static Instructions clear(Unit unit, Register register, boolean zero) {
        Instructions instructions = new Instructions();

		if (register.isCritical()) {
            instructions.append(Memory.relocate(unit, register.getValue()));
        }

        Reference reference = new RegisterReference(register);

        if (zero) {
            instructions.append(new Instruction("xor", reference, reference, reference.getSize()));
        }

        register.reset();
        
        return instructions;
    }
    
    /**
     * Copies a copy of object pointer to some register
     * @param type Usage type of the object pointer
     * @return Instructions for copying object pointer to some register
     */
    public static Instructions getObjectPointer(Unit unit, ReferenceType type) {
        Instructions instructions = new Instructions();
        Register register = null;

        // Try to get the appropriate register for the object pointer
        if (type == ReferenceType.DIRECT) {
            if (!unit.edi.isCritical()) {
                register = unit.edi;
            }
        }
        else if (!unit.esi.isCritical()) {
            register = unit.esi;
        }

        // When the approriate register cannot be used, any uncritical register will be chosen
        if (register == null) {
            register = unit.getNextRegister();
        }

        // Relocate possible value from the chosen register since the register is now reserved for the object pointer
        if (register.isCritical()) {
            instructions.append(Memory.relocate(unit, register.getValue()));
        }

        // Move a copy of the object pointer to the chosen register
        instructions.append(new Instruction("mov", new RegisterReference(register), MemoryReference.parameter(unit, 0, 4), Size.DWORD));
        instructions.setReference(Value.getObjectPointer(register));

        return instructions;
    }
}