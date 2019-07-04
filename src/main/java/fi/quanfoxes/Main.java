package fi.quanfoxes;

import fi.quanfoxes.Lexer.Lexer;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.patterns.MemberFunctionPattern;
import fi.quanfoxes.Parser.patterns.MemberVariablePattern;
import fi.quanfoxes.Parser.patterns.TypePattern;

import java.io.*;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;


public class Main {
    public static String fileName;
    public static List<String> buffer;

    public static List<String> readFile(String name)
    {
        try
        {
            File file = new File(name);
            return Files.readAllLines(Paths.get(file.getPath()));

        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
        return null;
    }

    public static void writeFile(String buffer, String name)
    {
        try (FileOutputStream outputStream = new FileOutputStream(name))
        {
            outputStream.write(buffer.getBytes());
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public static String load(String file) {

        try {
            StringBuilder builder = new StringBuilder();
            BufferedReader reader = new BufferedReader(new FileReader(file), 1024);

            String line;

            while ((line = reader.readLine()) != null) {
                builder.append(line).append(" ");
            }

            reader.close();
            return builder.toString();
        }
        catch (Exception e) {
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

    public static void main(String[] args) throws Exception {

        System.out.println("Test");
        
        Runtime runtime = Runtime.getRuntime();
        ExecutorService executors = Executors.newFixedThreadPool(runtime.availableProcessors());

        ArrayList<Future<String>> files = new ArrayList<>();

        for (String filename : args) {
            Future<String> file = executors.submit(() -> load(filename));
            files.add(file);
        }

        wait(files);

        for (Future<String> file : files) {
            if (file.get() == null) {
                return;
            }
        }

        ArrayList<Future<ArrayList<Token>>> sections = new ArrayList<>();

        for (Future<String> file : files) {
            Future<ArrayList<Token>> section = executors.submit(() -> Lexer.getTokens(file.get()));
            sections.add(section);
        }

        wait(sections);

        for (Future<ArrayList<Token>> section : sections) {
            if (section.get() == null) {
                return;
            }
        }

        ContextNode root = Parser.initialize();

        // Form types and subtypes
        for (Future<ArrayList<Token>> section : sections) {
            Parser.parse(root, section.get(), TypePattern.PRIORITY, TypePattern.PRIORITY);
        }

        // Form member variables and functions
        for (Future<ArrayList<Token>> section : sections) {
            Parser.parse(root, section.get(), MemberFunctionPattern.PRIORITY, MemberVariablePattern.PRIORITY);
        }
    }
}
