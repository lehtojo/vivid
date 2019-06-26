package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Parser.Instruction;

public class OperatorInstruction extends Instruction {
    private Token source;
    private Token destination;

    public OperatorInstruction(Token source, Token destination) {
        this.source = source;
        this.destination = destination;
    }

    public Token getLeft() {
        return source;
    }

    public Token getRigth() {
        return destination;
    }
}
