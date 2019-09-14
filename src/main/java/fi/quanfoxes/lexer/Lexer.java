package fi.quanfoxes.lexer;

import fi.quanfoxes.Errors;
import fi.quanfoxes.Keywords;

import java.util.ArrayList;
import java.util.List;

public class Lexer {
    private static final char COMMENT = '#';
    private static final char STRING = '\'';

    public static enum Type {
        UNSPECIFIED,
        TEXT,
        COMMENT,
        NUMBER,
        CONTENT,
        OPERATOR,
        STRING,
        END
    }

    public static class Area {
        public Type type;

        public String text;

        public Position start;
        public Position end;
    }

    public static class Position {
        private int line;
        private int character;

        private int absolute;

        /**
         * Creates an empty position, whose position is at zero
         */
        public Position() {
            this(0, 0, 0);
        }

        /**
         * Creates position with given properties
         * @param line Line number of the position
         * @param character Character position in the given line
         * @param absolute Character position from the start
         */
        public Position(int line, int character, int absolute) {
            this.line = line;
            this.character = character;
            this.absolute = absolute;
        }

        /**
         * Adds positions together
         * @param position Position to add to this position
         * @return This position and given position added together
         */
        public Position add(Position position) {
            return new Position(line + position.line, character + position.character, absolute + position.absolute);
        }

        /**
         * Moves to the next character, increments the line number
         */
        public Position nextLine() {
            line++;
            character = 0;
            absolute++;
            return this;
        }

        /**
         * Moves to the next character
         */
        public Position nextCharacter() {
            character++;
            absolute++;
            return this;
        }

        /**
         * Returns the line number
         * @return Line number
         */
        public int getLine() {
            return line;
        }

        /**
         * Returns the friendly line number
         * @return Friendly line number
         */
        public int getFriendlyLine() {
            return line + 1;
        }

        /**
         * Returns the character position in the current line
         * @return Character position in the current line
         */
        public int getCharacter() {
            return character;
        }

        /**
         * Returns the friendly character position
         * @return Friendly character position
         */
        public int getFriendlyCharacter() {
            return character + 1;
        }

        /**
         * Returns the character position from the start of text
         * @return Character position from the start of text
         */
        public int getAbsolute() {
            return absolute;
        }

        /**
         * Returns the friendly character position from the start of text
         * @return Friendly character position from the start of text
         */
        public int getFriendlyAbsolute() {
            return absolute + 1;
        }

        /**
         * Clones this position
         */
        public Position clone() {
            return new Position(line, character, absolute);
        }
    }

    /**
     * Returns whether the given character is an operator
     * @param c Character to test
     * @return True if given character is an operator, otherwise false
     */
    private static boolean isOperator (char c) {
        return c >= 33 && c <= 47 && c != COMMENT && c != STRING || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126;
    }

    /**
     * Returns whether the given character is a digit
     * @param c Character to test
     * @return True if given character is a digit, otherwise false
     */
    private static boolean isDigit(char c) {
        return c >= 48 && c <= 57;
    }

    /**
     * Returns whether the given character is text
     * @param c Character to test
     * @return True if given character is text, otherwise false
     */
    private static boolean isText(char c) {
        return c >= 65 && c <= 90 || c >= 97 && c <= 122 || c == 95;
    }

    /**
     * Returns whether the given character is content
     * @param c Character to test
     * @return True if given character is content, otherwise false
     */
    private static boolean isContent(char c) {
        return ParenthesisType.get(c) != null;
    }

    /**
     * Returns whether the given character is comment
     * @param c Character to test
     * @return True if given character is comment, otherwise false
     */
    private static boolean isComment(char c) {
        return c == COMMENT;
    }

    /**
     * Returns whether the given character is string
     * @param c Character to test
     * @return True if given character is string, otherwise false
     */
    private static boolean isString(char c) {
        return c == STRING;
    }

