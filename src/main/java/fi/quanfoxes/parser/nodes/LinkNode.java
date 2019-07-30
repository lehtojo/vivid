package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;

public class LinkNode extends OperatorNode implements Resolvable, Contextable {

    public LinkNode() {
        super(Operators.DOT);
    }

    private Context getContext(Node node) throws Exception {
        if (node instanceof Contextable) {
            Contextable contextable = (Contextable)node;
            return contextable.getContext();
        }

        throw new Exception("Couldn't resolve the context");
    }

    @Override
    public Node resolve(Context base) throws Exception {
        Node left = first();
        Node right = last();

        if (left instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)left;
            Node resolved = resolvable.resolve(base);

            if (resolved != null) {
                left.replace(resolved);
                left = resolved;
            }
        }

        if (right instanceof Resolvable) {
            Context context = getContext(left);

            Resolvable resolvable = (Resolvable)right;
            Node resolved = resolvable.resolve(context);

            if (resolved != null) {
                right.replace(resolved);
                right = resolved;
            }
        }

        return null;
    }

    @Override
    public Context getContext() throws Exception {
        Node right = last();
        return getContext(right);
    }
}