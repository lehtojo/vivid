package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class CastNode extends Node implements Contextable, Resolvable {

    public CastNode(Node object, Node type) {
        super.add(object);
        super.add(type);
    }

    @Override
    public Context getContext() throws Exception {
        Node type = last();

        if (type instanceof Contextable) {
            Contextable contextable = (Contextable)type;
            return contextable.getContext();
        }
        
        return null;
    }

    private void resolve(Context context, Node node) throws Exception {
        if (node instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)node;
            Node resolved = resolvable.resolve(context);

            node.replace(resolved);
        }
    }

    @Override
    public Node resolve(Context context) throws Exception {
        resolve(context, first());
        resolve(context, last());

        return null;
    }
}