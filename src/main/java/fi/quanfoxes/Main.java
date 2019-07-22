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

    public static <T> void wait(ArrayList<Future<T>> tasks) {
        int i = 0;

        while (i < tasks.size()) {
            i += tasks.get(i).isDone() ? 1 : 0;
        }
    }

    public static void members(Node root) throws Exception {
        Node node = root.getFirst();

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

            node = node.getNext();
        }
    }

    public static void functions(Node parent) throws Exception {
        Node node = parent.getFirst();

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

            node = node.getNext();
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
        
        long start = System.nanoTime();

        // Create thread pool for multi-threading
        Runtime runtime = Runtime.getRuntime();
        executors = Executors.newFixedThreadPool(runtime.availableProcessors());

        ArrayList<Future<String>> files = new ArrayList<>();

        // Load source files
        for (String filename : args) {
            Future<String> file = executors.submit(() -> load(filename));
            files.add(file);
        }

        // Wait for all threads to finish
        wait(files);

        // Make sure all files are loaded successfully
        for (Future<String> file : files) {
            if (file.get() == null) {
                System.exit(FILE_LOAD_ERROR);
                return;
            }
        }

        long lexer_start = System.nanoTime();

        ArrayList<Future<ArrayList<Token>>> sections = new ArrayList<>();

        // Tokenize each source file
        for (Future<String> file : files) {
            Future<ArrayList<Token>> section = executors.submit(() -> Lexer.getTokens(file.get()));
            sections.add(section);
        }

        // Wait for all threads to finish
        wait(sections);

        // Make sure all files are tokenized successfully
        for (Future<ArrayList<Token>> section : sections) {
            if (section.get() == null) {
                System.exit(LEXER_ERROR);
                return;
            }
        }

        long parser_start = System.nanoTime();

        List<Parse> parses = new ArrayList<>();

        for (Future<ArrayList<Token>> section : sections) {
            Context context = Parser.initialize();
            Node root = new Node();

            Parser.parse(root, context, section.get(), Parser.MEMBERS, Parser.MAX_PRIORITY);
            
            members(root);  
            functions(root);

            parses.add(new Parse(context, root));
        }

        Context context = new Context();
        Node root = new Node();

        for (Parse parse : parses) {
            context.merge(parse.getContext());
            root.add(parse.getNode());
        }

        Resolver.resolve(context, root, errors);

        long end = System.nanoTime();

        System.out.println(              "=====================");
        System.out.println(String.format("Disk: %.1f ms", (lexer_start - start) / 1000000.0f));
        System.out.println(String.format("Lexer: %.1f ms", (parser_start - lexer_start) / 1000000.0f));
        System.out.println(String.format("Parser: %.1f ms", (end - parser_start) / 1000000.0f));
        System.out.println(String.format("Total: %.1f ms", (end - start) / 1000000.0f));
        System.out.println(              "=====================");

        System.exit(0);
    }
}
