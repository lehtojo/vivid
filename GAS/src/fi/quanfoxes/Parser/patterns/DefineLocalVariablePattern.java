package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Instruction;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.instructions.CreateLocalVariableInstruction;

import java.util.Arrays;
import java.util.List;

public class DefineLocalVariablePattern extends Pattern {

    public DefineLocalVariablePattern() {
        super(TokenType.DATA_TYPE, TokenType.NAME, TokenType.OPERATOR);
    }

    @Override
    public boolean passes(final List<Token> tokens) {
        return ((OperatorToken)tokens.get(2)).getOperator() == OperatorType.ASSIGN;
    }

    @Override
    public List<Instruction> build(final List<Token> tokens) {
        final DataTypeToken dataTypeToken = (DataTypeToken)tokens.get(0);
        final NameToken nameToken = (NameToken)tokens.get(1);

        return Arrays.asList(
            new CreateLocalVariableInstruction(dataTypeToken.getDataType(), nameToken.getName())
        );
    }
}
