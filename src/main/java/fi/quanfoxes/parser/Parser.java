package fi.quanfoxes.parser;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Patterns.Option;

import java.util.*;

public class Parser {

    public static final int MAX_PRIORITY = 21;
    public static final int MEMBERS = 20;
    public static final int MIN_PRIORITY = 1;

    /**
     * Used to store instances of patterns in tokens lists
     */
    private static class Instance {
        private Pattern pattern;
        private List<Token> tokens;
        private List<Token> molded;

        /**
         * Create an instance of pattern in tokens list
         * @param pattern Pattern that this instance represents
         * @param tokens List of tokens associated with the pattern
         * @param molded Given tokens molded for use
         */
        public Instance(Pattern pattern, List<Token> tokens, List<Token> molded) {
            this.pattern = pattern;
            this.tokens = tokens;
            this.molded = molded;
        }

        /**
         * Returns the pattern that this instance represents
         * @return Pattern that this instance represents
         */
        public Pattern getPattern() {
            return pattern;
        }

        /**
         * Returns a list of tokens associated with this pattern
         * @return List of tokens associated with this pattern
         */
        public List<Token> getTokens() {
            return molded;
        }

        /**
         * Replaces the tokens with a processed token
         * @param token Processed token to insert
         */
        public void replace(DynamicToken token) {
            int start = pattern.start();
            int end = pattern.end();

            tokens.subList(start, end == -1 ? tokens.size() : end).clear();
            tokens.add(token);
        }
    }

    /**
     * Fills missing optional tokens with null
     * @param indices Indices that represent the missing optional tokens
     * @param candidate Token list that matches the pattern
     * @return Token list molded for use
     */
    private static List<Token> mold(List<Integer> indices, List<Token> candidate) {
        List<Token> molded = new ArrayList<>(candidate);

        for (int index : indices) {
            molded.add(index, null);
        }

        return molded;
    }

    /**
     * Tries to find the next pattern with given priority from the token list
     * @param tokens Token list to scan
     * @param priority Priority used to filter found patterns
     * @return Success: Instance of a pattern with the given priority in the token list. Failure: null
     */
    private static Instance next(List<Token> tokens, int priority) {

        for (int start = 0; start < tokens.size(); start++) {

            // Start from the root
            Patterns patterns = Patterns.getRoot();
            Instance instance = null;

            // Try finding the next pattern
            for (int end = start; end < tokens.size(); end++) {

                // Navigate forward on the tree
                patterns = patterns.navigate(tokens.get(end).getType());

                // When tree becomes null the end of the tree is reached
                if (patterns == null) {
                    break;
                }

                if (patterns.hasOptions()) {
                    List<Token> candidate = tokens.subList(start, end + 1);

                    for (Option option : patterns.getOptions()) {
                        Pattern pattern = option.getPattern();
                        List<Token> molded = mold(option.getMissing(), candidate);

                        if (pattern.priority(molded) == priority && pattern.passes(molded)) {
                            instance = new Instance(pattern, candidate, molded);
                            break;
                        }
                    }
                }
            }

            if (instance != null) {
                return instance;
            }
        }

        return null;
    }

    public static Node parse(Context context, List<Token> section) throws Exception {
        return Parser.parse(context, section, MIN_PRIORITY, MAX_PRIORITY);
    }

    public static Node parse(Context context, List<Token> tokens, int priority) throws Exception {
        return Parser.parse(context, tokens, priority, priority);
    }

    public static Node parse(Context context, List<Token> tokens, int minPriority, int maxPriority) throws Exception {
        Node node = new Node();
        Parser.parse(node, context, tokens, minPriority, maxPriority);
        
        return node;
    }

    public static void parse(Node parent, Context context, List<Token> tokens) throws Exception {
        Parser.parse(parent, context, tokens, MIN_PRIORITY, MAX_PRIORITY);
    }

    public static void parse(Node parent, Context context, List<Token> tokens, int priority) throws Exception {
        Parser.parse(parent, context, tokens, priority, priority);
    }

    public static void parse(Node parent, Context context, List<Token> tokens, int minPriority, int maxPriority) throws Exception {

        for (int priority = maxPriority; priority >= minPriority; priority--) {
            
            Instance instance;

            // Find all patterns with the current priority
            while ((instance = next(tokens, priority)) != null) {
                
                // Build the pattern into a node
                Pattern pattern = instance.getPattern();
                Node node = pattern.build(context, instance.getTokens());

                // Replace the pattern with a processed token
                DynamicToken token = new DynamicToken(node);
                instance.replace(token);
            }
        }

        // Combine all processed tokens in order
        for (Token token : tokens) {
            if (token.getType() == TokenType.DYNAMIC) {
                DynamicToken dynamic = (DynamicToken)token;
                parent.add(dynamic.getNode());
            }
        }
    }

    public static Context initialize() throws Exception {
        Context context = new Context();
        Types.inject(context);

        Function allocate = new Function(context, "allocate", AccessModifier.PUBLIC | AccessModifier.EXTERNAL, Types.LINK);
        Variable bytes = new Variable(allocate, Types.NORMAL, VariableType.PARAMETER, "bytes", AccessModifier.PUBLIC);
        allocate.setParameters(bytes);

        Function power = new Function(context, "integer_power", AccessModifier.PUBLIC | AccessModifier.EXTERNAL, Types.NORMAL);
        Variable base = new Variable(power, Types.NORMAL, VariableType.PARAMETER, "a", AccessModifier.PUBLIC);
        Variable exponent = new Variable(power, Types.NORMAL, VariableType.PARAMETER, "b", AccessModifier.PUBLIC);
        power.setParameters(base, exponent);

        return context;
    }
}
