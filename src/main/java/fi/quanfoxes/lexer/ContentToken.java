package fi.quanfoxes.lexer;

import java.util.*;

import fi.quanfoxes.Errors;
import fi.quanfoxes.lexer.Lexer.Position;

public class ContentToken extends Token {
    private ParenthesisType type;

    private List<ContentToken> sections = new ArrayList<>();
    private List<Token> tokens = new ArrayList<>();

    private static final int OPENING = 0;
    private static final int EMPTY = 2;

    /**
     * Returns whether the given token is a comma operator
     * @param token Token to test
     * @return True if the given token is a comma operator
     */
    private boolean isComma (Token token) {
        return token.getType() == TokenType.OPERATOR && ((OperatorToken)token).getOperator() == OperatorType.COMMA;
    }

    /**
     * Finds the indices of potential comma operators in the given token list
     * @param tokens Token list to iterate through
     * @return Indices of all comma operators in the given list
     */
    private Stack<Integer> findSections(List<Token> tokens) {
        Stack<Integer> indices = new Stack<>();

        for (int i = tokens.size() - 1; i >= 0; i--) {
            Token token = tokens.get(i);

            if (isComma(token)) {
                indices.push(i);
            }
        }

        return indices;
    }

    /**
     * Creates a content token with content and a position
     * @param raw Raw content text to tokenize
     * @param position Position of this content token
     * @throws Exception Various reasons: Empty sections inside content, Lexer
     */
    public ContentToken(String raw, Position position) throws Exception {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.get(raw.charAt(OPENING));

        if (raw.length() != EMPTY) {

            String content = raw.substring(1, raw.length() - 1);
            List<Token> tokens = Lexer.getTokens(content, position.clone().nextCharacter());
            Stack<Integer> sections = findSections(tokens);

            if (sections.empty()) {
                this.tokens = tokens;
                return;
            }

            sections.add(0, tokens.size());

            int start = 0;
            int end;

            while (!sections.empty()) {
                end = sections.pop();

                List<Token> section = tokens.subList(start, end);

                if (section.isEmpty()) {
                    Position comma = tokens.get(start).getPosition();
                    throw Errors.get(comma.nextCharacter(), "Parameter cannot be empty");
                }

                this.sections.add(new ContentToken(section));

                start = end + 1;
            }
        }
    }

    /**
     * Creates a content token from tokens
     * @param tokens Content in token list form
     */
    public ContentToken(List<Token> tokens) {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.PARENTHESIS;
        this.tokens = new ArrayList<>(tokens);
    }

    /**
     * Creates a content token from tokens
     * @param tokens Content in token list form
     */
    public ContentToken(Token... tokens) {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.PARENTHESIS;
        this.tokens = new ArrayList<>(Arrays.asList(tokens));
    }

    /**
     * Creates a content token, which has multiple sections
     * @param tokens Sections in content token form
     */
    public ContentToken(ContentToken... sections) {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.PARENTHESIS;
        this.sections = new ArrayList<>(Arrays.asList(sections));
    }

    /**
     * Return whether this content token has sections
     * @return True if this content token has sections, otherwise false
     */
    private boolean isTable() {
        return sections.size() > 0;
    }

    /**
     * Returns the tokens of the first section
     * @return Tokens of the first section
     */
    public List<Token> getTokens() {
        return getTokens(0);
    }

    /**
     * Returns 
     * @param section
     * @return
     */
    public List<Token> getTokens(int section) {
        return isTable() ? sections.get(section).getTokens() : tokens;
    }

    public int getSectionCount() {
        return Math.max(1, sections.size());
    }

    public ParenthesisType getParenthesisType() {
        return type;
    }

    @Override
    public String getText() {
        if (getSectionCount() > 1) {
            StringBuilder builder = new StringBuilder("(");

            for (int i = 0; i < sections.size() - 1; i++) {
                builder.append(sections.get(i).getText()).append(", ");
            }

            builder.append(sections.get(sections.size() - 1)).append(")");
            return builder.toString();
        }
        else {
            return tokens.stream().map(Token::getText)
                    .collect(StringBuilder::new, StringBuilder::append, StringBuilder::append).toString();
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof ContentToken)) return false;
        if (!super.equals(o)) return false;
        ContentToken that = (ContentToken) o;
        return Objects.equals(sections, that.sections) &&
                Objects.equals(tokens, that.tokens);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), sections, tokens);
    }
}
