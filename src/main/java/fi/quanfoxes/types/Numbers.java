package fi.quanfoxes.types;

import java.util.HashMap;
import java.util.Map;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.NumberType;

public class Numbers {
    private static Map<NumberType, Number> numbers = new HashMap<>();

    public static Number getType(NumberType type) {
        return numbers.get(type);
    }

    private static void add(Number number) {
        numbers.put(number.getNumberType(), number);
    }

    static {
        add(Types.BYTE);
        add(Types.LONG);
        add(Types.NORMAL);
        add(Types.SHORT);
        add(Types.TINY);
        add(Types.UINT);
        add(Types.ULONG);
        add(Types.USHORT);
    }
}