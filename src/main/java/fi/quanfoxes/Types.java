package fi.quanfoxes;

import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.nodes.TypeNode;
import fi.quanfoxes.types.*;
import fi.quanfoxes.types.Byte;
import fi.quanfoxes.types.Long;
import fi.quanfoxes.types.Short;

public class Types {

    public static void add(ContextNode root, TypeNode type) throws Exception {
        root.declare(type);
        root.add(type);
    }

    public static void inject(ContextNode root) throws Exception {
        add(root, new Byte());
        add(root, new Long());
        add(root, new Normal());
        add(root, new Short());
        add(root, new Tiny());
        add(root, new Uint());
        add(root, new Ulong());
        add(root, new Ushort());
    }
}
