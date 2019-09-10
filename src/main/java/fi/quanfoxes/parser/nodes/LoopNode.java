package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class LoopNode extends Node {
    private Context context;

    public LoopNode(Context context, Node condition, Node body) {
        this.context = context;

        if (condition != null) {
            super.add(condition);
        }

        super.add(body);
    }

    /**
     * Returns the private context of the while statement
     * @return Private context of the while statement
     */
    public Context getContext() {
        return context;
    }

    /**
     * Returns whether this loop loops forever
     * @return True if this loop loops forever, otherwise false
     */
    public boolean isForever() {
        return first() == last();
    }

    /**
     * Returns the node that is executed when program enters the while statement
     * @return Node that is executed when program enters the while statement
     */
    public Node getStart() {
        return first().first();
    }

    /**
     * Returns the node that represents the condition of the statement
     * @return
     */
    public Node getCondition() {
        return getStart().next();
    }

    /**
     * Returns the node that represents the action that is done in the end of each cycle in the while statement
     * @return Node that represents the action that is done in the end of each cycle in the while statement
     */
    public Node getAction() {
        return first().last();
    }

    /**
     * Returns the node that is executed each cycle in the while statement
     * @return Node that is executed each cycle in the while statement
     */
    public Node getBody() {
        return last();
    }
}
