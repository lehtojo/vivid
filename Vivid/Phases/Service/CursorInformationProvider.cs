using System.Collections.Generic;

public static class CursorInformationProvider
{
	/// <summary>
	/// Returns whether the specified absolute position is between the start and end positions.
	/// </summary>
	public static bool IsBetween(Position start, int absolute, Position end)
	{
		return start.Absolute <= absolute && absolute <= end.Absolute;
	}

	/// <summary>
	/// Tries to find the parenthesis where the specified to cursor position is inside.
	/// If the function fails, it returns null.
	/// </summary>
	public static DocumentToken? FindCursorParenthesis(List<Token> tokens, int absolute)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			var token = tokens[i];

			if (!token.Is(TokenType.PARENTHESIS)) continue;

			var result = FindCursorParenthesis(token.To<ParenthesisToken>().Tokens, absolute);
			if (result != null) return result;

			var end = token.To<ParenthesisToken>().End;

			if (token.Is(ParenthesisType.PARENTHESIS) && end != null && IsBetween(token.Position, absolute, end))
			{
				return new DocumentToken(tokens, i, token);
			}
		}

		return null;
	}

	/// <summary>
	/// Tries to find a token that is registered as cursor from the specified tokens
	/// </summary>
	public static DocumentToken? FindUnmarkedCursorToken(List<Token> tokens, int absolute, out DocumentToken? next)
	{
		next = null;

		for (var i = 0; i < tokens.Count; i++)
		{
			var token = tokens[i];

			if (token.Type == TokenType.PARENTHESIS)
			{
				var cursor = FindUnmarkedCursorToken(token.To<ParenthesisToken>().Tokens, absolute, out next);
				if (cursor != null) return cursor;
			}

			var start = token.Position;
			var end = Common.GetEndOfToken(token);

			if (end != null && IsBetween(token.Position, absolute, end))
			{
				if (i + 1 < tokens.Count) { next = new DocumentToken(tokens, i + 1, tokens[i + 1]); }

				return new DocumentToken(tokens, i, token);
			}
		}

		return null;
	}

	/// <summary>
	/// Tries to find a token that is registered as cursor from the specified tokens
	/// </summary>
	public static Token? FindUnmarkedCursorToken(List<Token> tokens, int absolute)
	{
		return FindUnmarkedCursorToken(tokens, absolute, out _)?.Token;
	}

	/// <summary>
	/// Tries to find a token that is registered as cursor from the specified tokens
	/// </summary>
	public static Token? FindMarkedCursorToken(List<Token> tokens)
	{
		foreach (var token in tokens)
		{
			if (token.Position.IsCursor) return token;
			if (token.Type != TokenType.PARENTHESIS) continue;

			var cursor = FindMarkedCursorToken(token.To<ParenthesisToken>().Tokens);
			if (cursor != null) return cursor;
		}

		return null;
	}

	/// <summary>
	/// Unmarks all cursor tokens in the specified tokens
	/// </summary>
	public static Token? UnmarkCursors(List<Token> tokens)
	{
		foreach (var token in tokens)
		{
			if (token.Position.IsCursor) { token.Position.IsCursor = false; }
			if (token.Type != TokenType.PARENTHESIS) continue;

			UnmarkCursors(token.To<ParenthesisToken>().Tokens);
		}

		return null;
	}

	/// <summary>
	/// Finds the function, whose blueprint contains the cursor
	/// </summary>
	public static Function? FindCursorFunction(DocumentParse parse)
	{
		// Find the function that contains the cursor
		foreach (var iterator in parse.Blueprints)
		{
			if (FindMarkedCursorToken(iterator.Value) != null) return iterator.Key;
		}

		return null;
	}

	/// <summary>
	/// Finds the cursor node from the specified function
	/// </summary>
	public static Node? FindCursorNode(Function function)
	{
		foreach (var implementation in function.Implementations)
		{
			var cursor = implementation.Node!.Find(i => i.Position != null && i.Position.IsCursor);

			if (cursor != null)
			{
				// If the cursor is a link node and the right operand is a cursor as well, switch to it.
				// Note: We do this, because expressions such as "member" expand to "this.member" and we want the right operand.
				if (cursor.Instance == NodeType.LINK && cursor.Right.Position?.IsCursor == true) { cursor = cursor.Right; }

				return cursor;
			}
		}

		return null;
	}
}