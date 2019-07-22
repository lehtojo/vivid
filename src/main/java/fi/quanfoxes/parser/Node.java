package fi.quanfoxes.parser;

import java.util.Objects;

public class Node {
    private Node parent;

    private Node previous;
    private Node next;

    private Node first;
    private Node last;

    public Node getParent() {
        return parent;
    }

    public Node getPrevious() {
        return previous;
    }

    public Node getNext() {
        return next;
    }

    public Node getFirst() {
        return first;
    }

    public Node getLast() {
        return last;
    }

    public void insert(Node position, Node child) {
        if (position == first) {
            first = child;
        }

        Node left = position.previous;
        Node right = position.next;

        if (left != null) {
            left.next = child;
        }

        if (right != null) {
            right.previous = child;
        }

        child.parent = position.parent;
        child.previous = left;
        child.next = right;
    }

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

    public void remove(Node child) throws Exception {
        if (child.parent != this) {
            throw new Exception("Given node isn't a child node of this node");
        }

        Node left = child.previous;
        Node right = child.next;

        if (left != null) {
            left.next = right;
        }

        if (right != null) {
            right.previous = left;
        }
    }

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
}
