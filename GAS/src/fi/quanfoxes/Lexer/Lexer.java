package fi.quanfoxes.Lexer;

import fi.quanfoxes.DataTypeDatabase;
import fi.quanfoxes.KeywordDatabase;

import java.util.ArrayList;
import java.util.List;

public class Lexer {

    private static final String OPENING_PARENTHESIS = "{[(";
    private static final String CLOSING_PARENTHESIS  = "}])";

    public enum TextType {
        UNSPECIFIED,
        TEXT,
        NUMBER,
        CONTENT,
        OPERATOR
    }

    public static class TokenArea {

        public TextType type;

        public String text;

        public int start;
        public int end;
    }

    private static boolean isOperator (char c) {
        return c >= 33 && c <= 47 && c != 46 || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126;
    }

    private static boolean isDigit(char c) {
        return c >= 48 && c <= 57;
    }

    private static boolean isText(char c) {
        return c >= 65 && c <= 90 || c >= 97 && c <= 122 || c == 46 || c == 95;
    }

    private static boolean isContent (char c) {
        return OPENING_PARENTHESIS.indexOf(c) > 0;
    }

    private static TextType getType (char c) {

        if (isText(c)) {
            return TextType.TEXT;
        }
        else if (isDigit(c)) {
            return TextType.NUMBER;
        }
        else if (isContent(c)) {
            return TextType.CONTENT;
        }
        else if (isOperator(c)) {
            return TextType.OPERATOR;
        }

        return TextType.UNSPECIFIED;
    }

    private static boolean isPartOf (TextType baseType, TextType currentType, char c) {
        if (currentType == baseType || baseType == TextType.UNSPECIFIED) {
            return true;
        }

        switch (baseType) {
            case TEXT:
                return currentType == TextType.NUMBER;
            case NUMBER:
                return c == '.';
            default:
                return false;
        }
    }

    private static int skipSpaces(String text, int position) {

        while (position < text.length() && Character.isSpaceChar(text.charAt(position))) {
            position++;
        }

        return position;
    }

    private static int skipContent(String text, int start) throws Exception {

        int count = 0;
        int position = start;

        final char opening = text.charAt(position);
        final char closing = CLOSING_PARENTHESIS.charAt(OPENING_PARENTHESIS.indexOf(opening));

        while (position < text.length()) {
            final char c = text.charAt(position);

            if (c == opening) {
                count++;
            }
            else if (c == closing) {
                count--;
            }

            position++;

            if (count == 0) {
                return position;
            }
        }

        throw new Exception("Couldn't find closing parenthesis");
    }

    public static TokenArea getNextTokenArea(String text, int start) throws Exception {

        var position = skipSpaces(text, start);

        final TokenArea area = new TokenArea();
        area.start = position;
        area.type = getType(text.charAt(position));

        // Content area can be determined
        if (area.type == TextType.CONTENT) {
            area.end = skipContent(text, area.start);
            area.text = text.substring(area.start, area.end);
            return area;
        }

        // Possible types are now: TEXT, NUMBER, OPERATOR
        while (position < text.length()) {

            final char c = text.charAt(position);

            if (isContent(c)) {

                // There cannot be number and content tokens side by side
                if (area.type == TextType.NUMBER) {
                    throw new Exception("Missing operator between number and parenthesis");
                }

                break;
            }

            final TextType type = getType(c);

            if (!isPartOf(area.type, type, c)) {
                break;
            }

            position++;
        }

        area.end = position;
        area.text = text.substring(area.start, area.end);

        return area;
    }

    private static Token parseTextToken (final String text) {
        if (DataTypeDatabase.exists(text)) {
            return new DataTypeToken(text);
        }
        else if (KeywordDatabase.exists(text)) {
            return new KeywordToken(text);
        }
        else {
            return new NameToken(text);
        }
    }

    private static Token parseToken (TokenArea area) throws Exception {
        switch (area.type) {
            case TEXT:
                return parseTextToken(area.text);
            case NUMBER:
                return new NumberToken(area.text);
            case OPERATOR:
                return new OperatorToken(area.text);
            case CONTENT:
                return new ContentToken(area.text);
            default:
                throw new Exception("Unrecognized token");
        }
    }

    private static void scanFunctions (List<Token> tokens) {
        if (tokens.size() < 2) {
            return;
        }

        for (int i = tokens.size() - 2; i >= 0; i--) {
            final Token current = tokens.get(i);

            if (current.getType() == TokenType.NAME) {
                final Token next = tokens.get(i + 1);

                if (next.getType() == TokenType.CONTENT) {
                    final Token function = new FunctionToken((NameToken)current,
                                                            (ContentToken) next);
                    tokens.set(i, function);
                    tokens.remove(i + 1);

                    // TODO: Function cannot be produced next since now created token isn't content
                }
            }
        }
    }

    private static void reduceMath(List<Token> tokens) {

    }

    private static void postProcess (List<Token> tokens) {
        scanFunctions(tokens);
        reduceMath(tokens);
    }

    public static List<Token> getTokens (String line) throws Exception {
        line = line.trim();

        final List<Token> tokens = new ArrayList<>();
        int position = 0;

        while (position != line.length()) {
            final TokenArea area = getNextTokenArea(line, position);

            final Token token = parseToken(area);
            tokens.add(token);

            position = area.end;
        }

        postProcess(tokens);

        return tokens;
    }
}
