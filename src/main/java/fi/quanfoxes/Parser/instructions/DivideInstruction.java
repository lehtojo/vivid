package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;

public class DivideInstruction extends OperatorInstruction {
    public DivideInstruction(Token left, Token right) {
        super(left, right);
    }
}
