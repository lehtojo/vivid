package fi.quanfoxes.parser;

import java.util.Objects;

import fi.quanfoxes.parser.nodes.NodeType;

public class Node {
    private Node parent;

    private Node previous;
    private Node next;

    private Node first;
    private Node last;

    /**
     * Inserts the given node before the given node position
     * @param position Given node will be inserted before this node
     * @param child Node to insert
     */
    public void insert(Node position, Node child) {
        if (position == first) {
            first = child;
        }

        Node left = position.previous;

        if (left != null) {
            left.next = child;
        }

        position.previous = child;

        if (child.parent != null) {
            child.parent.remove(child);
        }
        
        child.parent = position.parent;
        child.previous = left;
        child.next = position;
    }

    /**
     * Adds the given node to the end
     * @param child Node to add
     */
    public void add(Node child) {
        child.parent = this;
        child.previous = last;
        child.next = null;

        if (first == null) {
            first = child;
        }

        if (last != null) {
            last.next = child;
        }

        last = child;
    }

    /**
     * Removes a child node
     * @param child Child node to remove
     * @return True if removal succeeded, otherwise false
     */
    public boolean remove(Node child) {
        if (child.parent != this) {
            return false;
        }

        Node left = child.previous;
        Node right = child.next;

        if (left != null) {
            left.next = right;
        }

        if (right != null) {
            right.previous = left;
        }

        return true;
    }

    /**
     * Replaces this node with another node
     * @param node Node that will replace this node
     */
    public void replace(Node node) {
        Node iterator = first;

        // Update the parent of each child
        while (iterator != null) {
            iterator.parent = node;
            iterator = iterator.next;
        }

        if (previous == null) {
            // Since this node is the first, parent must be updated
            if (parent != null) {
                parent.first = node;  
            }
        }
        else {
            // Update the previous node
            previous.next = node;
        }

        if (next == null) {
            // Since this node is the last, parent must be updated
            if (parent != null) {
                parent.last = node;
            }
        }
        else {
            // Update the next node
            next.previous = node;
        }

        node.previous = previous;
        node.next = next;
    }

    /**
     * Moves all child nodes from the given node to this node. Lastly, destroyes the given node
     * @param node Node to merge
     */
    public void merge(Node node) {
        Node iterator = node.first;

        while (iterator != null) {
            Node next = iterator.next;
            add(iterator);
            iterator = next;
        }

        node.destroy();
    }

    /**
     * Unsafely disconnect this node from others
     */
    public void destroy() {
        parent = null;
        previous = null;
        next = null;
        first = null;
        last = null;
    }

    /**
     * Disconnects this node from the parent
     * @return Reference to this node
     */
    public Node disconnect() {
        parent.remove(this);
        return this;
	}

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Node node = (Node) o;
        return Objects.equals(next, node.next) &&
                Objects.equals(first, node.first);
    }

    @Override
    public int hashCode() {
        return Objects.hash(next, first);
    }

    @Override
    public String toString() {
        return "Node{" + "\n" +
                "children:\n" + first +
                "},\n" +
                next.toString();
    }

    /**
     * Returns the next node
     */
    public Node next() {
        return next;
    }

    /**
     * Returns the previous node
     * @return Previous node
     */
    public Node previous() {
        return previous;
    }

    /**
     * Returns the parent node
     * @return Parent node
     */
    public Node parent() {
        return parent;
    }

    /**
     * Returns the first child node
     * @return First child node
     */
    public Node first() {
        return first;
    }

    /**
     * Returns the last child node
     * @return Last child node
     */
    public Node last() {
        return last;
    }

    /**
     * Returns the type of this node
     * @return Type of this node
     */
    public NodeType getNodeType()
    {
        return NodeType.NORMAL;
    }
}
