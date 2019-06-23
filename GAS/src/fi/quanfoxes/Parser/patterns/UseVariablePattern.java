package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.NameToken;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Instruction;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.instructions.BindVariableInstruction;

import java.util.Collections;
import java.util.List;

public class UseVariablePattern extends Pattern {
    public UseVariablePattern() {
        super(TokenType.NAME);
    }

    @Override
    public boolean passes(final List<Token> tokens) {
        return true;
    }

    @Override
    public List<Instruction> build(final List<Token> tokens) {
        final NameToken nameToken = (NameToken)tokens.get(0);

        return Collections.singletonList(
                new BindVariableInstruction(nameToken.getName())
        );
    }
}