    /**
     * Returns the type of the given character
     * @param c Character to use
     * @return Type of the character
     */
    private static Type getType(char c) {

        if (isText(c)) {
            return Type.TEXT;
        }
        else if (isDigit(c)) {
            return Type.NUMBER;
        }
        else if (isContent(c)) {
            return Type.CONTENT;
        }
        else if (isOperator(c)) {
            return Type.OPERATOR;
        }
        else if (isComment(c)) {
            return Type.COMMENT;
        }
        else if (isString(c)) {
            return Type.STRING;
        }
        else if (c == '\n') {
            return Type.END;
        }

        return Type.UNSPECIFIED;
    }

    /**
     * Returns whether given character is part of the given type of text
     * @param base Type of the text before the given character
     * @param current Type of the given character
     * @param c Character to test
     * @return True if the given character is part of the text, otherwise false
     */
    private static boolean isPartOf(Type base, Type current, char c) {
        if (current == base || base == Type.UNSPECIFIED) {
            return true;
        }

        switch (base) {
            case TEXT:
                return current == Type.NUMBER;
            case NUMBER:
                return c == '.';
            default:
                return false;
        }
    }

    /**
     * Skips all spaces, starting from the given position
     * @param text Text to iterate
     * @param position Starting point in the text
     * @return Position after spaces in the text
     */
    private static Position skipSpaces(String text, Position position) {

        while (position.getAbsolute() < text.length()) {
            char c = text.charAt(position.absolute);
            
            if (!Character.isSpaceChar(c)) {
                break;
            }
            else {
                position.nextCharacter();
            }
        }

        return position;
    }

    /**
     * Skips over parenthesis content until finds the closing parenthesis
     * @param text Text to iterate
     * @param start Opening parenthesis as position in the text
     * @return Success: Position after the closing parenthesis
     * @throws Exception Throws if closing parenthesis wasn't found
     */
    private static Position skipContent(String text, Position start) throws Exception {

        Position position = start.clone();

        char opening = text.charAt(position.getAbsolute());
        char closing = ParenthesisType.get(opening).getClosing();
        
        int count = 0;

        while (position.getAbsolute() < text.length()) {
            char c = text.charAt(position.getAbsolute());

            if (c == '\n') {
                position.nextLine();
            }
            else {
                if (c == opening) {
                    count++;
                }
                else if (c == closing) {
                    count--;
                }

                position.nextCharacter();
            }

            if (count == 0) {
                return position;
            }
        }

        throw Errors.get(start, "Couldn't find closing parenthesis");
    }

    /**
     * Skips over single-line comment
     * @param text Text to iterate
     * @param start Start of the comment in the text
     * @return Position after the comment
     */
    private static Position skipComment(String text, Position start) {
        int i = text.indexOf('\n', start.getAbsolute());

        if (i != -1) {
            int length = i - start.getAbsolute();
            return new Position(start.line, start.character + length, i).nextLine();
        }
        else {
            int length = text.length() - start.getAbsolute();
            return new Position(start.line, start.character + length, text.length());
        }
    }

    /**
     * Skips over single-line string
     * @param text Text to iterate
     * @param start Start of the string in the text
     * @return Position after the string
     */
    private static Position skipString(String text, Position start) throws Exception {
        int i = text.indexOf(STRING, start.getAbsolute() + 1);
        int j = text.indexOf('\n', start.getAbsolute() + 1);

        if (i == -1 || j != -1 && j < i) {
            throw Errors.get(start, "Couldn't find the end of the string");
        }

        int length = i - start.getAbsolute();

        return new Position(start.line, start.character + length, i + 1);
    }

