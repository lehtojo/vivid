using System;
using System.Collections.Generic;

public class OperatorToken : Token
{
	public Operator Operator { get; private set; }

	public OperatorToken(string identifier) : base(TokenType.OPERATOR)
	{
		Operator = Operators.Get(identifier);
	}

	public OperatorToken(Operator operation) : base(TokenType.OPERATOR)
	{
		Operator = operation;
	}

	public override bool Equals(object? obj)
	{
		return obj is OperatorToken token &&
			   base.Equals(obj) &&
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
}
