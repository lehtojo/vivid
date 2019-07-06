package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Token;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public abstract class Pattern {
    private ArrayList<Integer> path;

    public Pattern(Integer... path) {
        this.path = new ArrayList<>(Arrays.asList(path));
    }

    public abstract int priority(List<Token> tokens);
    public abstract boolean passes(List<Token> tokens);
    public abstract Node build(Context context, List<Token> tokens) throws Exception;

    public int start() {
        return 0;
    }

    public int end() {
        return -1;
    }

    public ArrayList<Integer> getPath() {
        return path;
    }
}
