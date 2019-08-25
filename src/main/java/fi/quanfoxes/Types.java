package fi.quanfoxes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.types.*;
import fi.quanfoxes.types.Byte;
import fi.quanfoxes.types.Long;
import fi.quanfoxes.types.Short;

public class Types {
    public static final Type UNKNOWN = null;

    public static final Bool BOOL = new Bool();
    public static final Byte BYTE = new Byte();
    public static final Link LINK = new Link();
    public static final Long LONG = new Long();
    public static final Normal NORMAL = new Normal();
    public static final Short SHORT = new Short();
    public static final Tiny TINY = new Tiny();
    public static final Uint UINT = new Uint();
    public static final Ulong ULONG = new Ulong();
    public static final Ushort USHORT = new Ushort();

    public static void inject(Context context) throws Exception {
        context.declare(BOOL);
        context.declare(BYTE);
        context.declare(LINK);
        context.declare(LONG);
        context.declare(NORMAL);
        context.declare(SHORT);
        context.declare(TINY);
        context.declare(UINT);
        context.declare(ULONG);
        context.declare(USHORT);
    }
}
