package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Variable;

public class VariableValue extends Value {
    private Variable variable;

    public VariableValue(Register register, Variable variable) {
        super(register, Size.get(variable.getType().getSize()), ValueType.VARIABLE, true, false, true);
        this.variable = variable;
    }

    private VariableValue(VariableValue value) {
        super(value);
        this.variable = value.variable;
    }

    @Override
    public Value clone(Register register) {
        VariableValue clone = new VariableValue(this);
        register.attach(clone);
        return clone;
    }

    public Variable getVariable() {
        return variable;
    }
}