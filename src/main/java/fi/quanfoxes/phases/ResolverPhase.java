package fi.quanfoxes.phases;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;
import fi.quanfoxes.parser.Aligner;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Processor;
import fi.quanfoxes.parser.Resolver;
import fi.quanfoxes.phases.ParserPhase.Parse;

public class ResolverPhase extends Phase {

    private static void complain(List<Exception> errors) {
        for (Exception error : errors) {
            System.err.printf("Error: %s\n", error.getMessage());
        }
    }

    @Override
    public Status execute(Bundle bundle) {
        Parse parse = bundle.get("parse", null);

        if (parse == null) {
            return Status.error("Nothing to resolve");
        }

        List<Exception> errors = new ArrayList<>();

        Context context = parse.getContext();
        Node node = parse.getNode();

        // Try to resolve any problems in the node tree
        Resolver.resolve(context, node, errors);
        
        if (errors.size() > 0) {
            int previous = errors.size();
            int count = 0;

            while (true) {
                errors.clear();

                // Try to resolve any problems in the node tree
                Resolver.resolve(context, node, errors);

                count = errors.size();

                // Try again only if the amount of errors has decreased
                if (count >= previous) {
                    break;
                }

                previous = count;
            }
        }

        if (errors.size() > 0) {
            complain(errors);
            return Status.error("Compilation error");
        }

        Processor.process(node);
        Aligner.align(context);

        return Status.OK;
    }
}