import fi.quanfoxes.DataTypes;
import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.nodes.*;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;

public class ParserTest {

    @Test
    public void a () throws Exception {
        String code = "public type spoon {}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.getRoot();
        Parser.parse(root, tokens);
    }

    @Test
    public void b () throws Exception {
        DataTypes.initialize();

        String code = "public type spoon {" +
                      "   public num a" +
                      "   private tiny b" +
                      "}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.getRoot();
        Parser.parse(root, tokens, Parser.STRUCTURAL_MIN_PRIORITY, Parser.STRUCTURAL_MAX_PRIORITY);
    }

    @Test
    public void c () throws Exception {
        DataTypes.initialize();

        String code = "public type spoon {" +
                      "   public num a" +
                      "   private tiny b" +
                      "   " +
                      "   public func getSum() {" +
                      "      return a + b" +
                      "   }" +
                      "}";

        ArrayList<Token> tokens = Lexer.getTokens(code);

        ContextNode root = Parser.getRoot();
        Parser.parse(root, tokens, Parser.STRUCTURAL_MIN_PRIORITY, Parser.STRUCTURAL_MAX_PRIORITY);

        TypeNode type = root.getType("spoon");
        type.getFunction("getSum").parse();
    }

    /*@Test
    public void simpleNumVariable () throws Exception {
        ArrayList<Token> tokens = new ArrayList<>(Arrays.asList(
            new DataTypeToken("num"),
            new IdentifierToken("a"),
            new OperatorToken(OperatorType.ASSIGN),
            new NumberToken(7)
        ));

        ContextNode context = new ContextNode();
        Parser.parse(context, tokens);

        // Verify ----------------------------------
        ContextNode excepted = new ContextNode();

        VariableNode variable = new VariableNode("a", DataTypes.get("num"));
        excepted.declare(variable);

        VariableNode left = new VariableNode(variable);
        NumberNode right = new NumberNode(NumberType.UINT8, 7);

        OperatorNode assign = new OperatorNode(OperatorType.ASSIGN);
        assign.add(left);
        assign.add(right);

        excepted.add(assign);

        //assertEquals(excepted, actual);
    }

    @Test
    public void advancedNumVariable () throws Exception {
        // normal apple_count = 80000
        // num apples_per_person = 1 + apple_count / 40000

        ArrayList<Token> tokens = new ArrayList<>(Arrays.asList(
                new DataTypeToken("normal"),
                new IdentifierToken("apple_count"),
                new OperatorToken(OperatorType.ASSIGN),
                new NumberToken(80000),

                new DataTypeToken("num"),
                new IdentifierToken("apples_per_person"),
                new OperatorToken(OperatorType.ASSIGN),
                new NumberToken(1),
                new OperatorToken(OperatorType.ADD),
                new IdentifierToken("apple_count"),
                new OperatorToken(OperatorType.DIVIDE),
                new NumberToken(40000)
        ));

        ProgramNode actual = Parser.parse(tokens);

        // Verify ----------------------------------
        ProgramNode excepted = new ProgramNode();

        // Variables
        Variable appleCountVariable = new Variable("apple_count", DataTypes.get("normal"));
        excepted.declare(appleCountVariable);

        Variable applesPerPersonVariable = new Variable("apples_per_person", DataTypes.get("num"));
        excepted.declare(applesPerPersonVariable);

        // First line
        OperatorNode assignAppleCount = new OperatorNode(OperatorType.ASSIGN);

        VariableNode appleCount = new VariableNode(appleCountVariable);
        NumberNode constantAppleCount = new NumberNode(NumberType.UINT32, 80000);

        assignAppleCount.add(appleCount);
        assignAppleCount.add(constantAppleCount);

        excepted.add(assignAppleCount);

        // Second line
        OperatorNode divide = new OperatorNode(OperatorType.DIVIDE);

        VariableNode appleCountDivision = new VariableNode(appleCountVariable);
        NumberNode constantPersonCount = new NumberNode(NumberType.UINT16, 40000);

        divide.add(appleCountDivision);
        divide.add(constantPersonCount);

        // Add
        OperatorNode add = new OperatorNode(OperatorType.ADD);

        NumberNode constantExtraApple = new NumberNode(NumberType.UINT8, 1);

        add.add(constantExtraApple);
        add.add(divide);

        // Assign
        OperatorNode assignPerPerson = new OperatorNode(OperatorType.ASSIGN);

        VariableNode applesPerPerson = new VariableNode(applesPerPersonVariable);

        assignPerPerson.add(applesPerPerson);
        assignPerPerson.add(add);

        excepted.add(assignPerPerson);

        assertEquals(excepted, actual);
    }*/
}
