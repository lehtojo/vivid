package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Resolver;
import fi.quanfoxes.parser.Type;

public class ReturnNode extends Node implements Resolvable {
    public ReturnNode(Node object) {
        super.add(object);
    }

    private Type getReturnType(Node node) throws Exception {
        if (node instanceof Contextable) {
            Contextable contextable = (Contextable)node;
            return (Type)contextable.getContext();
        }

        throw new Exception("Couldn't resolve the return type");
    }

    @Override
    public boolean resolve(Context context) throws Exception {
        // Returned object must be resolved first
        Resolver.resolve(context, getFirst());

        // Find the parent function where the return value can be assigned
        Function function = context.findParentFunction();
        Type type = getReturnType(getFirst());

        // Try to update the return type
        function.setReturnType(type);

        return false;
    }
}