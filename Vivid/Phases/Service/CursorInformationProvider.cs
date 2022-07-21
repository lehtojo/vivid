using System.Collections.Generic;
using System.Linq;
using System;

public struct CursorInformationRange
{
	public int Start { get; set; }
	public int End { get; set; }

	public CursorInformationRange(int start, int end)
	{
		Start = start;
		End = end;
	}
}

public struct DocumentChangeLineInformation
{
	public string[] PreviousDocumentLines { get; set; }
	public string[] CurrentDocumentLines { get; set; }
	public int PreviousDocumentFirstLine { get; set; }
	public int PreviousDocumentLastLine { get; set; }
	public int CurrentDocumentFirstLine { get; set; }
	public int CurrentDocumentLastLine { get; set; }

	public DocumentChangeLineInformation(
		string[] previous_document_lines,
		string[] current_document_lines,
		int previous_document_first_line,
		int previous_document_last_line,
		int current_document_first_line,
		int current_document_last_line
	) {
		PreviousDocumentLines = previous_document_lines;
		CurrentDocumentLines = current_document_lines;
		PreviousDocumentFirstLine = previous_document_first_line;
		PreviousDocumentLastLine = previous_document_last_line;
		CurrentDocumentFirstLine = current_document_first_line;
		CurrentDocumentLastLine = current_document_last_line;
	}
}

public static class CursorInformationProvider
{
	public static DocumentToken?[]? GetCursorSurroundings(Token token, int absolute)
	{
		if (token.Type == TokenType.CONTENT)
		{
			return GetCursorSurroundings(token.To<ContentToken>().Tokens, token.Position.Absolute, absolute, token.To<ContentToken>().End!.Absolute);
		}

		return null;
	}

	public static DocumentToken?[]? GetCursorSurroundings(List<Token> tokens, int container_start, int absolute, int container_end)
	{
		if (tokens.Count == 0) return null;

		if (absolute >= container_start && absolute <= tokens[0].Position.Absolute)
		{
			return new[] { null, new DocumentToken(tokens, 0, tokens.First()) };
		}

		var result = GetCursorSurroundings(tokens[0], absolute);
		if (result != null) return result;

		for (var i = 1; i < tokens.Count; i++)
		{
			var left = tokens[i - 1];
			var right = tokens[i];

			if (left.Position.Absolute <= absolute && right.Position.Absolute >= absolute)
			{
				return new[] { new DocumentToken(tokens, i - 1, left), new DocumentToken(tokens, i, right) };
			}

			result = GetCursorSurroundings(tokens[i], absolute);
			if (result != null) return result;
		}

		var end = Common.GetEndOfToken(tokens.Last());

		if (end != null && absolute >= end.Absolute && absolute <= container_end)
		{
			return new[] { new DocumentToken(tokens, tokens.Count - 1, tokens.Last()), null };
		}

		result = GetCursorSurroundings(tokens.Last(), absolute);
		return result;
	}

	/// <summary>
	/// Tries to return the two tokens which surround the specified cursor position.
	/// If the function fails, it returns an empty array.
	/// </summary>
	public static DocumentToken?[]? GetCursorSurroundings(string document, List<Token> tokens, int line, int character)
	{
		var absolute = ServiceUtility.ToAbsolutePosition(document, line, character);
		if (absolute == null) return Array.Empty<DocumentToken>();

		return GetCursorSurroundings(tokens, 0, (int)absolute!, document.Length);
	}

	/// <summary>
	/// Tries to return the two tokens which surround the specified cursor position.
	/// If the function fails, it returns an empty array.
	/// </summary>
	public static DocumentToken?[]? GetCursorSurroundings(string document, List<Token> tokens, int absolute)
	{
		return GetCursorSurroundings(tokens, 0, absolute, document.Length);
	}

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

			if (!token.Is(TokenType.CONTENT)) continue;

			var result = FindCursorParenthesis(token.To<ContentToken>().Tokens, absolute);
			if (result != null) return result;

			var end = token.To<ContentToken>().End;

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
	public static Token? FindUnmarkedCursorToken(List<Token> tokens, int absolute)
	{
		foreach (var token in tokens)
		{
			if (token.Type == TokenType.CONTENT)
			{
				var cursor = FindUnmarkedCursorToken(token.To<ContentToken>().Tokens, absolute);
				if (cursor != null) return cursor;
			}

			var start = token.Position;
			var end = Common.GetEndOfToken(token);
			if (end != null && IsBetween(token.Position, absolute, end)) return token;
		}

		return null;
	}

