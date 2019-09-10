package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.nodes.LoopNode;
import fi.quanfoxes.Errors;

import java.util.List;

public class WhilePattern extends Pattern {
    public static final int PRIORITY = 15;

    private static final int WHILE = 0;
    private static final int STEPS = 1;
    private static final int BODY = 3;

    public WhilePattern() {
        // Pattern:
        // while (...) [\n] {...}
        super(TokenType.KEYWORD, /* loop */
              TokenType.DYNAMIC | TokenType.OPTIONAL, /* [(...)] */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.CONTENT); /* {...} */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(WHILE);

        if (keyword.getKeyword() != Keywords.LOOP) {
            return false;
        }

        return true;
    }

    private List<Token> getBody(List<Token> tokens) {
        ContentToken content = (ContentToken)tokens.get(BODY);
        return content.getTokens();
    }

    private Node mold(List<Token> tokens, Node steps) throws Exception {
        var iterator = steps.first();
        var count = 0;

        while (iterator != null) {
            count++;
            iterator = iterator.next();
        }

        switch(count) {
            case 0:
                throw Errors.get(tokens.get(WHILE).getPosition(), "While parenthesis cannot be empty");
            case 1:
                steps.insert(steps.first(), new Node());
                steps.add(new Node());
                return steps;
            case 2:
                steps.insert(steps.first(), new Node());
            default:
                return steps;
        }
    }

    private Node getSteps(List<Token> tokens) throws Exception {
        DynamicToken dynamic = (DynamicToken)tokens.get(STEPS);

        if (dynamic != null) {
            Node steps = mold(tokens, dynamic.getNode());
            return steps; 
        }
        
        return null;
    }

    @Override
    public Node build(Context base, List<Token> tokens) throws Exception {
        Context context = new Context();
        context.link(base);

        Node body = Parser.parse(context, getBody(tokens));
        Node steps = getSteps(tokens);

        return new LoopNode(context, steps, body);
    }
}
