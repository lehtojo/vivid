package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;

public class DivideInstruction extends OperatorInstruction {
    public DivideInstruction(Token source, Token destination) {
        super(source, destination);
    }
}
