package fi.quanfoxes.phases;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.NodeType;
import fi.quanfoxes.parser.nodes.TypeNode;

public class ParserPhase extends Phase {
    public static class Parse {
        private Context context;
        private Node node;

        public Parse(Context context, Node node) {
            this.context = context;
            this.node = node;
        }

        public Context getContext() {
            return context;
        }

        public Node getNode() {
            return node;
        }
    }

    private List<Exception> errors = new ArrayList<>();

    private void members(Node root) {
        Node node = root.first();

        while (node != null) {
            if (node.getNodeType() == NodeType.TYPE_NODE) {
                final TypeNode type = (TypeNode) node;

                async(() -> {
                    try {
                        type.parse();
                    }
                    catch (Exception e) {
                        errors.add(e);
                    }

                    return Status.OK;
                });    

                members(type);
            }

            node = node.next();
        }
    }

    public void functions(Node parent) {
        Node node = parent.first();

        while (node != null) {
            if (node.getNodeType() == NodeType.TYPE_NODE) {
                TypeNode type = (TypeNode)node;
                functions(type);

            } else if (node.getNodeType() == NodeType.FUNCTION_NODE) {
                final FunctionNode function = (FunctionNode)node;

                async(() -> {
                    try {
                        function.parse();
                    } catch (Exception e) {
                        errors.add(e);
                    }

                    return Status.OK;
                });
            }

            node = node.next();
        }
    }

    @Override
    public Status execute(Bundle bundle) {
        final List<Token>[] files = bundle.get("input_file_tokens", null);

        if (files == null) {
            return Status.error("Nothing to parse");
        }

        final Parse[] parses = new Parse[files.length];

        // Form the 'hull' of the code
        for (int i = 0; i < files.length; i++) {
            final int index = i;

            async(() -> {
                List<Token> tokens = files[index];

                Node node = new Node();
                Context context = Parser.initialize();

                try {
                    Parser.parse(node, context, tokens);
                }
                catch (Exception e) {
                    return Status.error(e.getMessage());
                }

                parses[index] = new Parse(context, node);
                
                return Status.OK;
            });
        }

        sync();

        // Parse types, subtypes and their members
        for (int i = 0; i < files.length; i++) {
            final int index = i;

            async(() -> {
                members(parses[index].getNode());
                return Status.OK;
            });
        }

        sync();

        // Parse types, subtypes and their members
        for (int i = 0; i < files.length; i++) {
            final int index = i;

            async(() -> {
                functions(parses[index].getNode());
                return Status.OK;
            });
        }
        
        sync();

        // Merge all parsed files
        Context context = new Context();
        Node root = new Node();

        for (Parse parse : parses) {
            context.merge(parse.getContext());
            root.merge(parse.getNode());
        }

        bundle.put("parse", new Parse(context, root));

        return Status.OK;
    }
}