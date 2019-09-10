package fi.quanfoxes.parser.nodes;

import java.util.ArrayList;
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

    public Node solve(Context environment, Context context) throws Exception {
        Node node = getParameters();

        if (node != null) {
            Node parameter = first();

            while (parameter != null) {
                Node resolved = Resolver.resolve(environment, parameter, new ArrayList<>());

                if (resolved != null) {
                    parameter.replace(resolved);
                    parameter = resolved.next();
                }
                else {
                    parameter = parameter.next();
                }
            }
        }

        List<Type> parameters = Resolver.getTypes(this); 

        if (parameters == null) {
            throw new Exception(String.format("Couldn't resolve function parameters '%s'", value));
        }

        Function function = Singleton.getFunctionByName(context, value, parameters);

        if (function == null) {
            throw new Exception(String.format("Couldn't resolve function '%s'", value));
        }

        return new FunctionNode(function).setParameters(this);
    }

    private Node getParameters() {
        return first();
    }

    @Override
    public Node resolve(Context context) throws Exception {
        return solve(context, context);
    }
}