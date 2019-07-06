package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;

import java.util.ArrayList;

public class FunctionNode extends Node {
    private Function function;
    private ArrayList<Token> body;

    public FunctionNode(Function function, ArrayList<Token> body) throws Exception {
        this.function = function;
        this.body = body;
    }

    public void parse() throws Exception {
        Parser.parse(this, function, body);
        body.clear();
    }

    public Function getFunction() {
        return function;
    }

    
}
