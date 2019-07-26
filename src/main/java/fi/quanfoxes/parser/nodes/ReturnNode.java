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

        throw new Exception("Couldn't resolve return type");
    }

    @Override
    public Node resolve(Context context) throws Exception {
        // Returned object must be resolved first
        Node node = first();

        if (node instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)node;
            Node resolved = resolvable.resolve(context);

            node.replace(resolved);
            node = resolved;
        }
        
        // Find the parent function where the return value can be assigned
        Function function = context.getFunctionParent();

        Type current = function.getReturnType();
        Type type = getReturnType(node);

        if (type == null) {
            throw new Exception("Couldn't resolve return type");
        }
        
        if (current != type) {
            Type shared = type;

            if (current != null) {
                shared = Resolver.getSharedType(current, type);
            }

            if (shared == null) {
                throw new Exception(String.format("Type '%s' isn't compatible with the current return type '%s'", type.getName(), current.getName()));
            }
    
            function.setReturnType(type);
        }
        
        return null;
    }
}