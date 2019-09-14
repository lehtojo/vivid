package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Type;

import java.util.ArrayList;
import java.util.List;

public class TypeNode extends Node implements Contextable {
    private Type type;
    private List<Token> body;

    public TypeNode(Type type) {
        this(type, new ArrayList<>());
    }

    public TypeNode(Type type, List<Token> body) {
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

    @Override
    public Context getContext() {
        return type;
    }

    @Override
    public NodeType getNodeType() {
        return NodeType.TYPE_NODE;
    }
}
