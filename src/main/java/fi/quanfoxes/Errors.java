package fi.quanfoxes;

import fi.quanfoxes.lexer.Lexer.Position;

public class Errors {
    public static Exception get(Position position, Exception exception) {
        return new Exception(String.format("Line: %d, Character: %d | %s", position.getLine(), position.getCharacter(), exception.getMessage()));
    }
}