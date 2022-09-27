using System.Collections.Generic;

namespace Vivid.Unit
{
	public static class LexerTests
	{
		private static List<Token> GetTokens(params Token[] tokens)
		{
			return new List<Token>(tokens);
		}

		public static void StandardMath()
		{
			var actual = Lexer.GetTokens("1 + 2");
			var expected = GetTokens
			(
				new NumberToken(1),
				new OperatorToken(Operators.ADD),
				new NumberToken(2)
			);

			Assert.AreEqual(expected, actual);
		}

		public static void StandardMathAssignment()
		{
			var actual = Lexer.GetTokens("sum = a * b + 5");
			var expected = GetTokens
			(
				new IdentifierToken("sum"),
				new OperatorToken(Operators.ASSIGN),
				new IdentifierToken("a"),
				new OperatorToken(Operators.MULTIPLY),
				new IdentifierToken("b"),
				new OperatorToken(Operators.ADD),
				new NumberToken(5)
			);

			Assert.AreEqual(expected, actual);
		}

		public static void Math()
		{
			var actual = Lexer.GetTokens("1 % a ^ (number / c - 9)");
			var expected = GetTokens
			(
				new NumberToken(1),
				new OperatorToken(Operators.MODULUS),
				new IdentifierToken("a"),
				new OperatorToken(Operators.POWER),
				new ParenthesisToken
				(
					new IdentifierToken("number"),
					new OperatorToken(Operators.DIVIDE),
					new IdentifierToken("c"),
					new OperatorToken(Operators.SUBTRACT),
					new NumberToken(9)
				)
			);

			Assert.AreEqual(expected, actual);
		}

		public static void Assignment()
		{
			var actual = Lexer.GetTokens("a += b -= c + 5 *= d /= e");
			var expected = GetTokens
			(
				new IdentifierToken("a"),
				new OperatorToken(Operators.ASSIGN_ADD),
				new IdentifierToken("b"),
				new OperatorToken(Operators.ASSIGN_SUBTRACT),
				new IdentifierToken("c"),
				new OperatorToken(Operators.ADD),
				new NumberToken(5),
				new OperatorToken(Operators.ASSIGN_MULTIPLY),
				new IdentifierToken("d"),
				new OperatorToken(Operators.ASSIGN_DIVIDE),
				new IdentifierToken("e")
			);

			Assert.AreEqual(expected, actual);
		}

		public static void NestedParenthesis()
		{
			var actual = Lexer.GetTokens("apple + (banana * (orange - dragonfruit ^ (5 + 5)) - (blueberry / cucumber ^ potato))");
			var expected = GetTokens
			(
				new IdentifierToken("apple"),
				new OperatorToken(Operators.ADD),
				new ParenthesisToken
				(
					new IdentifierToken("banana"),
					new OperatorToken(Operators.MULTIPLY),
					new ParenthesisToken
					(
						new IdentifierToken("orange"),
						new OperatorToken(Operators.SUBTRACT),
						new IdentifierToken("dragonfruit"),
						new OperatorToken(Operators.POWER),
						new ParenthesisToken
						(
							new NumberToken(5),
							new OperatorToken(Operators.ADD),
							new NumberToken(5)
						)
					),
					new OperatorToken(Operators.SUBTRACT),
					new ParenthesisToken
					(
						new IdentifierToken("blueberry"),
						new OperatorToken(Operators.DIVIDE),
						new IdentifierToken("cucumber"),
						new OperatorToken(Operators.POWER),
						new IdentifierToken("potato")
					)
				)
			);

			Assert.AreEqual(expected, actual);
		}

		public static void UndefinedOperator()
		{
			try
			{
				Lexer.GetTokens("a ; b");
			}
			catch
			{
				Assert.Pass("Undefined operator caused an error which was expected");
			}

			Assert.Fail("Undefined operator did not cause an error which was not expected");
		}
	}
}