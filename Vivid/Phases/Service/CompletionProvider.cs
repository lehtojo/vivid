using System.Collections.Generic;
using System.Linq;

public struct CompletionProviderLoadInformation
{
	public string? Filter { get; set; }
	public Function? CursorFunction { get; set; }
}

public enum CompletionItemType
{
	Text = 0,
	Method = 1,
	Function = 2,
	Constructor = 3,
	Field = 4,
	Variable = 5,
	Class = 6,
	Interface = 7,
	Module = 8,
	Property = 9,
	Unit = 10,
	Value = 11,
	Enum = 12,
	Keyword = 13,
	Snippet = 14,
	Color = 15,
	Reference = 17,
	File = 16,
	Folder = 18,
	EnumMember = 19,
	Constant = 20,
	Struct = 21,
	Event = 22,
	Operator = 23,
	TypeParameter = 24,
	User = 25,
	Issue = 26,
}

public class CompletionItem
{
	public string Identifier { get; set; }
	public int Type { get; set; }

	public CompletionItem(string identifier, CompletionItemType type)
	{
		Identifier = identifier;
		Type = (int)type;
	}
}

public static class CompletionProvider
{
	/// <summary>
	/// Finds the token from the specified tokens, which should be marked as the cursor using the request information.
	/// This can return a filter string which can be used to filter the completion items.
	/// </summary>
	public static string? MarkCompletionToken(DocumentRequest request, List<Token> tokens)
	{
		// Find the tokens that surround the cursor
		var surroundings = CursorInformationProvider.GetCursorSurroundings(request.Document, tokens, request.Absolute);
		if (surroundings == null) return null;

		Lexer.Postprocess(tokens);

		var left = surroundings[0];
		var right = surroundings[1];
		var position = (Position?)null;

		if (left != null)
		{
			// If the left token is an identifier, it can be marked as the cursor
			if (left.Token.Is(TokenType.IDENTIFIER))
			{
				left.Token.Position.IsCursor = true;

				// Now the filter must be the value of the identifier token
				return left.Token.To<IdentifierToken>().Value;
			}

			if (left.Token.Is(Operators.DOT))
			{
				// If the user is accessing an object, insert an empty identifier after the dot and mark it as the cursor
				if (right == null || !right.Token.Is(TokenType.IDENTIFIER))
				{
					position = left.Token.Position.Clone().NextCharacter();
					position.IsCursor = true;

					left.Container.Insert(left.Index + 1, new IdentifierToken(string.Empty, position));
					return string.Empty;
				}

				// If the cursor is between a dot and an identifier, the identifier can be marked as the cursor
				if (right != null && right.Token.Is(TokenType.IDENTIFIER))
				{
					left.Token.Position.IsCursor = true;

					// Now the filter must be the value of the identifier token
					return left.Token.To<IdentifierToken>().Value;
				}
			}
		}

		position = null;

		var container = (List<Token>?)null;
		var index = -1;

		if (left != null)
		{
			container = left.Container;
			index = left.Index;
			position = Common.GetEndOfToken(left.Token) ?? left.Token.Position.Clone();
		}
		else if (right != null)
		{
			container = right.Container;
			index = right.Index;
			position = right.Token.Position.Clone();
		}
		else
		{
			var parenthesis = CursorInformationProvider.FindCursorParenthesis(tokens, request.Absolute);
			if (parenthesis == null) return string.Empty;

			container = parenthesis.Token.To<ContentToken>().Tokens;
			index = 0;
			position = parenthesis.Token.Position.Clone().NextCharacter();
		}

		if (container != null && position != null && index >= 0)
		{
			// Insert an empty identifier token which is marked as the cursor in order to fetch context information
			position.IsCursor = true;

			container.Insert(index, new Token(TokenType.END));
			container.Insert(index, new IdentifierToken(string.Empty, position));
			container.Insert(index, new Token(TokenType.END));
		}

		return string.Empty;
	}

	/// <summary>
	/// Updates the project and marks the cursor described by the request information.
	/// Returns relevant information about the cursor for code completion.
	/// </summary>
	public static CompletionProviderLoadInformation UpdateAndMark(Project project, DocumentRequest request)
	{
		var document = request.Document;
		var file = project.GetSourceFile(request.Uri);
		var parse = project.GetParse(file);

		var changed = project.Update(request.Uri, document);

		var filter = MarkCompletionToken(request, changed);
		if (filter == null) return new CompletionProviderLoadInformation();

		// Find the function that contains the cursor
		var cursor_function = CursorInformationProvider.FindCursorFunction(parse);

		return new CompletionProviderLoadInformation()
		{
			Filter = filter,
			CursorFunction = cursor_function
		};
	}

