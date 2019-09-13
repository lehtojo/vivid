package fi.quanfoxes.assembler;

import fi.quanfoxes.assembler.references.*;
import fi.quanfoxes.parser.Variable;

public class Value extends Reference {
    private Reference reference;
    private ValueType type;

    private boolean critical;
    private boolean disposable;
    private boolean floating;

    public Value(Register register, Size size, ValueType type, boolean critical, boolean disposable, boolean floating) {
        super(size);
        this.type = type;
        this.critical = critical;
        this.disposable = disposable;
        this.floating = floating;

        register.attach(this);
    }

    protected Value(Value value) {
        super(value.getSize());
        this.type = value.type;
        this.critical = value.critical;
        this.disposable = value.disposable;
        this.floating = value.floating;
    }

    public Value clone(Register register) {
        Value clone = new Value(this);
        register.attach(clone);
        return clone;
    }

    public ValueType getValueType() {
        return type;
    }

    public Reference getReference() {
        return reference;
    }

    public void setReference(Register register) {
        this.reference = new RegisterReference(register, size);
    }

    public void setCritical(boolean critical) {
        this.critical = critical;
	}

    public boolean isCritical() {
        return critical;
    }

    @Override
    public String toString() {
        return reference.toString();
    }

    @Override
    public boolean isRegister() {
        return reference.isRegister();
    }

    @Override
    public Register getRegister() {
        return reference.getRegister();
    }

    @Override
    public String peek(Size size) {
        return reference.use(size);
    }

    @Override
    public String use(Size size) {
        if (disposable && reference.isRegister()) {
            Register register = ((RegisterReference)reference).getRegister();
            register.reset();
        }

        if (floating) {
            critical = false;
        }

        return reference.use(size);
    }

    @Override
    public LocationType getType() {
        return LocationType.VALUE;
    }

    public static Value getObjectPointer(Register register) {
        return new Value(register, Size.DWORD, ValueType.OBJECT_POINTER, false, false, false);
    }

    public static Value getOperation(Register register, Size size) {
        return new Value(register, size, ValueType.OPERATION, true, true, false);
    }

    public static Value getNumber(Register register, Size size) {
        return new Value(register, size, ValueType.NUMBER, true, false, true);
    }

    public static Value getString(Register register) {
        return new Value(register, Size.DWORD, ValueType.STRING, true, false, true);
    }

    public static Value getVariable(Reference reference, Variable variable) {
        if (reference.isRegister()) {
            return new VariableValue(reference.getRegister(), variable);
        }

        return null;
    }
}