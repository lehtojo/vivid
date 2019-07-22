package fi.quanfoxes.parser;

import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.UnresolvedIdentifier;

public class UnresolvedType extends Type implements Resolvable {
    private Resolvable resolvable;

    public UnresolvedType(Context context, String name) {
        super(context);
        this.resolvable = new UnresolvedIdentifier(name);
    }

    public UnresolvedType(Context context, Resolvable resolvable) throws Exception {
        super(context);
        this.resolvable = resolvable;
    }

    @Override
    public Node resolve(Context context) throws Exception {
        Node resolved = resolvable.resolve(context);

        if (resolved instanceof Contextable) {
            Contextable contextable = (Contextable)resolved;
            return new TypeNode((Type)contextable.getContext());
        }

        throw new Exception("Couldn't resolve type");
    }
}