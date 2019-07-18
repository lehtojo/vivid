package fi.quanfoxes;

import fi.quanfoxes.lexer.Lexer;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.patterns.TypePattern;

import java.io.*;
import java.util.ArrayList;
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

    public static ArrayList<Future<Integer>> functions(Node parent) throws Exception {
        ArrayList<Future<Integer>> tasks = new ArrayList<>();
        Node node = parent.getFirst();

        while (node != null) {
            if (node instanceof TypeNode) {
                TypeNode type = (TypeNode)node;
                functions(type);

            } else if (node instanceof FunctionNode) {
                FunctionNode function = (FunctionNode)node;
                
                tasks.add(executors.submit(() -> {
                    try {
                        function.parse();
                    } catch (Exception e) {
                        errors.add(e);
                        return 1;
                    }        

                    return 0;
                }));
            }

            node = node.getNext();
        }

        return tasks;
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

        // Create a root node for storing global types, functions and variables
        Context context = Parser.initialize();
        Node root = new Node();

        // Form types and subtypes
        for (Future<ArrayList<Token>> section : sections) {
            Parser.parse(root, context, section.get(), Parser.MEMBERS, Parser.MAX_PRIORITY);
        }

        // Parse member variables and functions in all types
        members(root);
        
        if (!errors.isEmpty()) {
            errors.forEach(e -> System.out.println("ERROR: " + e.getMessage()));
            System.exit(-3);
            return;
        }

        // Parse function bodies
        wait(functions(root));

        if (!errors.isEmpty()) {
            errors.forEach(e -> System.out.println("ERROR: " + e.getMessage()));
            System.exit(-3);
            return;
        }

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
