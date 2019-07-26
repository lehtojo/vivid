package fi.quanfoxes;

import fi.quanfoxes.lexer.Lexer;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Resolver;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

public class Main {
    private static final int FILE_LOAD_ERROR = -1;
    private static final int LEXER_ERROR = -2;
    private static final int PARSE_ERROR = -3;

    public static ExecutorService executors;
    public static ArrayList<Exception> errors = new ArrayList<>();

    public static String load(String file) {

        try {
            StringBuilder builder = new StringBuilder();
            BufferedReader reader = new BufferedReader(new FileReader(file), 1024);

            String line;

            while ((line = reader.readLine()) != null) {
                builder.append(line.replace("\t", "")).append(" ");
            }

            reader.close();
            return builder.toString();
        } catch (Exception e) {
            System.out.println("ERROR: Couldn't load " + file);
            return null;
        }
    }

    public static <T> void wait(List<Future<T>> tasks) {
        int i = 0;

        while (i < tasks.size()) {
            i += tasks.get(i).isDone() ? 1 : 0;
        }
    }

    public static void members(Node root) throws Exception {
        Node node = root.first();

        while (node != null) {
            if (node instanceof TypeNode) {
                TypeNode type = (TypeNode) node;

                try {
                    type.parse();
                }
                catch (Exception e) {
                    errors.add(e);
                }

                members(type);
            }

            node = node.next();
        }
    }

    public static void functions(Node parent) throws Exception {
        Node node = parent.first();

        while (node != null) {
            if (node instanceof TypeNode) {
                TypeNode type = (TypeNode)node;
                functions(type);

            } else if (node instanceof FunctionNode) {
                FunctionNode function = (FunctionNode)node;

                try {
                    function.parse();
                } catch (Exception e) {
                    errors.add(e);
                } 
            }

            node = node.next();
        }
    }

    private static class Parse {
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

    public static void main(String[] args) throws Exception {
        
        long a = System.nanoTime();

        // Create thread pool for multi-threading
        Runtime runtime = Runtime.getRuntime();
        executors = Executors.newFixedThreadPool(runtime.availableProcessors());

        List<Future<String>> files = new ArrayList<>();

        // Load source files
        for (String filename : args) {
            files.add(executors.submit(() -> load(filename)));
        }

        // Wait for all threads to finish
        wait(files);

        // Verify all files are loaded successfully
        for (Future<String> file : files) {
            if (file.get() == null) {
                System.exit(FILE_LOAD_ERROR);
                return;
            }
        }

        long b = System.nanoTime();

        List<Future<List<Token>>> tokenized_files = new ArrayList<>();

        // Tokenize each file
        for (Future<String> file : files) {
            tokenized_files.add(executors.submit(() -> Lexer.getTokens(file.get())));
        }

        // Wait for all threads to finish
        wait(tokenized_files);

        // Verify all files are tokenized successfully
        for (Future<List<Token>> section : tokenized_files) {
            if (section.get() == null) {
                System.exit(LEXER_ERROR);
                return;
            }
        }

        long c = System.nanoTime();

        List<Future<Parse>> parses = new ArrayList<>();

        // Parse each tokenized file
        for (Future<List<Token>> file : tokenized_files) {
            parses.add(executors.submit(() -> {
                Context context = Parser.initialize();
                Node node = Parser.parse(context, file.get());

                members(node);  
                functions(node);

                return new Parse(context, node);
            }));
        }

        // Wait for all threads to finish
        wait(tokenized_files);

        // Verify all files are parsed successfully
        for (Future<Parse> parse : parses) {
            if (parse.get() == null) {
                System.exit(PARSE_ERROR);
                return;
            }
        }

        // Merge all parsed files
        Context context = new Context();
        Node root = new Node();

        for (Future<Parse> iterator : parses) {
            Parse parse = iterator.get();
            context.merge(parse.getContext());
            root.merge(parse.getNode());
        }

        // Try to resolve any problems in the node tree
        Resolver.resolve(context, root, errors);

        long d = System.nanoTime();

        System.out.println(              "=====================");
        System.out.println(String.format("Disk: %.1f ms", (b - a) / 1000000.0f));
        System.out.println(String.format("Lexer: %.1f ms", (c - b) / 1000000.0f));
        System.out.println(String.format("Parser: %.1f ms", (d - c) / 1000000.0f));
        System.out.println(String.format("Total: %.1f ms", (d - a) / 1000000.0f));
        System.out.println(              "=====================");

        System.exit(0);
    }
}
