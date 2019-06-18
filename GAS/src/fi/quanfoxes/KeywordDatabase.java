package fi.quanfoxes;

import java.util.ArrayList;
import java.util.List;

public class KeywordDatabase {
    private static List<Keyword> keywords = new ArrayList<>();

    public static void initialize () {
        keywords.add(new Keyword("base"));
        keywords.add(new Keyword("break"));
        keywords.add(new Keyword("case"));
        keywords.add(new Keyword("continue"));
        keywords.add(new Keyword("else"));
        keywords.add(new Keyword("extern"));
        keywords.add(new Keyword("func"));
        keywords.add(new Keyword("goto"));
        keywords.add(new Keyword("if"));
        keywords.add(new Keyword("lock"));
        keywords.add(new Keyword("loop"));
        keywords.add(new Keyword("new"));
        keywords.add(new Keyword("return"));
        keywords.add(new Keyword("this"));
        keywords.add(new Keyword("type"));
    }

    public static boolean exists (String name) {
        return keywords.stream().anyMatch(k -> k.getName().equals(name));
    }

    public static Keyword get(String text) {
        return keywords.stream().filter(k -> k.getName().equals(text)).findFirst().get();
    }
}
