package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.nodes.ContextNode;

import java.util.*;

public class Parser {
    private static ContextNode root = new ContextNode();

    public static final int MAX_PRIORITY = 20;
    public static final int STRUCTURAL_MAX_PRIORITY = 20;
    public static final int STRUCTURAL_MIN_PRIORITY = 19;
    public static final int MIN_PRIORITY = 1;

    private static class PatternInfo {
        private Pattern pattern;
        private List<Token> tokens;

        PatternInfo(Pattern pattern, List<Token> tokens) {
            this.pattern = pattern;
            this.tokens = tokens;
        }

        Pattern getPattern() {
            return pattern;
        }

        List<Token> getTokens() {
            return tokens;
        }

        void replace(Token token) {
            tokens.clear();
            tokens.add(token);
        }
    }

    private static PatternInfo findNextPatternByPriority(ArrayList<Token> section, int priority) {

        for (int start = 0; start < section.size(); start++) {

            // Start from the root
            Patterns tree = Patterns.getRoot();
            PatternInfo best = null;

            // Try finding the next pattern
            for (int end = start; end < section.size(); end++) {

                // Navigate forward on the tree
                tree = tree.navigate(section.get(end).getType());

                // When tree becomes null the end of the tree is reached
                if (tree == null) {
                    break;
                }

                if (tree.hasLeaves()) {
                    List<Token> tokens = section.subList(start, end + 1);

                    for (Pattern pattern : tree.getLeaves()) {
                        if (pattern.priority(tokens) == priority && pattern.passes(tokens)) {
                            best = new PatternInfo(pattern, tokens);
                            break;
                        }
                    }
                }
            }

            if (best != null) {
                return best;
            }
        }

        return null;
    }

    public static void parse(Node parent, ArrayList<Token> section) throws Exception {
        parse(parent, section, MIN_PRIORITY, MAX_PRIORITY);
    }

    public static void parse(Node parent, ArrayList<Token> section, int minPriority, int maxPriority) throws Exception {

        for (int priority = maxPriority; priority >= minPriority; priority--) {
            PatternInfo info;

            while ((info = findNextPatternByPriority(section, priority)) != null) {
                Pattern pattern = info.getPattern();
                Node node = pattern.build(parent, info.getTokens());

                ProcessedToken token = new ProcessedToken(node);
                info.replace(token);
            }
        }

        // Combine all programmed tokens in order
        for (Token token : section) {
            if (token.getType() == TokenType.PROCESSED) {
                ProcessedToken program = (ProcessedToken)token;
                parent.add(program.getNode());
            }
        }
    }

    public static ContextNode getRoot() {
        return root;
    }
}
