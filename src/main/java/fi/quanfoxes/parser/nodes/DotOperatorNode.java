package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class DotOperatorNode extends OperatorNode implements Resolvable, Contextable {

    public DotOperatorNode() {
        super(OperatorType.DOT);
    }

    private Context getContext(Node node) throws Exception {
        if (node instanceof Contextable) {
            Contextable contextable = (Contextable)node;
            return contextable.getContext();
        }

        throw new Exception("Couldn't resolve the context");
    }

    public Node resolve(Context context, Node node) throws Exception {
        if (node instanceof UnresolvedIdentifier) {
            UnresolvedIdentifier id = (UnresolvedIdentifier)node;
            return id.try_resolve(context);
        }
        else if (node instanceof UnresolvedFunction) {
            UnresolvedFunction function = (UnresolvedFunction)node;
            return function.try_resolve(context);
        }
        else {
            return node;
        }
    }

    @Override
    public boolean resolve(Context base) throws Exception {
        Node left = getFirst();
        Node resolvedLeft = resolve(base, left);
        
        Context context = getContext(resolvedLeft);

        Node right = getLast();
        Node resolvedRight = resolve(context, right);

        left.replaceWith(resolvedLeft);
        right.replaceWith(resolvedRight);

        return false;
    }

    @Override
    public Context getContext() throws Exception {
        Node right = getLast();
        return getContext(right);
    }
}