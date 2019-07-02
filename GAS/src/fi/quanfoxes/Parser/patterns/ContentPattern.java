package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.ContentToken;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.nodes.ContentNode;

import java.util.List;

public class ContentPattern extends Pattern {
    private static final int PRIORITY = 16;

    public ContentPattern() {
        super(TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        return true;
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        ContentToken content = (ContentToken)tokens.get(0);
        ContentNode node = new ContentNode();

        for (int i = 0; i < content.getSectionCount(); i++) {
            ContentToken section = content.getSection(i);
            Parser.parse(node, section.getTokens());
        }

        return node;
    }
}
