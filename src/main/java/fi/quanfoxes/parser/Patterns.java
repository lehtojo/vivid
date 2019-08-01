package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Flag;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.patterns.*;

import java.util.*;

public class Patterns {
    private Map<Integer, Patterns> branches = new HashMap<>();
    private List<Option> options = new ArrayList<>();

    public static class Option {
        private Pattern pattern;
        private List<Integer> optionals;

        public Option(Pattern pattern, List<Integer> optionals) {
            this.pattern = pattern;
            this.optionals = optionals;
        }

        public Pattern getPattern() {
            return pattern;
        }

        public List<Integer> getMissing() {
            return new ArrayList<>(optionals);
        }
    }

    /**
     * Returns existing branch by identifier or creates it
     * @param branch Branch identifier
     * @return Existing or created branch
     */
    private Patterns getOrGrowBranch(int branch) {
        if (branches.containsKey(branch)) {
            return branches.get(branch);
        }
        else {
            Patterns patterns = new Patterns();
            branches.put(branch, patterns);
            return patterns;
        }
    }

    /**
     * Creates a path for the given pattern in this tree
     * @param pattern Pattern to add
     * @param path Path to find the pattern
     * @param missing Optionals that aren't included in the built path
     * @param position Current index in the path
     */
    private void grow(Pattern pattern, List<Integer> path, List<Integer> missing, int position) {
        if (position >= path.size()) {
            options.add(new Option(pattern, missing));
            return;
        }

        int mask = path.get(position);

        if (Flag.has(mask, TokenType.OPTIONAL)) {
            List<Integer> variation = new ArrayList<>(missing);
            variation.add(position);

            grow(pattern, path, variation, position + 1);
        }

        for (int i = 0; i < TokenType.COUNT; i++) {
            int type = 1 << i;

            if (Flag.has(mask, type)) {
                Patterns branch = getOrGrowBranch(type);
                branch.grow(pattern, path, missing, position + 1);
            }
        }
    }

    /**
     * Navigate to the next branch by the given identifier
     * @param type Identifier to find the branch
     * @return Success: Reference to the navigated branch, Failure: null
     */
    public Patterns navigate(int type) {
        if (branches.containsKey(type)) {
            return branches.get(type);
        }

        return null;
    }

    /**
     * Returns all patterns in the current branch
     * @return All patterns in the current branch
     */
    public List<Option> getOptions() {
        return options;
    }

    /**
     * Returns whether this branch has branches
     * @return True if this branch has branches, otherwise false
     */
    public boolean hasBranches() { 
        return !branches.isEmpty(); 
    }

    /**
     * Returns whether this branch has patterns
     * @return True if this branch has patterns, otherwise false
     */
    public boolean hasOptions() {
        return !options.isEmpty();
    }

    /*
     * ===========================================
     */

    private static Patterns root = new Patterns();

    public static Patterns getRoot() {
        return root;
    }

    private static void add(Pattern pattern) {
        root.grow(pattern, pattern.getPath(), new ArrayList<>(), 0);
    }

    static {
        add(new ArrayPattern());
        add(new CastPattern());
        add(new ConstructionPattern());
        add(new ConstructorPattern());
        add(new ContentPattern());
        add(new ElsePattern());
        add(new ExtendedTypePattern());
        add(new IfPattern());
        add(new InstructionPattern());
        add(new JumpPattern());
        add(new LabelPattern());
        add(new LinkPattern());
        add(new MemberFunctionPattern());
        add(new MemberVariablePattern());
        add(new OperatorPattern());
        add(new ReturnPattern());
        add(new SingletonPattern());
        add(new TypePattern());
        add(new UnarySignPattern());
        add(new VariablePattern());
        add(new WhilePattern());
    }
}
