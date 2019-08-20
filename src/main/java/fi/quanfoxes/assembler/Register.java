package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Variable;

public class Register {
    private String identifier;
    private Value value;

    private int size;

    public Register(String identifier, int size) {
        this.identifier = identifier;
        this.size = size;
    }

    private Register(Register register) {
        this.identifier = register.identifier;
        this.size = register.size;

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
        if (value != null && value.getType() == ValueType.VARIABLE) {
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

    public int getSize() {
        return size;
    }

    @Override
    public String toString() {
        return identifier;
    }

    public Register clone() {
        return new Register(this); 
    }
}