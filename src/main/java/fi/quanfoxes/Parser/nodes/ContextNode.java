package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Parser.Node;

import java.util.HashMap;

public class ContextNode extends Node {

    private HashMap<String, VariableNode> variables = new HashMap<>();
    private HashMap<String, TypeNode> types = new HashMap<>();
    private HashMap<String, FunctionNode> functions = new HashMap<>();

    public void declare(VariableNode node) throws Exception {
        if (isVariableDeclared(node.getIdentifier())) {
            throw new Exception("Variable with the same name already exists");
        }

        variables.put(node.getIdentifier(), node);
    }

    public void declare(TypeNode node) throws Exception {
        if (isTypeDeclared(node.getIdentifier())) {
            throw new Exception("Type with the same name already exists");
        }

        types.put(node.getIdentifier(), node);
    }

    public void declare(FunctionNode node) throws Exception {
        if (isFunctionDeclared(node.getIdentifier())) {
            throw new Exception("Function with the same name already exists");
        }

        functions.put(node.getIdentifier(), node);
    }

    public boolean isVariableDeclared(String identifier) {
        return variables.containsKey(identifier) ||
                (getParent() != null && ((ContextNode)getParent()).isVariableDeclared(identifier));
    }

    public boolean isTypeDeclared(String identifier) {
        return types.containsKey(identifier) ||
                (getParent() != null && ((ContextNode)getParent()).isTypeDeclared(identifier));
    }

    public boolean isFunctionDeclared(String identifier) {
        return functions.containsKey(identifier) ||
                (getParent() != null && ((ContextNode)getParent()).isFunctionDeclared(identifier));
    }

    public VariableNode getVariable(String identifier) {
        if (variables.containsKey(identifier)) {
            return variables.get(identifier);
        }
        else if (getParent() != null) {
            return ((ContextNode)getParent()).getVariable(identifier);
        }
        else {
            return null;
        }
    }

    public TypeNode getType(String identifier) {
        if (types.containsKey(identifier)) {
            return types.get(identifier);
        }
        else if (getParent() != null) {
            return ((ContextNode)getParent()).getType(identifier);
        }
        else {
            return null;
        }
    }

    public FunctionNode getFunction(String identifier) {
        if (functions.containsKey(identifier)) {
            return functions.get(identifier);
        }
        else if (getParent() != null) {
            return ((ContextNode)getParent()).getFunction(identifier);
        }
        else {
            return null;
        }
    }
}
