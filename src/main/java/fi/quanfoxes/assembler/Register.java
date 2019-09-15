package fi.quanfoxes.assembler;

import java.util.Map;

import fi.quanfoxes.parser.Variable;

public class Register {
    private Map<Size, String> partitions;
    private Value value;

    /**
     * Creates a register with partitions
     * @param partitions Partitions of the register (for example: rax, eax, ax, al)
     */
    public Register(Map<Size, String> partitions) {
        this.partitions = partitions;
    }

    /**
     * Copies the state of the given register
     * @param register Register to copy
     */
    private Register(Register register) {
        partitions = register.partitions;

        if (register.isReserved()) {
            value = register.value.clone(this);
        }  
    }

    /**
     * Exchanges values between the given register
     */
    public void exchange(Register register) {
        Value other = register.getValue();
        register.attach(value);
        attach(other);
    }

    /**
     * Attaches value to this register
     * @param value Value to attach
     */
    public void attach(Value value) {
        this.value = value;
        this.value.setReference(this);
    }

    /**
     * Returns whether this register contains the given variable
     * @param variable Variable to test
     * @return True if this register holds the variable, otherwise false
     */
    public boolean contains(Variable variable) {
        if (value != null && value.getValueType() == ValueType.VARIABLE) {
            return ((VariableValue)value).getVariable() == variable;
        }

        return false;
    }

    /**
     * Removes the current value from this register if it's present
     */
    public void reset() {
        value = null;
    }

    /**
     * Returns whether the held value is critical currently
     * @return True if the value held in this register is currently critical, otherwise false
     */
    public boolean isCritical() {
        return value != null && value.isCritical();
    }

    /**
     * Returns whether this register doesn't hold a value
     * @return True if this register doesn't hold any value currently, otherwise false
     */
    public boolean isAvailable() {
        return value == null;
    }

    /**
     * Returns whether this register holds a value
     * @return True if this register holds a value currently, otherwise false
     */
    public boolean isReserved() {
        return value != null;
    }

    /**
     * Returns the value that is held in this register
     * @return Value of this register
     */
    public Value getValue() {
        return value;
    }

    /**
     * Returns partition that represents the given size
     * @param size Size of the partition
     * @return Partition that represents the given size
     */
    public String getPartition(Size size) {
        return partitions.get(size);
    }

    /**
     * Returns the default identifier of this register
     * @return Default register identifier
     */
    public String getIdentifier() {
        return partitions.get(Size.DWORD);
    }

    @Override
    public String toString() {
        return partitions.get(Size.DWORD);
    }

    /**
     * Clones the state of this register
     */
    public Register clone() {
        return new Register(this); 
    }
}