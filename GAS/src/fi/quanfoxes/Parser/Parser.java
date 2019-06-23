package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

public class Parser {
    private static final boolean DEBUG = true;

    public static List<Instruction> parse(final List<Token> line) throws Exception {

        // To resolve operator execution order each pattern is given a priority
        // Parser finds the pattern with highest priority and processes it and returns a token to represent the result
        // Every line of tokens is therefore dynamic

        // Example:
        // num a = b * c + d * e
        // num a = b * c + [ResultToken ID=1]
        // num a = [ResultToken ID=2] + [ResultToken ID=1]
        // num a = [ResultToken ID=3]
        //
        // Pattern for creating local variables is now recognized
        // [Datatype][Name][Operator: assign][ResultToken] => [ResultToken].type == [DatatypeToken].type
        //
        // --------------------------------------------------------------------------------------------------
        //
        // num b = if line.empty() then 0 else 1
        // num b = if [ResultToken ID=1] then 0 else 1
        //
        // Pattern for short if statement is now recognized
        // [Keyword: if][ResultToken][Keyword: then][Number][Keyword: else][Number]
        //
        // num b = [ResultToken ID=2]
        //
        // Pattern for creating local variables is now recognized
        // [Datatype][Name][Operator: assign][ResultToken] => [ResultToken].type == [DatatypeToken].type
        //
        // --------------------------------------------------------------------------------------------------
        //
        // Lines are processed one by one
        // Processing must be implemented by using stream since for example if statement need multiple lines
        

        final List<Instruction> instructions = new ArrayList<>();
        final List<Token> tokens = new ArrayList<>();

        Patterns patterns = Patterns.getRoot();
        Pattern best = null;

        if (DEBUG && !patterns.hasEntries()) {
            throw new Exception("INTERNAL_PARSER_ERROR: There aren't any patterns added");
        }

        for (final Token token : line) {
            patterns = patterns.filter(token.getType());
            tokens.add(token);

            if (patterns.hasPatterns()) {

                if (DEBUG) {
                    // Find out which patterns are passed
                    final List<Pattern> passed = patterns.getPatterns().stream()
                            .filter(p -> p.passes(tokens)).collect(Collectors.toList());

                    // Same syntax should not mean two different things
                    if (passed.size() > 1) {
                        throw new Exception("INTERNAL_PARSER_ERROR: Syntax matches multiple known patterns");
                    }

                    // Since there cannot be more than one passed pattern the first pattern is chosen
                    best = passed.get(0);
                }
                else {
                    // Try if any pattern passes token list
                    final Optional<Pattern> pattern = patterns.getPatterns().stream()
                            .findFirst().filter(p -> p.passes(tokens));

                    if (pattern.isPresent()) {
                        best = pattern.get();
                    }
                }
            }

            // When there are no more entries best pattern must be chosen
            if (!patterns.hasEntries()) {

                if (best == null) {
                    throw new Exception("Syntax doesn't match any known patterns");
                }

                instructions.addAll(best.build(tokens));

                // Reset the filtering
                patterns = Patterns.getRoot();
                tokens.clear();
                best = null;
            }
        }

        return instructions;
    }
}
