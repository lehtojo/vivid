package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;

public class MultiplyInstruction extends OperatorInstruction {
    public MultiplyInstruction(Token source, Token destination) {
        super(source, destination);
    }
}
