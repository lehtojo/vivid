package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;

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
    public abstract Node build(Node parent, List<Token> tokens) throws Exception;

    public ArrayList<Integer> getPath() {
        return path;
    }
}
