package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class UnresolvedFunction extends Node implements Resolvable {
    private String value;

    public UnresolvedFunction(String value) {
        this.value = value;
    }

    public UnresolvedFunction setParameters(Node parameters) {
        Node parameter = parameters.getFirst();

        while (parameter != null) {
            super.add(parameter);
            parameter = parameter.getNext();
        }

        return this;
    }

    public Node try_resolve(Context context) throws Exception {
        if (context.isFunctionDeclared(value)) {
            return new FunctionNode(context.getFunction(value));
        }
        else {
            throw new Exception("Couldn't resolve function");
        }
    }

    @Override
    public boolean resolve(Context context) throws Exception {
        Node resolved = try_resolve(context);
        replaceWith(resolved);
        return true;
    }
}