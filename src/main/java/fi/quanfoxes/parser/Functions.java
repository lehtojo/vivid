package fi.quanfoxes.parser;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.stream.IntStream;

public class Functions {
    private List<Function> functions = new ArrayList<>();

    /**
     * Adds function with same name to this function list
     */
    public void add(Function function) {
        functions.add(function);
    }

    /**
     * Updates all functions
     */
    public void update() {
        for (Function function : functions) {
            function.update();
        }
    }

    private class Candidate {
        public Function function;
        public int casts;
    }

    /**
     * Tries to find the matching function by comparing parameters.
     * When there are two or more callable functions with the given parameter list, the function with least casts is chosen
     * @param parameters Parameter list used to filter
     * @return Success: Function with same or least castable parameters, Failure: null
     */
    public Function get(List<Type> parameters) {
        List<Candidate> candidates = new ArrayList<>();

        for (Function function : functions) {
            List<Type> types = function.getParameters();

            // Verify that the current function has equal amount of parameters with the given parameter list
            if (parameters.size() != types.size()) {
                continue;
            }

            // Verify each given parameter is atleast castable to the required parameter type of the current function
            if (IntStream.range(0, parameters.size()).anyMatch(i -> Resolver.getSharedType(parameters.get(i), types.get(i)) == null)) {
                continue; 
            }

            Candidate candidate = new Candidate();
            candidate.function = function;
            candidate.casts = IntStream.range(0, parameters.size()).map(i -> parameters.get(i) == types.get(i) ? 0 : 1).sum();

            candidates.add(candidate);
        }

        if (candidates.isEmpty()) {
            return null;
        }

        // Return the candidate which has the minium amount of casts in terms of parameters
        return candidates.stream().min(Comparator.comparingInt(i -> i.casts)).get().function;
    }

    /**
     * Returns all functions
     */
    public List<Function> getFunctions() {
        return functions;
    }
}