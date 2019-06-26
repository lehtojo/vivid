package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;

public class AddInstruction extends OperatorInstruction {
    public AddInstruction(Token source, Token destination) {
        super(source, destination);
    }
}
