package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Lexer.NumberType;
import fi.quanfoxes.Parser.Node;

public class NumberNode extends Node {
    private NumberType type;
    private Number value;

    public NumberNode(NumberType type, Number value) {
        this.type = type;
        this.value = value.longValue();
    }

    public NumberType getType() {
        return type;
    }

    public Number getValue() {
        return value;
    }
}