	/// <summary>
	/// Tries to find a token that is registered as cursor from the specified tokens
	/// </summary>
	public static Token? FindMarkedCursorToken(List<Token> tokens)
	{
		foreach (var token in tokens)
		{
			if (token.Position.IsCursor) return token;
			if (token.Type != TokenType.CONTENT) continue;

			var cursor = FindMarkedCursorToken(token.To<ContentToken>().Tokens);
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
			if (token.Type != TokenType.CONTENT) continue;

			UnmarkCursors(token.To<ContentToken>().Tokens);
		}

		return null;
	}

	/// <summary>
	/// Finds the function, whose blueprint contains the cursor
	/// </summary>
	public static Function? FindCursorFunction(Context context)
	{
		// Find the function that contains the cursor
		return Common.GetAllVisibleFunctions(context).FirstOrDefault(i => FindMarkedCursorToken(i.Blueprint) != null);
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
			if (cursor != null) return cursor;
		}

		return null;
	}

	/// <summary>
	/// Returns the first and last lines that have changed between the two specified documents.
	/// </summary>
	public static DocumentChangeLineInformation? GetChangedLineRange(string previous_document, string current_document)
	{
		var previous_lines = previous_document.Split('\n');
		var current_lines = current_document.Split('\n');

		// Find the first line that has changed, starting from the top
		var i = 0;
		var n = Math.Min(previous_lines.Length, current_lines.Length);

		for (; i < n && previous_lines[i] == current_lines[i]; i++) {}

		// If we reached the end of the document, it means that both documents are identical
		if (i == n) return null;

		// Find the last line that has changed, starting from the bottom
		var j = 1;

		for (; previous_lines.Length - j >= i && current_lines.Length - j >= i; j++)
		{
			if (previous_lines[previous_lines.Length - j] != current_lines[current_lines.Length - j]) break;
		}

		return new DocumentChangeLineInformation(
			previous_lines,
			current_lines,
			i, previous_lines.Length - j + 1,
			i, current_lines.Length - j + 1
		);
	}

	/// <summary>
	/// Finds the function, which contains the specified line changes.
	/// If such function cannot be found, this function returns null.
	/// </summary>
	public static Function? FindChangedFunction(DocumentChangeLineInformation changes, DocumentParse parse)
	{
		// Find the function that contains the changed lines
		foreach (var function in parse.Functions)
		{
			var start = function.Start;
			var end = function.End;

			if (start == null || end == null) continue;

			if (changes.PreviousDocumentFirstLine > start.Line && changes.PreviousDocumentLastLine < end.Line) return function;
		}

		return null;
	}

	/// <summary>
	/// Returns the changed tokens of the specified function.
	/// This function assumes that the lines outside the function have not changed.
	/// </summary>
	public static List<Token>? GetChangedFunctionTokens(SourceFile file, DocumentChangeLineInformation changes, Function function)
	{
		// Determine the first line of the function in the current document
		var function_start_line = function.Start!.Line;

		// Compute the distance between the last line of the function and the end of the document in terms of lines (previous document)
		var end_line_distance = changes.PreviousDocumentLines.Length - function.End!.Line;

		// Compute the last line of the function in the current document by using the distance from the end of the document
		var function_end_line = changes.CurrentDocumentLines.Length - end_line_distance;

		// Load the lines between the start and end of the function in the current document
		var function_lines = changes.CurrentDocumentLines.Skip(function_start_line).Take((function_end_line + 1) - function_start_line).ToList();

		// Combine the loaded lines into a single string
		var function_content = string.Join('\n', function_lines);

		// Tokenize the function content
		var line_ending_count = function_start_line;
		var absolute = changes.CurrentDocumentLines.Take(function_start_line).Sum(i => i.Length) + line_ending_count;

		var tokens = Lexer.GetTokens(function_content, new Position(function_start_line, 0, 0, absolute));
		Lexer.Postprocess(tokens);
		Lexer.RegisterFile(tokens, file);

		// Find all parentheses in the tokens
		var parentheses = tokens.FindAll(i => i.Is(ParenthesisType.PARENTHESIS) || i.Is(ParenthesisType.CURLY_BRACKETS));

		// Verify the parentheses has function parameters and the function body
		if (parentheses.Count != 2 || !parentheses[0].Is(ParenthesisType.PARENTHESIS) || !parentheses[1].Is(ParenthesisType.CURLY_BRACKETS)) return null;

		// Verify the last token is the function body
		if (!ReferenceEquals(tokens.Last(), parentheses[1])) return null;

		// Return the function body tokens
		return tokens.Last().To<ContentToken>().Tokens;
	}
}