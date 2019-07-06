package fi.quanfoxes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.types.*;
import fi.quanfoxes.types.Byte;
import fi.quanfoxes.types.Long;
import fi.quanfoxes.types.Short;

public class Types {

    public static void inject(Context context) throws Exception {
        new Byte(context);
        new Long(context);
        new Normal(context);
        new Short(context);
        new Tiny(context);
        new Uint(context);
        new Ulong(context);
        new Ushort(context);
    }
}
