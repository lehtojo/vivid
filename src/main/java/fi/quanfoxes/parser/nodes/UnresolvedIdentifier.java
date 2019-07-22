package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class UnresolvedIdentifier extends Node implements Resolvable {
    private String value;

    public UnresolvedIdentifier(String value) {
        this.value = value;
    }

    public Node getResolvedNode(Context context) throws Exception {
        if (context.isTypeDeclared(value)) {
            return new TypeNode(context.getType(value));
        }
        else if (context.isVariableDeclared(value)) {
            return new VariableNode(context.getVariable(value));
        }
        else {
            throw new Exception("Couldn't resolve identifier");
        }
    }

    @Override
    public Node resolve(Context context) throws Exception {
        return getResolvedNode(context);
    }
}