package fi.quanfoxes.Parser.instructions;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Parser.Instruction;

public class OperatorInstruction extends Instruction {
    private Token left;
    private Token right;

    public OperatorInstruction(Token left, Token right) {
        this.left = left;
        this.right = right;
    }

    public Token getLeft() {
        return left;
    }

    public Token getRight() {
        return right;
    }
}
