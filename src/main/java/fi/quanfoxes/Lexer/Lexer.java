package fi.quanfoxes.Lexer;

import fi.quanfoxes.Keywords;

import java.util.ArrayList;

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

    // find ./src -name *.java > Build/.files
    // javac -d Build/Out @Build/.files
    // cd Build
    // rm .files
    // jar cfm gz.jar Manifest.txt ./Out 

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
        return OPENING_PARENTHESIS.indexOf(c) >= 0;
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

        if (OperatorType.has(text)) {
            return new OperatorToken(text);
        }
        else if (Keywords.exists(text)) {
            return new KeywordToken(text);
        }
        else {
            return new IdentifierToken(text);
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

    public static ArrayList<Token> getTokens (String section) throws Exception {
        section = section.trim();

        ArrayList<Token> tokens = new ArrayList<>();
        int position = 0;

        while (position != section.length()) {
            TokenArea area = getNextTokenArea(section, position);

            Token token = parseToken(area);
            tokens.add(token);

            position = area.end;
        }

        return tokens;
    }
}
