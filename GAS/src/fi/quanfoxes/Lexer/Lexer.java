package fi.quanfoxes.Lexer;

import fi.quanfoxes.DataTypeDatabase;
import fi.quanfoxes.KeywordDatabase;

import java.util.ArrayList;
import java.util.List;

public class Lexer {

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

        public Object data;
    }

    private static boolean isOperator (char c) {
        return c >= 33 && c <= 47 || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126;
    }

    private static boolean isText(char c) {
        return Character.isLetter(c) || "_-".contains(String.valueOf(c));
    }

    private static TextType getType (char c) {

        if (Character.isDigit(c)) {
            return TextType.NUMBER;
        }
        else if (isText(c)) {
            return TextType.TEXT;
        }
        else {

            if (isContent(c)) {
                return TextType.CONTENT;
            }
            else if (isOperator(c)) {
                return TextType.OPERATOR;
            }
        }

        return TextType.UNSPECIFIED;
    }

    private static boolean isContent (char c) {
        return c == '(' || c == '{';
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

        while (position < text.length()) {
            if (text.charAt(position) == '(') {
                count++;
            }
            else if (text.charAt(position) == ')') {
                count--;
            }

            position++;

            if (count == 0) {
                return position;
            }
        }

        throw new Exception("Couldn't find closing parenthesis");
    }

    public static TokenArea getNextTokenArea(String text, int position) throws Exception {

        int i = skipSpaces(text, position);

        final TokenArea area = new TokenArea();
        area.start = i;
        area.type = getType(text.charAt(i));

        // Content area can be determined
        if (area.type == TextType.CONTENT) {
            area.end = skipContent(text, area.start);
            area.text = text.substring(area.start, area.end);
            return area;
        }

        // Possible types are now: TEXT, NUMBER, OPERATOR
        while (i < text.length()) {

            if (isContent(text.charAt(i))) {

                // There cannot be number and content tokens side by side
                if (area.type == TextType.NUMBER) {
                    throw new Exception("Missing operator between number and parenthesis");
                }

                break;
            }

            TextType type = getType(text.charAt(i));

            if (area.type == TextType.TEXT) {

                // Operators cannot be part of text
                if (type == TextType.OPERATOR || type == TextType.UNSPECIFIED) {
                    break;
                }
            }
            else if (area.type != type) {
                break;
            }

            i++;
        }

        area.end = i;
        area.text = text.substring(area.start, area.end);

        return area;
    }

    private static Token parseTextToken (TokenArea area) {
        if (DataTypeDatabase.exists(area.text)) {
            return new DataTypeToken(area);
        }
        else if (KeywordDatabase.exists(area.text)) {
            return new KeywordToken(area);
        }
        else {
            return new NameToken(area);
        }
    }

    private static Token parseToken (TokenArea area) throws Exception {
        switch (area.type) {
            case TEXT:
                return parseTextToken(area);
            case NUMBER:
                return new NumberToken(area);
            case OPERATOR:
                return new OperatorToken(area);
            case CONTENT:
                return new ContentToken(area);
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

    private static void postProcess (List<Token> tokens) {
        scanFunctions(tokens);
    }

    public static List<Token> getTokens (String line) throws Exception {
        line = line.trim();

        final List<Token> tokens = new ArrayList<>();
        int position = 0;

        while (position != line.length()) {
            TokenArea area = getNextTokenArea(line, position);

            Token token = parseToken(area);
            tokens.add(token);

            position = area.end;
        }

        postProcess(tokens);

        return tokens;
    }

    public void analyze (String[] lines) {
        for (String line : lines) {

        }
    }
}