    /**
     * Returns the next token text area, starting from the given position
     * @param text Text to iterate
     * @param start Starting point in the text
     * @return Next token text area
     * @throws Exception 
     */
    public static Area getNextToken(String text, Position start) throws Exception {

        // Firsly the spaces must be skipped to find the next token
        Position position = skipSpaces(text, start);

        // Verify there's text to iterate
        if (position.getAbsolute() == text.length()) {
            return null;
        }

        Area area = new Area();
        area.start = position.clone();
        area.type = getType(text.charAt(position.getAbsolute()));

        switch (area.type) {

            case COMMENT: {
                area.end = skipComment(text, area.start);
                area.text = text.substring(area.start.getAbsolute(), area.end.getAbsolute());
                return area;
            }

            case CONTENT: {
                area.end = skipContent(text, area.start);
                area.text = text.substring(area.start.getAbsolute(), area.end.getAbsolute());
                return area;
            }

            case END: {
                area.end = position.clone().nextLine();
                area.text = "\n";
                return area;
            }

            case STRING: {
                area.end = skipString(text, area.start);
                area.text = text.substring(area.start.getAbsolute(), area.end.getAbsolute());
                return area;
            }

            default: break;
        }

        // Possible types are now: TEXT, NUMBER, OPERATOR
        while (position.getAbsolute() < text.length()) {

            char c = text.charAt(position.getAbsolute());

            if (isContent(c)) {

                // There cannot be number and content tokens side by side
                if (area.type == Type.NUMBER) {
                    throw Errors.get(position, "Missing operator between number and parenthesis");
                }

                break;
            }

            Type type = getType(c);

            if (!isPartOf(area.type, type, c)) {
                break;
            }

            position.nextCharacter();
        }

        area.end = position;
        area.text = text.substring(area.start.getAbsolute(), area.end.getAbsolute());

        return area;
    }

    /**
     * Converts given text into a token
     * @param text Text to convert
     * @return Text converted into a token
     */
    private static Token parseTextToken(String text) {
        if (Operators.exists(text)) {
            return new OperatorToken(text);
        }
        else if (Keywords.exists(text)) {
            return new KeywordToken(text);
        }
        else {
            return new IdentifierToken(text);
        }
    }

    /**
     * Converts given token area into a token
     * @param area Token area to convert into a token
     * @param anchor Position that the given area is relative to
     * @return Token area converted into a token
     * @throws Exception Various reasons: Unrecognized token, ...
     */
    private static Token parseToken(Area area, Position anchor) throws Exception {
        switch (area.type) {
            case TEXT:
                return parseTextToken(area.text);
            case NUMBER:
                return new NumberToken(area.text);
            case OPERATOR:
                return new OperatorToken(area.text);
            case CONTENT:
                return new ContentToken(area.text, anchor.add(area.start));
            case END:
                return new Token(TokenType.END);
            case STRING:
                return new StringToken(area.text);
            default:
                throw Errors.get(anchor.add(area.start), new Exception(String.format("Unrecognized token '%s'", area.text)));
        }
    }

    private static final int FUNCTION_LENGTH = 2;

    /**
     * Builds function tokens from identifier and content tokens
     * @param tokens List to iterate
     */
    private static void functions(List<Token> tokens) {
        if (tokens.size() < FUNCTION_LENGTH) {
            return;
        }

        for (int i = tokens.size() - 2; i >= 0;) {
            Token current = tokens.get(i);

            if (current.getType() == TokenType.IDENTIFIER) {
                Token next = tokens.get(i + 1);

                if (next.getType() == TokenType.CONTENT) {
                    ContentToken parameters = (ContentToken)next;

                    if (parameters.getParenthesisType() == ParenthesisType.PARENTHESIS) {
                        IdentifierToken name = (IdentifierToken)current;
                        FunctionToken function = new FunctionToken(name, parameters);
                    
                        tokens.set(i, function);
                        tokens.remove(i + 1);

                        i -= FUNCTION_LENGTH;
                        continue;
                    }
                }
            }

            i--;
        }
    }

    /**
     * Tokenizes given raw text
     * @param raw Raw code in text form
     * @return Tokenized code
     * @throws Exception Various reasons
     */
    public static List<Token> getTokens(String raw) throws Exception {
        return getTokens(raw, new Position());
    }

    /**
     * Tokenizes given raw text
     * @param raw Raw code in text form
     * @param anchor Position that the given text is relative to
     * @return Tokenized code
     * @throws Exception Various reasons
     */
    public static List<Token> getTokens(String raw, Position anchor) throws Exception {

        List<Token> tokens = new ArrayList<>();
        Position position = new Position();

        while (position.getAbsolute() < raw.length()) {
            Area area = getNextToken(raw, position);

            if (area == null) {
                break;
            }

            if (area.type != Type.COMMENT) {
                Token token = parseToken(area, anchor);
                token.setPosition(anchor.add(area.start));
                tokens.add(token);
            }

            position = area.end;
        }

        functions(tokens);

        return tokens;
    }
}
