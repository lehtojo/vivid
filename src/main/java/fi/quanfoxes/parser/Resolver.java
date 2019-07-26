package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.parser.nodes.FunctionNode;
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
     * Returns the shared type between the two given types
     * @return Success: Shared type between the two given types, Failure: null
     */
    public static Type getSharedType(Type a, Type b) {
        if (a instanceof Number && b instanceof Number) {
            return getSharedNumber((Number)a, (Number)b);
        }

        Type type = a;
        
        while (type != null) {
            Type iterator = b;

            while (iterator != null) {
                
                if (iterator == type) {
                    return type;
                }
                
                Context parent = iterator.getParent();

                if (!parent.isType()) {
                    return null;
                }

                iterator = (Type)parent;
            }

            Context parent = type.getParent();

            if (!parent.isType()) {
                return null;
            }

            type = (Type)parent;
        }

        return null;
    }

    /**
     * Returns all child node types
     * @return Types of children nodes
     * @throws Exception Throws if any child isn't contextable
     */
    public static List<Type> getTypes(Node node) throws Exception {
        List<Type> types = new ArrayList<>();
        Node iterator = node.first();
        
        while (iterator != null) {
            if (iterator instanceof Contextable) {
                Contextable contextable = (Contextable)iterator;
                Context context = contextable.getContext();

                if (context == null || !context.isType()) {
                    throw new Exception("Couldn't resolve type");
                }
                else {
                    types.add((Type)context);
                }
            }

            iterator = iterator.next();
        }

        return types;
    }
}