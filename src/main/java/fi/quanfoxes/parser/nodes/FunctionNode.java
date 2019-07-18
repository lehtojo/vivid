package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;

import java.util.ArrayList;

public class FunctionNode extends Node implements Contextable {
    private Function function;
    private ArrayList<Token> body;

    public FunctionNode(Function function) throws Exception {
        this(function, new ArrayList<>());
    }

    public FunctionNode(Function function, ArrayList<Token> body) throws Exception {
        this.function = function;
        this.function.addUsage(this);
        this.body = body;
    }

    public void parse() throws Exception {
        Parser.parse(this, function, body);
        body.clear();
    }

    public FunctionNode setParameters(Node parameters) {
        Node parameter = parameters.getFirst();

        while (parameter != null) {
            super.add(parameter);
            parameter = parameter.getNext();
        }

        return this;
    }

    public Function getFunction() {
        return function;
    }

    @Override
    public Context getContext() {
        return function.getReturnType();
    }
}
