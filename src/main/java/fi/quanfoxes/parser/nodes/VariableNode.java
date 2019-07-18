package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;

public class VariableNode extends Node {
    private Variable variable;

    public VariableNode(Variable variable) {
        this.variable = variable;
        this.variable.addUsage(this);
    }

    public Variable getVariable() {
        return variable;
    }
}
