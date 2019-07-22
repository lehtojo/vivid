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
        Node node = getFirst();

        if (node instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)node;
            Node resolved = resolvable.resolve(context);

            node.replace(resolved);
            node = resolved;
        }
        
        // Find the parent function where the return value can be assigned
        Function function = context.getFunctionContext();

        Type current = function.getReturnType();
        Type type = getReturnType(node);

        if (type == null) {
            throw new Exception("Couldn't resolve return type");
        }
        
        if (current != type) {
            if (current != null) {
                type = (Type)Resolver.getSharedContext(current, type);
            }

            if (type == null) {
                throw new Exception("Invalid return type");
            }
    
            function.setReturnType(type);
        }
        
        return null;
    }
}