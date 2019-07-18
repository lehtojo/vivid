package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Flag;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.patterns.*;

import java.util.*;

public class Patterns {
    private HashMap<Integer, Patterns> branches = new HashMap<>();
    private ArrayList<Pattern> leaves = new ArrayList<>();

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

    private void grow(Pattern pattern, ArrayList<Integer> path) {
        if (path.isEmpty()) {
            leaves.add(pattern);
            return;
        }

        int mask = path.remove(0);

        for (int i = 0; i < TokenType.COUNT; i++) {
            int type = 1 << i;

            if (Flag.has(mask, type)) {
                Patterns branch = getOrGrowBranch(type);
                branch.grow(pattern, new ArrayList<>(path));
            }
        }
    }

    public Patterns navigate(final int type) {
        if (branches.containsKey(type)) {
            return branches.get(type);
        }

        return null;
    }

    public List<Pattern> getLeaves() {
        return leaves;
    }

    public boolean hasBranches() { return !branches.isEmpty(); }
    public boolean hasLeaves() {
        return !leaves.isEmpty();
    }

    // ----------------------------------------------------------

    private static Patterns root = new Patterns();
    public static Patterns getRoot() {
        return root;
    }

    private static void add(Pattern pattern) {
        root.grow(pattern, pattern.getPath());
    }

    static {
        add(new ContentPattern());
        add(new DotPattern());
        add(new MemberFunctionPattern());
        add(new MemberVariablePattern());
        add(new OperatorPattern());
        add(new TypePattern());
        add(new UnarySignPattern());
        add(new VariablePattern());
        add(new WhilePattern());
    }
}
