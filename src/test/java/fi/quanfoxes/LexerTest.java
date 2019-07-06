package fi.quanfoxes;

import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.*;
import org.junit.Test;

import java.util.Arrays;
import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertSame;

public class LexerTest {

    public void assertTokenArea (String input, int start, Lexer.Type exceptedType, int exceptedStart, int exceptedEnd) throws Exception {
        Lexer.Area area = Lexer.getNextToken(input, start);

        assertSame(exceptedType, area.type);
        assertSame(exceptedStart, area.start);
        assertSame(exceptedEnd, area.end);
    }

    @Test
    public void tokenArea_typeDetectionStart() throws Exception {
        assertTokenArea("num a = 0;", 0, Lexer.Type.TEXT, 0, 3);
    }

    @Test
    public void tokenArea_numberDetection() throws Exception {
        assertTokenArea("num a = 1234;", 8, Lexer.Type.NUMBER, 8, 12);
    }

    @Test
    public void tokenArea_decimalNumberDetection() throws Exception {
        assertTokenArea("num a = 1.234;", 8, Lexer.Type.NUMBER, 8, 13);
    }
    @Test
    public void tokenArea_assignOperator() throws Exception {
        assertTokenArea("num a = 1.234;", 5, Lexer.Type.OPERATOR, 6, 7);
    }

    @Test
    public void tokenArea_simpleFunction() throws Exception {
        assertTokenArea("apple() * apple();", 0, Lexer.Type.TEXT, 0, 5);
    }

    @Test
    public void tokenArea_functionWithParameters() throws Exception {
        assertTokenArea("num a = banana(1 + 2 * (3 - 4));", 7, Lexer.Type.TEXT, 8, 14);
    }

    @Test
    public void tokenArea_richFunctionName() throws Exception {
        assertTokenArea("a = this_Is_Very_Weird_Function(apple() + banana() * 3 / 2) % 2;", 3, Lexer.Type.TEXT,4, 31);
    }

    @Test
    public void tokenArea_hexadecimal() throws Exception {
        assertTokenArea("0xFF;", 0, Lexer.Type.NUMBER, 0, 4);
    }

    @Test
    public void tokenArea_simpleContent() throws Exception {
        assertTokenArea("decimal b = apple() + (4 * banana() + 5) & orange();", 21, Lexer.Type.CONTENT, 22, 40);
    }

    @Test
    public void tokenArea_operatorAndContent() throws Exception {
        assertTokenArea("c = 5*(4+a);", 5, Lexer.Type.OPERATOR, 5, 6);
    }

    @Test
    public void tokenArea_noClosingParenthesis()  {
        try {
            assertTokenArea("c = (banana() + apple(orange() * 5) % 3", 4, Lexer.Type.CONTENT, 4, 35);
        }
        catch (Exception e) {
            assertSame(1, 1);
            return;
        }

        assertSame(1, 2);
    }

    @Test
    public void tokenArea_missingOperator()  {
        try {
            assertTokenArea("a = 4(3/a);", 4, Lexer.Type.NUMBER, 4, 5);
        }
        catch (Exception e) {
            assertSame(1, 1);
            return;
        }

        assertSame(1, 2);
    }

    @Test
    public void tokens_math () throws Exception {
        List<Token> actual = Lexer.getTokens("num a = 2 * b");
        List<Token> excepted = Arrays.asList
        (
            new IdentifierToken("num"),
            new IdentifierToken("a"),
            new OperatorToken(OperatorType.ASSIGN),
            new NumberToken((byte)2),
            new OperatorToken(OperatorType.MULTIPLY),
            new IdentifierToken("b")
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_math_functions () throws Exception {
        List<Token> actual = Lexer.getTokens("num a = banana() + apple(5 % b)");
        List<Token> excepted = Arrays.asList
        (
                new IdentifierToken("num"),
                new IdentifierToken("a"),
                new OperatorToken(OperatorType.ASSIGN),
                new FunctionToken(new IdentifierToken("banana"),
                new ContentToken("()")),
                new OperatorToken(OperatorType.ADD),
                new FunctionToken(new IdentifierToken("apple"),
                new ContentToken(
                    new NumberToken((byte)5),
                    new OperatorToken(OperatorType.MODULUS),
                    new IdentifierToken("b")
                ))
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_math_functions_and_content () throws Exception {
        List<Token> actual = Lexer.getTokens("num variable = banana() * ( apple() and 3 or 55 xor 777 )");
        List<Token> excepted = Arrays.asList
        (
                new IdentifierToken("num"),
                new IdentifierToken("variable"),
                new OperatorToken(OperatorType.ASSIGN),
                new FunctionToken(new IdentifierToken("banana"),
                new ContentToken("()")),
                new OperatorToken(OperatorType.MULTIPLY),
                new ContentToken(
                    new FunctionToken(new IdentifierToken("apple"),
                    new ContentToken("()")),
                    new OperatorToken(OperatorType.BITWISE_AND),
                    new NumberToken(3),
                    new OperatorToken(OperatorType.BITWISE_OR),
                    new NumberToken(55),
                    new OperatorToken(OperatorType.BITWISE_XOR),
                    new NumberToken(777)
                )
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_keyword () throws Exception {

        List<Token> actual = Lexer.getTokens("type banana");
        List<Token> excepted = Arrays.asList
        (
            new KeywordToken(Keywords.get("type")),
            new IdentifierToken("banana")
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_loop () throws Exception {

        List<Token> actual = Lexer.getTokens("loop (3)");
        List<Token> excepted = Arrays.asList
        (
            new KeywordToken(Keywords.get("loop")),
            new ContentToken(new NumberToken(3))
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_if () throws Exception {

        List<Token> actual = Lexer.getTokens("if (a <= b)");
        List<Token> excepted = Arrays.asList
        (
            new KeywordToken(Keywords.get("if")),
            new ContentToken(
                    new IdentifierToken("a"),
                    new OperatorToken(OperatorType.LESS_OR_EQUAL),
                    new IdentifierToken("b"))
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_advanced_if () throws Exception {

        List<Token> actual = Lexer.getTokens("if (a > b && (a < (c + apple(d / e, f % banana()))))");
        List<Token> excepted = Arrays.asList
        (
                new KeywordToken(Keywords.get("if")),
                new ContentToken(
                        new IdentifierToken("a"),
                        new OperatorToken(OperatorType.GREATER_THAN),
                        new IdentifierToken("b"),
                        new OperatorToken(OperatorType.AND),
                        new ContentToken(
                                new IdentifierToken("a"),
                                new OperatorToken(OperatorType.LESS_THAN),
                                new ContentToken(
                                        new IdentifierToken("c"),
                                        new OperatorToken(OperatorType.ADD),
                                        new FunctionToken(
                                                new IdentifierToken("apple"),
                                                new ContentToken(
                                                        new ContentToken(
                                                                new IdentifierToken("d"),
                                                                new OperatorToken(OperatorType.DIVIDE),
                                                                new IdentifierToken("e")),
                                                        new ContentToken(
                                                            new IdentifierToken("f"),
                                                            new OperatorToken(OperatorType.MODULUS),
                                                            new FunctionToken(
                                                                new IdentifierToken("banana"),
                                                                new ContentToken("()"))))))))
        );

        assertEquals(excepted, actual);
    }

    @Test
    public void tokens_unrecognized_token () throws Exception {

        try {
            Lexer.getTokens("num a = b ` c");
        }
        catch (Exception e) {
            assertSame(e.getMessage(), "Unrecognized token");
            return;
        }

        assertSame(1, 2);
    }
}
