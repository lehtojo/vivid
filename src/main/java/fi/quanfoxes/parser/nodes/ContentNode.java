package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;

public class ContentNode extends Node implements Contextable {

    @Override
    public Context getContext() throws Exception {
        Contextable contextable = (Contextable)first();
        return contextable.getContext();
    }
}
