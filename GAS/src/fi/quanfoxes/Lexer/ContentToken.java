package fi.quanfoxes.Lexer;

import java.util.*;

public class ContentToken extends Token {
    private List<ContentToken> sections = new ArrayList<>();
    private List<Token> tokens;

    public ContentToken(final Lexer.TokenArea area) throws Exception {
        this(area.text);
    }

    private boolean isComma (final Token token) {
        return token.getType() == TokenType.OPERATOR && ((OperatorToken)token).getOperator() == OperatorType.COMMA;
    }

    private Stack<Integer> findSections(final List<Token> tokens) {
        final Stack<Integer> indices = new Stack<>();

        for (int i = tokens.size() - 1; i >= 0; i--) {
            final Token token = tokens.get(i);

            if (isComma(token)) {
                indices.push(i);
            }
        }

        return indices;
    }

    public ContentToken(final String text) throws Exception {
        super(TokenType.CONTENT);

        // Make sure there is content
        if (text.length() > 2) {

            sections = new ArrayList<>();

            final String content = text.substring(1, text.length() - 1);
            final List<Token> tokens = Lexer.getTokens(content);
            final Stack<Integer> sections = findSections(tokens);

            if (sections.empty()) {
                this.tokens = tokens;
                return;
            }

            sections.add(0, tokens.size());

            int position = 0;
            int end;

            while (!sections.empty()) {
                end = sections.pop();

                final List<Token> section = tokens.subList(position, end);

                if (section.isEmpty()) {
                    throw new Exception("Parameter cannot be empty");
                }

                this.sections.add(new ContentToken(section));

                position = end + 1;
            }
        }
    }

    public ContentToken(List<Token> tokens) {
        super(TokenType.CONTENT);
        this.tokens = tokens;
    }

    public ContentToken(Token... tokens) {
        super(TokenType.CONTENT);
        this.tokens = Arrays.asList(tokens);
    }

    public ContentToken(ContentToken... sections) {
        super(TokenType.CONTENT);
        this.sections = Arrays.asList(sections);
    }

    public boolean hasSections() {
        return !sections.isEmpty();
    }

    public List<Token> getTokens() {
        return hasSections() ? sections.get(0).getTokens() : tokens;
    }

    public List<Token> getTokens(int section) {
        return sections.get(section).getTokens();
    }

    public ContentToken getSection(int section) {
        return sections.get(section);
    }

    public int getSectionCount() {
        return sections.size();
    }

    @Override
    public String getText() {
        if (hasSections()) {
            final StringBuilder builder = new StringBuilder("(");

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
