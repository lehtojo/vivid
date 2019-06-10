package fi.quanfoxes;

import java.io.*;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.List;
import java.util.Scanner;


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

    public static void main(String[] args)
    {
        System.out.println(System.getProperty("user.dir"));
        Scanner input = new Scanner(System.in);
        fileName = input.nextLine();
        buffer = readFile(fileName);
    }
}
