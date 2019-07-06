package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Type;

import java.util.ArrayList;

public class TypeNode extends Node {
    private Type type;
    private ArrayList<Token> body;

    public TypeNode(Type type, ArrayList<Token> body) {
        this.type = type;
        this.body = body;
    }

    public void parse() throws Exception {
        Parser.parse(this, type, body, Parser.MEMBERS);
        body.clear();
    }

    public Type getType() {
        return type;
    }
}
