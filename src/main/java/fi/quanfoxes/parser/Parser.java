package fi.quanfoxes.parser;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;

import java.util.*;

public class Parser {

    public static final int MAX_PRIORITY = 20;
    public static final int MEMBERS = 19;
    public static final int MIN_PRIORITY = 1;

    private static class PatternInfo {
        private Pattern pattern;
        private List<Token> tokens;

        public PatternInfo(Pattern pattern, List<Token> tokens) {
            this.pattern = pattern;
            this.tokens = tokens;
        }

        public Pattern getPattern() {
            return pattern;
        }

        public List<Token> getTokens() {
            return tokens;
        }

        public void replace(Token token) {
            int start = pattern.start();
            int end = pattern.end();

            tokens.subList(start, end == -1 ? tokens.size() : end).clear();
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

    public static Node parse(Context context, ArrayList<Token> section) throws Exception {
        return Parser.parse(context, section, MIN_PRIORITY, MAX_PRIORITY);
    }

    public static Node parse(Context context, ArrayList<Token> tokens, int priority) throws Exception {
        return Parser.parse(context, tokens, priority, priority);
    }

    public static Node parse(Context context, ArrayList<Token> tokens, int minPriority, int maxPriority) throws Exception {
        Node node = new Node();
        Parser.parse(node, context, tokens, minPriority, maxPriority);
        
        return node;
    }

    public static void parse(Node parent, Context context, ArrayList<Token> tokens) throws Exception {
        Parser.parse(parent, context, tokens, MIN_PRIORITY, MAX_PRIORITY);
    }

    public static void parse(Node parent, Context context, ArrayList<Token> tokens, int priority) throws Exception {
        Parser.parse(parent, context, tokens, priority, priority);
    }

    public static void parse(Node parent, Context context, ArrayList<Token> tokens, int minPriority, int maxPriority) throws Exception {

        for (int priority = maxPriority; priority >= minPriority; priority--) {
            
            PatternInfo info;

            // Find all patterns with the current priority
            while ((info = findNextPatternByPriority(tokens, priority)) != null) {
                
                // Build the pattern into a node
                Pattern pattern = info.getPattern();
                Node node = pattern.build(context, info.getTokens());

                // Replace the pattern with a processed token
                ProcessedToken token = new ProcessedToken(node);
                info.replace(token);
            }
        }

        // Combine all processed tokens in order
        for (Token token : tokens) {
            if (token.getType() == TokenType.PROCESSED) {
                ProcessedToken processed = (ProcessedToken)token;
                parent.add(processed.getNode());
            }
        }
    }

    public static Context initialize() throws Exception {
        Context context = new Context();
        Types.inject(context);
        return context;
    }
}
