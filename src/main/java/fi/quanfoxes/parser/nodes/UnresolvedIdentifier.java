package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class UnresolvedIdentifier extends Node implements Resolvable {
    private String value;

    public UnresolvedIdentifier(String value) {
        this.value = value;
    }

    public Node try_resolve(Context context) throws Exception {
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
    public boolean resolve(Context context) throws Exception {
        Node resolved = try_resolve(context);
        replaceWith(resolved);
        return true;
    }
}