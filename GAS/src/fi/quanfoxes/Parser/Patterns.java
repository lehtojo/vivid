package fi.quanfoxes.Parser;

/*

num a
num a = 9 * b
num b = a + (6 * c)
num c = apple()

if (a > b)

[DataType][Name] -> Reserve variable & Set active
[Name] -> Set active
[Operator]:
    [Assign] -> Enter assign mode -> Override active as destination
    [Add] -> Enter result mode -> Override active as source
    [Boolean operator] Enter compare mode -> Override active as source

[Number/Content]

[Keyword]



 */

import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.patterns.DeclareLocalVariablePattern;
import fi.quanfoxes.Parser.patterns.DefineLocalVariablePattern;
import fi.quanfoxes.Parser.patterns.UseVariablePattern;

import java.util.*;

public class Patterns {
    private Map<TokenType, Patterns> entries = new HashMap<>();
    private List<Pattern> patterns = new ArrayList<>();

    private void build(final Pattern pattern, final Stack<TokenType> path) {
        if (path.empty()) {
            patterns.add(pattern);
            return;
        }

        final TokenType node = path.pop();

        if (entries.containsKey(node)) {
            entries.get(node).build(pattern, path);
        }
        else {
            final Patterns patterns = new Patterns();
            patterns.build(pattern, path);
            entries.put(node, patterns);
        }
    }

    private void build(final Pattern pattern) {
        final Stack<TokenType> path = new Stack<>();

        final List<TokenType> nodes = pattern.getPath();
        Collections.reverse(nodes);

        path.addAll(nodes);

        build(pattern, path);
    }

    public Patterns filter(final TokenType type) {
        return entries.get(type);
    }

    public List<Pattern> getPatterns() {
        return patterns;
    }

    public boolean hasEntries() { return !entries.isEmpty(); }
    public boolean hasPatterns() {
        return !patterns.isEmpty();
    }

    private static Patterns root = new Patterns();

    private static void add(final Pattern pattern) {
        root.build(pattern);
    }

    public static Patterns getRoot() {
        return root;
    }

    static {
        add(new DefineLocalVariablePattern());
        add(new DeclareLocalVariablePattern());
        add(new UseVariablePattern());
    }
}
