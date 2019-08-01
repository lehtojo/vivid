package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.Errors;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.Operators;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Label;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.nodes.LabelNode;

public class LabelPattern extends Pattern {
    public static final int PRIORITY = 15;

    private static final int NAME = 0;
    private static final int OPERATOR = 1;

    public LabelPattern() {
        super(TokenType.IDENTIFIER, TokenType.OPERATOR);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        return operator.getOperator() == Operators.EXTENDER;
    }

	@Override
	public Node build(Context context, List<Token> tokens) throws Exception {
        IdentifierToken name = (IdentifierToken)tokens.get(NAME);

		if (context.isLocalLabelDeclared(name.getValue())) {
            throw Errors.get(name.getPosition(), String.format("Label '%s' already exists in the current context", name.getValue()));
        }

        Label label = new Label(context, name.getValue());
        return new LabelNode(label);
	}
}