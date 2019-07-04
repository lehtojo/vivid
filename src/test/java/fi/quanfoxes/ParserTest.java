package fi.quanfoxes;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.nodes.*;
import org.junit.Test;

import java.util.ArrayList;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class ParserTest {

    @Test
    public void unarySign() throws Exception {
        String code = "short mass = -777 * 2";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens);
    }

    @Test
    public void simpleLocalVariableMath() throws Exception {
        String code = "num a = 3 + 5 * 7";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens);
    }

    @Test
    public void multiVariableMath() throws Exception {
        String code = "num a = 3 + 5 * 7 " +
                      "long b = a / 6 / 5 - 4 ";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens);
    }

    @Test
    public void simpleType () throws Exception {
        String code = "public type spoon {}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens);
    }

    @Test
    public void typeWithMemberVariables () throws Exception {
        String code = "public type spoon {" +
                      "   public num a" +
                      "   private tiny b" +
                      "}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens, Parser.STRUCTURAL_MIN_PRIORITY, Parser.STRUCTURAL_MAX_PRIORITY);
    }

    @Test
    public void typeWithMemberVariablesAndFunctions () throws Exception {
        String code = "public type spoon {" +
                      "   public num a" +
                      "   private tiny b" +
                      "   " +
                      "   public func getSum() {" +
                      "      return a + b" +
                      "   }" +
                      "}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.initialize();
        Parser.parse(root, tokens, Parser.STRUCTURAL_MIN_PRIORITY, Parser.STRUCTURAL_MAX_PRIORITY);

        Runtime runtime = Runtime.getRuntime();
        ExecutorService executors = Executors.newFixedThreadPool(runtime.availableProcessors());

        ContextNode context = (ContextNode)root.getFirstChild();

        while (context != null) {


            context = (ContextNode)context.getNext();
        }

        TypeNode type = root.getType("spoon");
        type.getFunction("getSum").parse();
    }
}
