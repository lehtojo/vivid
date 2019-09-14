package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;

public class VariableNode extends Node implements Contextable {
    private Variable variable;

    public VariableNode(Variable variable) {
        this.variable = variable;
        this.variable.addUsage(this);
    }

    public Variable getVariable() {
        return variable;
    }

    @Override
    public Context getContext() throws Exception {
        return variable.getType();
    }
    
    @Override
    public NodeType getNodeType() {
        return NodeType.VARIABLE_NODE;
    }
}
