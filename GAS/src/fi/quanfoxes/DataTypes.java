package fi.quanfoxes;

import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.nodes.TypeNode;
import fi.quanfoxes.types.*;
import fi.quanfoxes.types.Byte;
import fi.quanfoxes.types.Long;
import fi.quanfoxes.types.Short;

public class DataTypes {
    //private static Map<String, DataType> types = new HashMap<>();

    public static void add(TypeNode type) throws Exception {
        Parser.getRoot().declare(type);
        Parser.getRoot().add(type);
    }

    /*public static boolean exists (final String name) {
        return types.containsKey(name);
    }

    public static DataType get (final String name) {
        return types.get(name);
    }*/

    public static void initialize() throws Exception{
        add(new Byte());
        add(new Long());
        add(new Normal());
        add(new Short());
        add(new Tiny());
        add(new Uint());
        add(new Ulong());
        add(new Ushort());
    }
}
