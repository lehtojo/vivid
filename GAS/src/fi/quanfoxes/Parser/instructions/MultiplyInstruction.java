package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;

public class MultiplyInstruction extends OperatorInstruction {
    public MultiplyInstruction(Token left, Token right) {
        super(left, right);
    }
}
