package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class ElseIfNode extends IfNode {
    public ElseIfNode(Context context, Node condition, Node body) {
		super(context, condition, body);
	}
}