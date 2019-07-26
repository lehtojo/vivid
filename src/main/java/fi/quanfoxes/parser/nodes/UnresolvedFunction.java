package fi.quanfoxes.parser.nodes;

import java.util.List;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Resolver;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.Type;

public class UnresolvedFunction extends Node implements Resolvable {
    private String value;

    public UnresolvedFunction(String value) {
        this.value = value;
    }

    public UnresolvedFunction setParameters(Node parameters) {
        Node parameter = parameters.first();

        while (parameter != null) {
            Node next = parameter.next();
            super.add(parameter);
            parameter = next;
        }

        return this;
    }

    public Node getResolvedNode(Context context) throws Exception {
        List<Type> parameters = Resolver.getTypes(this); 
        Function function = Singleton.getFunctionByName(context, value, parameters);

        if (function == null) {
            throw new Exception(String.format("Couldn't resolve function '%s'", value));
        }

        return new FunctionNode(function).setParameters(this);
    }

    @Override
    public Node resolve(Context context) throws Exception {
        return getResolvedNode(context);
    }
}