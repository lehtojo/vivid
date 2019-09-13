package fi.quanfoxes.assembler;

import java.util.Map;

import fi.quanfoxes.parser.Variable;

public class Register {
    private Map<Size, String> partitions;
    private Value value;

    public Register(Map<Size, String> partitions) {
        this.partitions = partitions;
    }

    private Register(Register register) {
        this.partitions = register.partitions;

        if (register.isReserved()) {
            this.value = register.value.clone(this);
        }  
    }

    public void exchange(Register register) {
        Value other = register.getValue();
        register.attach(value);
        attach(other);
    }

    public void attach(Value value) {
        this.value = value;
        this.value.setReference(this);
    }

    public boolean contains(Variable variable) {
        if (value != null && value.getValueType() == ValueType.VARIABLE) {
            return ((VariableValue)value).getVariable() == variable;
        }

        return false;
    }

    public void reset() {
        value = null;
    }

    public boolean isCritical() {
        return value != null && value.isCritical();
    }

    public boolean isAvailable() {
        return value == null;
    }

    public boolean isReserved() {
        return value != null;
    }

    public Value getValue() {
        return value;
    }

    public String getPartition(Size size) {
        return partitions.get(size);
    }

    public String getIdentifier() {
        return partitions.get(Size.DWORD);
    }

    @Override
    public String toString() {
        return partitions.get(Size.DWORD);
    }

    public Register clone() {
        return new Register(this); 
    }
}