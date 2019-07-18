package fi.quanfoxes;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.*;
import org.junit.Test;

import static org.junit.Assert.assertEquals;

import java.util.ArrayList;
import java.util.List;

public class ParserTest {

    public void members(Node root, List<Exception> errors) throws Exception {
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

                members(type, errors);
            }

            node = node.getNext();
        }
    }

    public static void functions(Node parent, List<Exception> errors) throws Exception {
        Node node = parent.getFirst();

        while (node != null) {
            if (node instanceof TypeNode) {
                TypeNode type = (TypeNode)node;
                functions(type, errors);

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

    private class Result {
        public Context context = new Context();
        public Node root = new Node();
        public List<Exception> errors = new ArrayList<>();
    }

    public Result parse(String... sources) throws Exception {
        ArrayList<ArrayList<Token>> sections = new ArrayList<>();
        
        for (String source : sources) {
            sections.add(Lexer.getTokens(source));
        }

        Result result = new Result();

        for (ArrayList<Token> tokens : sections) {
            Parser.parse(result.root, result.context, tokens);
        }
        
        members(result.root, result.errors);

        if (result.errors.size() > 0) {
            return result;
        }

        functions(result.root, result.errors);

        return result;
    }

    @Test
    public void unarySign() throws Exception {
        Result result = parse("short mass = -777 * 2");

        // Verify -------------------------------------
        Context context = Parser.initialize();
        Node root = new Node();

        // short mass
        Variable variable = new Variable(context, context.getType("short"), "mass", AccessModifier.PUBLIC);
        VariableNode mass = new VariableNode(variable);

        // -777 * 2
        OperatorNode multiply = new OperatorNode(OperatorType.MULTIPLY);
        
        // -777
        NumberNode negative = new NumberNode(NumberType.UINT16, 777);
        NegateNode negate = new NegateNode(negative);

        // 2
        NumberNode two = new NumberNode(NumberType.UINT8, 2);

        multiply.add(negate);
        multiply.add(two);

        // short mass = -777 * 2
        OperatorNode operator = new OperatorNode(OperatorType.ASSIGN);
        operator.add(mass);
        operator.add(multiply);

        root.add(operator);

        assertEquals(context, result.context);
        assertEquals(root, result.root);
    }

    @Test
    public void simpleLocalVariableMath() throws Exception {
        String code = "num a = 3 + 5 * 7";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        Context context = Parser.initialize();
        Node root = new Node();
        Parser.parse(root, context, tokens);
    }

    @Test
    public void multiVariableMath() throws Exception {
        String code = "num a = 3 + 5 * 7 " +
                      "long b = a / 6 / 5 - 4 ";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        Context context = Parser.initialize();
        Node root = new Node();
        Parser.parse(root, context, tokens);
    }

    @Test
    public void simpleType () throws Exception {
        String code = "public type spoon {}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        Context context = Parser.initialize();
        Node root = new Node();
        Parser.parse(root, context, tokens);
    }

    @Test
    public void typeWithMemberVariables () throws Exception {
        String code = "public type spoon {" +
                      "   public num a" +
                      "   private tiny b" +
                      "}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        Context context = Parser.initialize();
        Node root = new Node();
        Parser.parse(root, context, tokens, Parser.MEMBERS, Parser.MAX_PRIORITY);

        ArrayList<Exception> errors = new ArrayList<>();
        members(root, errors);

        if (errors.size() > 0) {
            throw new Exception(errors.stream().toString());
        }

        functions(root, errors);

        if (errors.size() > 0) {
            throw new Exception(errors.stream().toString());
        }
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

        Context context = Parser.initialize();
        Node root = new Node();
        Parser.parse(root, context, tokens, Parser.MEMBERS, Parser.MAX_PRIORITY);

        ArrayList<Exception> errors = new ArrayList<>();
        members(root, errors);

        if (errors.size() > 0) {
            throw new Exception(errors.stream().toString());
        }

        functions(root, errors);

        if (errors.size() > 0) {
            throw new Exception(errors.stream().toString());
        }
    }
}
