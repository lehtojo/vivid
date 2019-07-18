package fi.quanfoxes.parser;

import fi.quanfoxes.parser.nodes.UnresolvedIdentifier;

public class UnresolvedType extends Type {
    private Node node;

    public UnresolvedType(Context context, String name) {
        super(context);
        this.node = new UnresolvedIdentifier(name);
    }

    public UnresolvedType(Context context, Node node) throws Exception {
        super(context);
        this.node = node;
    }

    public Type resolve() throws Exception {
        Resolver.resolve(this, node);

        if (node instanceof Contextable) {
            Contextable contextable = (Contextable)node;
            return (Type)contextable.getContext();
        }

        throw new Exception("Couldn't resolve type");
    }
}