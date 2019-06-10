package tests;

import fi.quanfoxes.Lexer.Lexer;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class LexerTest {

    public void assertTokenArea (String input, int start, Lexer.TextType exceptedType, int exceptedStart, int exceptedEnd) throws Exception {
        Lexer lexer = new Lexer();
        Lexer.TokenArea area = lexer.getNextTokenArea(input, start);

        assertSame(exceptedType, area.type);
        assertSame(exceptedStart, area.start);
        assertSame(exceptedEnd, area.end);
    }

    @Test
    public void tokenArea_typeDetectionStart() throws Exception {
        assertTokenArea("num a = 0;", 0, Lexer.TextType.TEXT, 0, 3);
    }

    @Test
    public void tokenArea_numberDetection() throws Exception {
        assertTokenArea("num a = 1234;", 8, Lexer.TextType.NUMBER, 8, 12);
    }

    @Test
    public void tokenArea_decimalNumberDetection() throws Exception {
        assertTokenArea("num a = 1.234;", 8, Lexer.TextType.NUMBER, 8, 13);
    }
    @Test
    public void tokenArea_assignOperator() throws Exception {
        assertTokenArea("num a = 1.234;", 5, Lexer.TextType.OPERATOR, 6, 7);
    }

    @Test
    public void tokenArea_simpleFunction() throws Exception {
        assertTokenArea("apple() * apple();", 0, Lexer.TextType.FUNCTION, 0, 7);
    }

    @Test
    public void tokenArea_functionWithParameters() throws Exception {
        assertTokenArea("num a = banana(1 + 2 * (3 - 4));", 7, Lexer.TextType.FUNCTION, 8, 31);
    }

    @Test
    public void tokenArea_richFunctionName() throws Exception {
        assertTokenArea("a = this_Is-Very_Weird_Function(apple() + banana() * 3 / 2) % 2;", 3, Lexer.TextType.FUNCTION,4, 59);
    }

    @Test
    public void tokenArea_hexadecimal() throws Exception {
        assertTokenArea("0xFF;", 0, Lexer.TextType.NUMBER, 0, 4);
    }

    @Test
    public void tokenArea_simpleContent() throws Exception {
        assertTokenArea("decimal b = apple() + (4 * banana() + 5) & orange();", 21, Lexer.TextType.CONTENT, 22, 40);
    }

    @Test
    public void tokenArea_operatorAndContent() throws Exception {
        assertTokenArea("c = 5*(4+a);", 5, Lexer.TextType.OPERATOR, 5, 6);
    }

    @Test
    public void tokenArea_noClosingParenthesis()  {
        try {
            assertTokenArea("c = (banana() + apple(orange() * 5) % 3", 4, Lexer.TextType.CONTENT, 4, 35);
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
            assertTokenArea("a = 4(3/a);", 4, Lexer.TextType.NUMBER, 4, 5);
        }
        catch (Exception e) {
            assertSame(1, 1);
            return;
        }

        assertSame(1, 2);
    }
}
