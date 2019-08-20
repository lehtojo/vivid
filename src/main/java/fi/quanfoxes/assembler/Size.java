package fi.quanfoxes.assembler;

import java.util.HashMap;
import java.util.Map;

public enum Size {
    BYTE("byte", "db", 1),
    WORD("word", "dw", 2),
    DWORD("dword", "dd", 4),
    QWORD("qword", "dq", 8);

    private static Map<Integer, Size> sizes = new HashMap<>();

    static {
        for (Size size : Size.values()) {
            sizes.put(size.bytes, size);
        }
    }

    public static Size get(int bytes) {
        return sizes.get(bytes);
    }

    private String identifier;    
    private String data;

    private int bytes;

    private Size(String identifier, String data, int bytes) {
        this.identifier = identifier;
        this.data = data;
        this.bytes = bytes;
    }

    public String getIdentifier() {
        return identifier;
    }

    public String getDataIdentifier() {
        return data;
    }

    public int getBytes() {
        return bytes;
    }

    @Override
    public String toString() {
        return identifier;
    }
}