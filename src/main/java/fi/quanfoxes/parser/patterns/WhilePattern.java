package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.WhileNode;

import java.util.ArrayList;
import java.util.List;

public class WhilePattern extends Pattern {
    public static final int PRIORITY = 15;

    private static final int WHILE = 0;
    private static final int CONDITION = 1;
    private static final int BODY = 2;

    public WhilePattern() {
        // Pattern:
        // while (...) {...}
        // Examples:
        // while (a && b) {...}
        // while (true) {...}
        super(TokenType.KEYWORD, TokenType.PROCESSED, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(WHILE);

        if (keyword.getKeyword() != Keywords.WHILE) {
            return false;
        }

        ProcessedToken condition = (ProcessedToken)tokens.get(CONDITION);

        return (condition.getNode() instanceof ContentNode);
    }

    @Override
    public Node build(Context base, List<Token> tokens) throws Exception {
        ProcessedToken condition = (ProcessedToken)tokens.get(CONDITION);
        ArrayList<Token> body = ((ContentToken)tokens.get(BODY)).getTokens();

        Context context = new Context();
        context.link(base);
        
        //Parser.parse(context, section)

        //return new WhileNode(condition.getNode(), body.getNode());
        return null;
    }
}
