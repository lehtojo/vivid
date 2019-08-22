package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.types.Number;

public class Resolver {
    
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
            Node iterator = node.first();

            while (iterator != null) {
                Node resolved;

                if (iterator instanceof TypeNode) {
                    TypeNode type = (TypeNode)iterator;
                    resolved = Resolver.resolve(type.getType(), iterator, errors);
                    Resolver.resolveVariables(type.getType(), errors);
                }
                else if (iterator instanceof FunctionNode) {
                    FunctionNode function = (FunctionNode)iterator;
                    resolved = Resolver.resolve(function.getFunction(), iterator, errors);
                    Resolver.resolveVariables(function.getFunction(), errors);
                }
                else {
                    resolved = Resolver.resolve(context, iterator, errors);
                }

                if (resolved != null) {
                    iterator.replace(resolved);
                }

                iterator = iterator.next();
            }

            return node;
        }
    }

    private static Type getSharedNumber(Number a, Number b) {
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

    /**
     * Returns the shared type between the given types
     * @return Success: Shared type between the given types, Failure: null
     */
    public static Type getSharedType(Type a, Type b) {
        if (a == b) {
            return a;
        }

        if (a instanceof Number && b instanceof Number) {
            return Resolver.getSharedNumber((Number)a, (Number)b);
        }

        for (Type type : a.getSuperTypes()) {
            if (b.getSuperTypes().contains(type)) {
                return type;
            }
        }

        return Types.UNKNOWN;
    }

    /**
     * Returns the shared type between the given types
     * @param types List of types to solve
     * @return Success: Shared type between the given types, Failure: null
     */
    public static Type getSharedType(List<Type> types) {
        if (types.isEmpty()) {
            return Types.UNKNOWN;
        }
        else if (types.size() == 1) {
            return types.get(0);
        }

        Type current = types.get(0);

        for (int i = 1; i < types.size(); i++) {
            if (current == null) {
                break;
            }

            current = Resolver.getSharedType(current, types.get(i));
        }

        return current;
    }

    /**
     * Returns all child node types
     * @return Types of children nodes
     */
    public static List<Type> getTypes(Node node) {
        List<Type> types = new ArrayList<>();
        Node iterator = node.first();
        
        while (iterator != null) {
            if (iterator instanceof Contextable) {
                Contextable contextable = (Contextable)iterator;
                Context context = null;

                try {
                    context = contextable.getContext();
                }
                catch (Exception e) {
                    return null;
                }

                if (context == null || !context.isType()) {
                    return null;
                }
                else {
                    types.add((Type)context);
                }
            }

            iterator = iterator.next();
        }

        return types;
    }

    public static void resolveVariable(Variable variable) throws Exception {
        List<Type> types = new ArrayList<>();

        for (Node reference : variable.getUsages()) {
            Node parent = reference.parent();

            if (parent != null) {
                if (parent instanceof OperatorNode) {
                    OperatorNode operator = (OperatorNode)parent;

                    // Try to resolve type via contextable right side of the assign operator
                    if (operator.getOperator() == Operators.ASSIGN && 
                            operator.getRight() instanceof Contextable) {
                        
                        try {
                            Contextable contextable = (Contextable)operator.getRight();
                            Context context = contextable.getContext();
                            
                            // Verify the type is resolved
                            if (context != Types.UNKNOWN && context.isType()) {
                                types.add((Type)context);
                            }
                        }
                        catch(Exception e) {
                            continue;
                        }
                    }
                }
            }
        }

        Type shared = Resolver.getSharedType(types);

        if (shared != Types.UNKNOWN) {
            variable.setType(shared);
            return;
        }

        throw new Exception(String.format("Couldn't resolve type of variable '%s'", variable.getName()));
    }

    public static void resolveVariables(Context context, List<Exception> errors) {
        for (Variable variable : context.getVariables()) {
            if (variable.getType() == Types.UNKNOWN) {
                try {
                    resolveVariable(variable);
                }
                catch(Exception e) {
                    errors.add(e);
                }
            }
        }
    }
}