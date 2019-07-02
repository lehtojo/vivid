package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Parser;

import java.util.ArrayList;

public class FunctionNode extends ContextNode {
    private String identifier;
    private int access;

    private Node parameters;
    private int count = 0;

    private ArrayList<Token> body;

    public FunctionNode(String identifier, int access, Node parameters, ArrayList<Token> body) throws Exception {
        this.identifier = identifier;
        this.access = access;
        this.parameters = parameters;
        this.body = body;

        VariableNode parameter = (VariableNode)parameters.getFirstChild();

        while (parameter != null) {
            declare(parameter);

            count++;
            parameter = (VariableNode)parameter.getNext();
        }
    }

    public void parse() throws Exception {
        Parser.parse(this, body);
    }

    public Node getParameters() {
        return parameters;
    }

    public int getParameterCount() {
        return count;
    }

    public String getIdentifier() {
        return identifier;
    }

    public int getAccessModifiers() {
        return access;
    }
}
