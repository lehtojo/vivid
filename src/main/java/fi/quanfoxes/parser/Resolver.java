package fi.quanfoxes.parser;

import java.util.List;

import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.types.Number;

public class Resolver {

    /*

        TODO: Resolve functions returns new node tree on success, otherwise null 

        */

    /**
    * Tries to resolve any unresolved nodes in a node tree
    * @param context Context to use when resolving
    * @param node Node tree
    * @param errors Output list for errors
    * @return Returns a resolved node tree on success, otherwise null
    */
    public static Node resolve(Context context, Node node, List<Exception> errors) {
        if (node instanceof Resolvable) {
            Resolvable resolvable = (Resolvable)node;

            try {
                Node resolved = resolvable.resolve(context);
                return resolved == null ? node : resolved;
            }
            catch (Exception e) {
                errors.add(e);
            }

            return null;
        }
        else {
            Node iterator = node.getFirst();

            while (iterator != null) {
                Node resolved;

                if (iterator instanceof TypeNode) {
                    TypeNode type = (TypeNode)iterator;
                    resolved = Resolver.resolve(type.getType(), iterator, errors);
                }
                else if (iterator instanceof FunctionNode) {
                    FunctionNode function = (FunctionNode)iterator;
                    resolved = Resolver.resolve(function.getFunction(), iterator, errors);
                }
                else {
                    resolved = Resolver.resolve(context, iterator, errors);
                }

                if (resolved != null) {
                    iterator.replace(resolved);
                }

                iterator = iterator.getNext();
            }

            return node;
        }
    }

    private static Context getSharedNumber(Number a, Number b) {
        return a.getBitCount() > b.getBitCount() ? a : b;
    }

    public static Context getSharedContext(Context a, Context b) {
        if (a instanceof Number && b instanceof Number) {
            return getSharedNumber((Number)a, (Number)b);
        }

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

    /*public static Context getSharedContext(List<Context> contexts) {
        if (contexts.size() < 2) {
            return contexts.get(0);
        }

        Context target = contexts.get(0);
        
        while (target != null) {
            
            for (int i = 1; i < contexts.size(); i++) {
                Context context = contexts.get(i);

                if (context == target) {

                }
            }

            target = target.getParent();
        }

        return null;
    }*/

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