using System.Collections.Generic;
public class LabelPattern : Pattern
{
	public const int PRIORITY = 15;

	private const int NAME = 0;
	private const int OPERATOR = 1;

	public LabelPattern() : base(TokenType.IDENTIFIER, TokenType.OPERATOR) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		OperatorToken @operator = (OperatorToken)tokens[OPERATOR];
		return @operator.Operator == Operators.EXTENDER;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		IdentifierToken name = (IdentifierToken)tokens[NAME];

		if (context.IsLocalLabelDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Label '{name.Value}' already exists in the current context");
		}

		Label label = new Label(context, name.Value);
		return new LabelNode(label);
	}
}