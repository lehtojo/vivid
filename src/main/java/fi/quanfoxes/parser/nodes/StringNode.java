package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.Types;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;

public class StringNode extends Node implements Contextable {
    private String text;
    private String identifier;

    public StringNode(String text) {
        this.text = text;
    }

    public void setIdentifier(String identifier) {
        this.identifier = identifier;
    }

    public String getIdentifier() {
        return identifier;
    }

    public String getText() {
        return text;
    }

    @Override
    public Context getContext() {
        return Types.LINK;
	}
}