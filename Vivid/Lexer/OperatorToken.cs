using System;
using System.Collections.Generic;

public class OperatorToken : Token
{
	public Operator Operator { get; private set; }
	public Position End => Position.Translate(Operator.Identifier.Length);

	public OperatorToken(string identifier) : base(TokenType.OPERATOR)
	{
		Operator = Operators.Get(identifier);
	}

	public OperatorToken(Operator operation) : base(TokenType.OPERATOR)
	{
		Operator = operation;
	}

	public override bool Equals(object? other)
	{
		return other is OperatorToken token &&
			   base.Equals(other) &&
			   EqualityComparer<Operator>.Default.Equals(Operator, token.Operator);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Operator);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return Operator.Identifier;
	}
}
