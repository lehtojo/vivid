package fi.quanfoxes.parser.nodes;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolver;

public class ContentNode extends Node implements Contextable {

    @Override
    public Context getContext() throws Exception {
        List<Context> contexts = new ArrayList<>(); 
        
        Node iterator = getFirst();

        while (iterator != null) {
            Contextable contextable = (Contextable)iterator.getFirst();
            contexts.add(contextable.getContext());

            iterator = iterator.getNext();
        }

        return Resolver.getSharedContext(contexts);
    }
}