	/// <summary>
	/// Returns common completion items such as keywords
	/// </summary>
	private static List<CompletionItem> GetCommonCompletionItems()
	{
		return Keywords.Definitions.Select(i => new CompletionItem(i.Key, CompletionItemType.Keyword)).ToList();
	}

	/// <summary>
	/// Returns the appropriate completion item type for the specified variable
	/// </summary>
	private static CompletionItemType GetVariableCompletionItemType(Variable variable)
	{
		if (variable.IsConstant) return CompletionItemType.Constant;
		if (variable.IsMember) return CompletionItemType.Property;
		return CompletionItemType.Variable;
	}

	/// <summary>
	/// Creates completion items from the contents of the specified context
	/// </summary>
	private static List<CompletionItem> GetCompletionItems(Context context, bool local = false)
	{
		// Get all namespaces and types
		var types = context.Types.Where(i => !i.Value.IsTemplateTypeVariant)
			.Select(i => new CompletionItem(i.Key, i.Value.IsStatic ? CompletionItemType.Module : CompletionItemType.Class))
			.ToList();

		// Get all functions other than constructors and destructors
		var functions = context.Functions
			.Where(i => i.Key != Keywords.INIT.Identifier && i.Key != Keywords.DEINIT.Identifier)
			.Select(i => new CompletionItem(i.Key, CompletionItemType.Function))
			.ToList();

		// Add virtual functions as well if the specified context is a type
		if (context.IsType)
		{
			var virtuals = context.To<Type>().Virtuals.Values.SelectMany(i => i.Overloads);
			functions.AddRange(virtuals.Select(i => new CompletionItem(i.Name, CompletionItemType.Function)));
		}

		// Get all members and variables
		var variables = context.Variables
			.Where(i => !i.Value.IsHidden)
			.Select(i => new CompletionItem(i.Key, GetVariableCompletionItemType(i.Value)))
			.ToList();

		var items = variables.Concat(functions).Concat(types).ToList();

		if (context.IsType)
		{
			var type = (Type)context;

			items.Add(new CompletionItem(Keywords.INIT.Identifier, CompletionItemType.Constructor));
			items.Add(new CompletionItem(Keywords.DEINIT.Identifier, CompletionItemType.Constructor));

			foreach (var supertype in type.Supertypes)
			{
				items.AddRange(GetCompletionItems(supertype, local));
			}
		}

		if (!local && context.Parent != null)
		{
			items.AddRange(GetCompletionItems(context.Parent));
		}

		return items;
	}

	/// <summary>
	/// Sends the completion items available in the global scope to the client
	/// </summary>
	private static void SendGlobalScopeCompletionItems(Project project, IServiceResponse response, DocumentRequest request)
	{
		var items = GetCommonCompletionItems();
		items.AddRange(project.Documents.Values.Where(i => i.Context != null).SelectMany(i => GetCompletionItems(i.Context!, true)));

		var completions = items.DistinctBy(i => $"{i.Identifier}, {i.Type}").OrderBy(i => i.Identifier).ToArray();
		response.SendResponse(request.Uri, DocumentResponseStatus.OK, completions);
	}

	/// <summary>
	/// Sends completions to the requester based on the specified request
	/// </summary>
	public static void Provide(Project project, IServiceResponse response, DocumentRequest request)
	{
		var filename = ServiceUtility.ToPath(request.Uri);
		var information = UpdateAndMark(project, request);
		var filter = information.Filter;
		var cursor_function = information.CursorFunction;

		// If no function contains the cursor, send an error
		if (cursor_function == null)
		{
			SendGlobalScopeCompletionItems(project, response, request);
			return;
		}

		// Finally, build the document
		var parse = ProjectBuilder.Build(project.Documents, filename, false, cursor_function);

		// Find the cursor
		var cursor = CursorInformationProvider.FindCursorNode(cursor_function);

		// If the cursor is not found, send an error
		if (cursor == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		// Get all the common completion items
		var items = GetCommonCompletionItems();

		if (cursor.Parent != null && cursor.Parent.Is(NodeType.LINK))
		{
			// Get all completion items using the primary context, if possible
			var primary = cursor.Parent.Left.TryGetType();

			if (primary != null)
			{
				items = GetCompletionItems(primary, true);
			}
		}
		else
		{
			items.AddRange(GetCompletionItems(cursor.GetParentContext()));
		}

		var completions = items.DistinctBy(i => $"{i.Identifier}, {i.Type}").OrderBy(i => i.Identifier).ToArray();

		response.SendResponse(request.Uri, DocumentResponseStatus.OK, completions);
	}
}