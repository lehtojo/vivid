package fi.quanfoxes.parser;

/**
 * Allows contexting nodes
 */
public interface Contextable {
    public Context getContext() throws Exception;
}