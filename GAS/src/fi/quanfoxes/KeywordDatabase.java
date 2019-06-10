package fi.quanfoxes;

import java.util.ArrayList;
import java.util.List;

public class KeywordDatabase {
    private static List<Keyword> keywords = new ArrayList<>();

    public static boolean exists (String name) {
        return keywords.stream().anyMatch(k -> k.getName().equals(name));
    }

    public static Keyword get(String text) {
        return keywords.stream().filter(k -> k.getName().equals(text)).findFirst().get();
    }
}
