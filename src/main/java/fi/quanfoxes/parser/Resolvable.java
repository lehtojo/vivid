package fi.quanfoxes.parser;

public interface Resolvable {
    public Node resolve(Context context) throws Exception;
}