package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Flag;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.patterns.*;

import java.util.*;

public class Patterns {
    private Map<Integer, Patterns> branches = new HashMap<>();
    private List<Pattern> leaves = new ArrayList<>();

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
     */
    private void grow(Pattern pattern, List<Integer> path) {
        if (path.isEmpty()) {
            leaves.add(pattern);
            return;
        }

        int mask = path.remove(0);

        if (Flag.has(mask, TokenType.OPTIONAL)) {
            grow(pattern, new ArrayList<>(path));
        }

        for (int i = 0; i < TokenType.COUNT; i++) {
            int type = 1 << i;

            if (Flag.has(mask, type)) {
                Patterns branch = getOrGrowBranch(type);
                branch.grow(pattern, new ArrayList<>(path));
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
    public List<Pattern> getLeaves() {
        return leaves;
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
    public boolean hasLeaves() {
        return !leaves.isEmpty();
    }

    /*
     * ===========================================
     */

    private static Patterns root = new Patterns();

    public static Patterns getRoot() {
        return root;
    }

    private static void add(Pattern pattern) {
        root.grow(pattern, pattern.getPath());
    }

    static {
        add(new ConstructionPattern());
        add(new ConstructorPattern());
        add(new ContentPattern());
        add(new IfPattern());
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
