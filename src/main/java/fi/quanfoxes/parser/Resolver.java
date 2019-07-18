package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Resolver {

    public static void resolve(Context context, Node root) {
        Node iterator = root.getFirst();

        while (iterator != null) {
            if (iterator instanceof Resolvable) {
                Resolvable resolvable = (Resolvable) iterator;

                try {
                    if (!resolvable.resolve(context)) {
                        iterator = iterator.getNext();
                        continue;
                    }
                } catch (Exception e) {
                    // Resolve functions are intended to fail
                }
            }

            Resolver.resolve(context, iterator);

            iterator = iterator.getNext();
        }
    }

    public static Context getSharedContext(Context a, Context b) {
        Context context = a;
        
        while (context != null) {
            Context iterator = b;

            while (iterator != null) {
                
                if (iterator == context) {
                    return context;
                }
                
                iterator = iterator.getParent();
            }

            context = context.getParent();
        }

        return null;
    }

    public static Context getSharedContext(List<Context> contexts) {
        Context target = contexts.get(0);
        
        while (target != null) {
            
            for (int i = 1; i < contexts.size(); i++) {
                
            }

            target = target.getParent();
        }

        return null;
    }

    /*private static Type getNodeReturnType(Node node) {
        Type type = Types.UNKNOWN;
        
        if (node instanceof FunctionNode) {
            Function function = ((FunctionNode)node).getFunction();

            if (function.getReturnType() != Types.UNKNOWN) {
                type = function.getReturnType();
            }
        }
        else if (node instanceof VariableNode) {
            VariableNode variable = (VariableNode)node;
            type = variable.getVariable().getType();
        }
        else if (node instanceof OperatorNode) {
            OperatorNode operator = (OperatorNode)node;

            if (operator.getOperator() == OperatorType.DOT) {

                if (operator.getRight() instanceof FunctionNode) {
                    Function function = ((FunctionNode)operator.getRight()).getFunction();

                    if (function.getReturnType() != Types.UNKNOWN) {
                        type = function.getReturnType();
                    }
                }
                else if (operator.getRight() instanceof VariableNode) {
                    VariableNode variable = (VariableNode)operator.getRight();
        
                    if (variable.getVariable() != null) {
                        type = variable.getVariable().getType();
                    }
                }
            }
        }

        return type;
    }

    private static Type getAssignType(OperatorNode assign) throws Exception {
        Node left = assign.getLeft();
        Node right = assign.getRight();

        if (left instanceof FunctionNode) {
            throw new Exception("Function cannot be assigned");
        }
        else if (left instanceof OperatorNode) {
            // TODO: a = b = c, a += b += c, ...
            throw new Exception("Operator result cannot be assigned");
        }

        Type type = getNodeReturnType(left);

        if (type == Types.UNKNOWN) {
            type = getNodeReturnType(right);
        }

        return type;
    }

    private static void resolve(Variable variable) throws Exception {
        List<Type> types = new ArrayList<>();
        
        for (Node usage : variable.getUsages()) {
            Node parent = usage.getParent();
            Type type = Types.UNKNOWN;

            if (parent instanceof OperatorNode) {
                OperatorNode operator = (OperatorNode)parent;

                if (operator.getOperator() == OperatorType.ASSIGN) {
                    type = getAssignType(operator);
                }
            }

            if (type != Types.UNKNOWN) {
                types.add(type);
            }
        }

        for (Type type : types) {
            
        }
    }

    public static void resolve(Context context, Node root) throws Exception {
        for (Variable variable : context.getVariables()) {
            if (variable.getType() == Types.UNKNOWN) {
                resolve(variable);
            }
        }
    }*/
}