package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Type;

public class ConstructionNode extends Node implements Resolvable, Contextable {

    public ConstructionNode(Node constructor) {
        super.add(constructor);
    }

    /**
     * Returns potential custom constructor of the type
     * @return Potential constructor of the type to create, otherwise null
     */
    public Function getConstructor() {
        Node node = first();

        if (node instanceof FunctionNode) {
            FunctionNode constructor = (FunctionNode)node;
            return constructor.getFunction();
        }

        return null;
    }

    /**
     * Returns the type to create
     * @return Type to create by the constructor
     */
    public Type getType() {
        Function constructor = getConstructor();

        if (constructor != null) {
            return constructor.getTypeParent();
        }

        return null;
    }

    @Override
    public Node resolve(Context context) throws Exception {
        Node constructor = first();

        if (constructor instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)constructor;
            Node resolved = resolvable.resolve(context);

            constructor.replace(resolved);
        }

        return null;
    }

    @Override
    public Context getContext() throws Exception {
        return getType();
	}
}