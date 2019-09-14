package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;

import java.util.ArrayList;
import java.util.List;

public class FunctionNode extends Node implements Contextable {
    private Function function;
    private List<Token> body;

    public FunctionNode(Function function) throws Exception {
        this(function, new ArrayList<>());
    }

    public FunctionNode(Function function, List<Token> body) throws Exception {
        this.function = function;
        this.function.addUsage(this);
        this.body = body;
    }

    public void parse() throws Exception {
        Node node = Parser.parse(function, body, Parser.MIN_PRIORITY, Parser.MEMBERS - 1);
        add(node);
        
        body.clear();
    }

    public FunctionNode setParameters(Node parameters) {
        Node parameter = parameters.first();

        while (parameter != null) {
            Node next = parameter.next();
            add(parameter);
            parameter = next;
        }

        return this;
    }

    public Function getFunction() {
        return function;
    }

    public Node getParameters() {
        return first();
    }

    public Node getBody() {
        return last();
    }

    @Override
    public Context getContext() {
        return function.getReturnType();
    }

    @Override
    public NodeType getNodeType() {
        return NodeType.FUNCTION_NODE;
    }
}
