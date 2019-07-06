package fi.quanfoxes.lexer;

import java.util.*;

public class ContentToken extends Token {
    private ParenthesisType type;
    private ArrayList<ContentToken> sections = new ArrayList<>();
    private ArrayList<Token> tokens = new ArrayList<>();

    private static final int OPENING = 0;

    private boolean isComma (Token token) {
        return token.getType() == TokenType.OPERATOR && ((OperatorToken)token).getOperator() == OperatorType.COMMA;
    }

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

    public ContentToken(String text) throws Exception {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.get(text.charAt(OPENING));

        // Make sure there is content
        if (text.length() > 2) {

            sections = new ArrayList<>();

            String content = text.substring(1, text.length() - 1);
            ArrayList<Token> tokens = Lexer.getTokens(content);
            Stack<Integer> sections = findSections(tokens);

            if (sections.empty()) {
                this.tokens = tokens;
                return;
            }

            sections.add(0, tokens.size());

            int position = 0;
            int end;

            while (!sections.empty()) {
                end = sections.pop();

                List<Token> section = tokens.subList(position, end);

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
        this.type = ParenthesisType.PARENTHESIS;
        this.tokens = new ArrayList<>(tokens);
    }

    public ContentToken(Token... tokens) {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.PARENTHESIS;
        this.tokens = new ArrayList<>(Arrays.asList(tokens));
    }

    public ContentToken(ContentToken... sections) {
        super(TokenType.CONTENT);
        this.type = ParenthesisType.PARENTHESIS;
        this.sections = new ArrayList<>(Arrays.asList(sections));
    }

    private boolean isTable() {
        return sections.size() > 0;
    }

    public ArrayList<Token> getTokens() {
        return getTokens(0);
    }

    public ArrayList<Token> getTokens(int section) {
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
