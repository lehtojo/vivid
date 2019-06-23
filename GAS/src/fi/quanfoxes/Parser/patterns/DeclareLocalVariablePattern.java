package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Instruction;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.instructions.CreateLocalVariableInstruction;

import java.util.Collections;
import java.util.List;

public class DeclareLocalVariablePattern extends Pattern {
    public DeclareLocalVariablePattern() {
        super(TokenType.DATA_TYPE, TokenType.NAME);
    }

    @Override
    public boolean passes(final List<Token> tokens) {
        return true;
    }

    @Override
    public List<Instruction> build(final List<Token> tokens) {
        final DataTypeToken dataTypeToken = (DataTypeToken)tokens.get(0);
        final NameToken nameToken = (NameToken)tokens.get(1);

        return Collections.singletonList(
                new CreateLocalVariableInstruction(dataTypeToken.getDataType(), nameToken.getName())
        );
    }
}
