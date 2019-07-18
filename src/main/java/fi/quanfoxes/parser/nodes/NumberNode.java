package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.NumberType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.types.Number;
import fi.quanfoxes.types.Numbers;

public class NumberNode extends Node implements Contextable {
    private Number type;
    private java.lang.Number value;

    public NumberNode(NumberType type, java.lang.Number value) {
        this.type = Numbers.getType(type);
        this.value = value.longValue();
    }

    public Number getType() {
        return type;
    }

    public java.lang.Number getValue() {
        return value;
    }

    @Override
    public Context getContext() throws Exception {
        return type;
    }
}
