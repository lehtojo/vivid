package fi.quanfoxes;

import fi.quanfoxes.lexer.Lexer.Position;

public class Errors {
    public static Exception get(Position position, Exception exception) {
        return new Exception(String.format("Line: %d, Character: %d | %s", position.getFriendlyLine(), position.getFriendlyCharacter(), exception.getMessage()));
    }

    public static Exception get(Position position, String exception) {
        return new Exception(String.format("Line: %d, Character: %d | %s", position.getFriendlyLine(), position.getFriendlyCharacter(), exception));
    }

    public static Exception get(Position position, String format, Object... args) {
        return new Exception(String.format("Line: %d, Character: %d | %s", position.getFriendlyLine(), position.getFriendlyCharacter(), String.format(format, args)));
    }
}