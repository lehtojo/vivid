using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

public enum DocumentRequestType
{
	COMPLETIONS = 1,
	SIGNATURES = 2
}

public class DocumentRequest
{
	public DocumentRequestType Request { get; set; }
	public Uri Uri { get; set; }
	public string Document { get; set; }
	public DocumentPosition Position { get; set; }

	public DocumentRequest(string document, Uri uri, DocumentPosition position)
	{
		Document = document;
		Uri = uri;
		Position = position;
	}
}

public class DocumentToken
{
	public List<Token> Container { get; }
	public int Index { get; }
	public Token Token { get; }

	public DocumentToken(List<Token> container, int index, Token token)
	{
		Container = container;
		Index = index;
		Token = token;
	}
}

public enum CompletionItemType
{
	FUNCTION = 2,
	CONSTRUCTOR = 3,
	VARIABLE = 5,
	TYPE = 6,
	PROPERTY = 9,
	KEYWORD = 13
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

public class FunctionParameter
{
	public string Name { get; set; }
	public string Documentation { get; set; }

	public FunctionParameter(string name, string documentation)
	{
		Name = name;
		Documentation = documentation;
	}
}

public class FunctionSignature
{
	public string Identifier { get; set; }
	public string Documentation { get; set; }
	public FunctionParameter[] Parameters { get; set; }

	public FunctionSignature(string identifier, string documentation, FunctionParameter[] parameters)
	{
		Identifier = identifier;
		Documentation = documentation;
		Parameters = parameters;
	}
}

public class DocumentAnalysisResponse
{
	public int Status { get; set; }
	public Uri Uri { get; set; }
	public string Data { get; set; }

	public DocumentAnalysisResponse(int status, Uri uri, string data)
	{
		Status = status;
		Uri = uri;
		Data = data;
	}
}

public class ServicePhase : Phase
{
	public const string SERVICE_BIND_ADDRESS = "localhost";

	private static int? ToAbsolutePosition(string document, int line, int character)
	{
		var position = 0;

		for (var i = 0; i < line; i++)
		{
			var j = document.IndexOf('\n', position);

			if (j == -1)
			{
				return null;
			}

			position = j + 1;
		}

		position += character;

		if (position < 0 || position > document.Length)
		{
			return null;
		}

		return position;
	}

	private List<DocumentToken[]> GetAllCursorSurroundings(List<Token> tokens, int absolute)
	{
		var surroundings = new List<DocumentToken[]>();

		for (var i = 0; i < tokens.Count; i++)
		{
			var token = tokens[i];

			if (token.Is(TokenType.CONTENT))
			{
				surroundings.AddRange(GetAllCursorSurroundings(token.To<ContentToken>().Tokens, absolute));
			}

			if (token.Position.Absolute <= absolute)
			{
				continue;
			}

			surroundings.Add(new[] { i - 1 < 0 ? new DocumentToken(tokens, i, token) : new DocumentToken(tokens, i - 1, tokens[i - 1]), new DocumentToken(tokens, i, token) });
		}

		return surroundings;
	}

	private DocumentToken? FindCursorCallParenthesis(List<Token> tokens, int line, int character)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			var token = tokens[i];

			if (!token.Is(TokenType.CONTENT))
			{
				continue;
			}

			if (token.Is(ParenthesisType.PARENTHESIS) && token.Position.Line == line && token.Position.Character + 1 == character)
			{
				return new DocumentToken(tokens, i, token);
			}

			var result = FindCursorCallParenthesis(token.To<ContentToken>().Tokens, line, character);

			if (result != null)
			{
				return result;
			}
		}

		return null;
	}

	private DocumentToken[] GetCursorSurroundings(string document, List<Token> tokens, int line, int character)
	{
		var absolute = ToAbsolutePosition(document, line, character);

		if (absolute == null)
		{
			return Array.Empty<DocumentToken>();
		}

		var all = GetAllCursorSurroundings(tokens, (int)absolute);

		if (!all.Any())
		{
			return Array.Empty<DocumentToken>();
		}
		
		return all.OrderBy(i => Math.Abs(i[0].Token.Position.Absolute - (int)absolute) + Math.Abs(i[1].Token.Position.Absolute - (int)absolute)).First();
	}

	private static List<CompletionItem> GetCompletionItems(Context context)
	{
		var types = context.Types.Select(i => new CompletionItem(i.Key, CompletionItemType.TYPE)).ToList();
		var functions = context.Functions.Where(i => i.Key != Keywords.INIT.Identifier && i.Key != Keywords.DEINIT.Identifier).Select(i => new CompletionItem(i.Key, CompletionItemType.FUNCTION)).ToList();
		var variables = context.Variables.Where(i => !i.Value.IsHidden).Select(i => new CompletionItem(i.Key, CompletionItemType.VARIABLE)).ToList();

		var items = variables.Concat(functions).Concat(types).ToList();

		if (context.IsType)
		{
			var type = (Type)context;

			items.Add(new CompletionItem(Keywords.INIT.Identifier, CompletionItemType.CONSTRUCTOR));
			items.Add(new CompletionItem(Keywords.DEINIT.Identifier, CompletionItemType.CONSTRUCTOR));

			foreach (var supertype in type.Supertypes)
			{
				items.AddRange(GetCompletionItems(supertype));
			}
		}

		if (context.Parent != null && !context.IsType)
		{
			items.AddRange(GetCompletionItems(context.Parent));
		}

		return items;
	}

	private static List<CompletionItem> GetCommonCompletionItems()
	{
		return Keywords.Values.Select(i => new CompletionItem(i.Key, CompletionItemType.KEYWORD)).ToList();
	}

	private static FunctionSignature[] GetFunctionSignatures(IEnumerable<Function> overloads)
	{
		return overloads.Select(i => new FunctionSignature(i.ToString(), "Halooo!", i.Parameters.Select(i => new FunctionParameter(i.Name, "Bababui")).ToArray())).ToArray();
	}

	public static int GetDamerauLevenshteinDistance(string a, string b)
	{
		var n = a.Length;
		var m = b.Length;

		if (n == 0)
		{
			return m;
		}

		if (m == 0)
		{
			return n;
		}

		var p = new int[n + 1]; // 'Previous' cost array, horizontally
		var d = new int[n + 1]; // Cost array, horizontally

		// Indexes into strings s and t
		int i; // Iterates through s
		int j; // Iterates through t

		for (i = 0; i <= n; i++)
		{
			p[i] = i;
		}

		for (j = 1; j <= m; j++)
		{
			var c = b[j - 1];
			d[0] = j;

			for (i = 1; i <= n; i++)
			{
				int cost = a[i - 1] == c ? 0 : 1;
				d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
			}

			var temporary = p;
			p = d;
			d = temporary;
		}

		return p[n];
	}

	private static void SendEmptyResponse(UdpClient socket, Uri uri, IPEndPoint receiver)
	{
		var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(1, uri, string.Empty), typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		socket.Send(bytes, bytes.Length, receiver);
	}

	private static void SendResponse(UdpClient socket, IPEndPoint receiver, DocumentAnalysisResponse response)
	{
		var payload = JsonSerializer.Serialize(response, typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		socket.Send(bytes, bytes.Length, receiver);
	}

	private void Respond(UdpClient socket, IPEndPoint receiver, DocumentRequestType request, Uri uri, string document, int line, int character)
	{
		document = document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

		var filter = string.Empty;
		var tokens = Lexer.GetTokens(document, false);

		if (request == DocumentRequestType.COMPLETIONS)
		{
			var surroundings = GetCursorSurroundings(document, tokens, line, character);

			Lexer.Join(tokens);

			if (surroundings.Any())
			{
				var left = surroundings[0];
				var right = surroundings[1];

				// If the left token is an identifier, it can be marked as the cursor
				if (left.Token.Is(TokenType.IDENTIFIER))
				{
					left.Token.Position.IsCursor = true;
					filter = left.Token.To<IdentifierToken>().Value;
				}
				else if (left.Token.Is(Operators.DOT))
				{
					// If the user is accessing an object, insert an empty identifier after the dot and mark it as the cursor
					if (!right.Token.Is(TokenType.IDENTIFIER))
					{
						var position = left.Token.Position.Clone().NextCharacter();
						position.IsCursor = true;

						right.Container.Insert(right.Index, new IdentifierToken(string.Empty, position));
					}
					else
					{
						// If the cursor is between a dot and an identifier, the identifier can be marked as the cursor
						left.Token.Position.IsCursor = true;
						filter = left.Token.To<IdentifierToken>().Value;
					}
				}
				else
				{
					// Insert an empty identifier token which is marked as the cursor in order to fetch context information
					var position = left.Token.Position.Clone().NextCharacter().NextCharacter();
					position.IsCursor = true;

					right.Container.Insert(right.Index, new Token(TokenType.END));
					right.Container.Insert(right.Index, new IdentifierToken(string.Empty, position));
					right.Container.Insert(right.Index, new Token(TokenType.END));
				}
			}
		}
		else
		{
			var parenthesis = FindCursorCallParenthesis(tokens, line, character);

			if (parenthesis == null)
			{
				SendEmptyResponse(socket, uri, receiver);
				return;
			}

			Lexer.Join(tokens);

			// Require the previous token to be an identifier token
			if (parenthesis.Index - 1 < 0 || !parenthesis.Container[parenthesis.Index - 1].Is(TokenType.IDENTIFIER))
			{
				SendEmptyResponse(socket, uri, receiver);
				return;
			}

			var identifier = parenthesis.Container[parenthesis.Index - 1];

			// Register the function name as the cursor position
			identifier.Position.IsCursor = true;
			filter = identifier.To<IdentifierToken>().Value;
		}

		var file = new SourceFile("Source.v", document, 0);
		Lexer.RegisterFile(tokens, file);

		// Parse the document
		var context = Parser.Initialize(0);
		var root = Parser.CreateRootNode(context);

		Parser.Parse(root, context, tokens);

		// Find all the types under the parsed document and parse them completely
		var types = root.FindAll(i => i.Is(NodeType.TYPE)).Cast<TypeNode>().ToArray();
		types.ForEach(i => i.Parse());
		
		root.FindAll(i => i.Is(NodeType.NAMESPACE)).Cast<NamespaceNode>().ForEach(i => i.Parse(context));

		// Ensure exported and virtual functions are implemented
		ParserPhase.ImplementFunctions(context, true);

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);

		var report = ResolverPhase.GetReport(context);
		var evaluated = false;

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			// Try to resolve any problems in the node tree
			ParserPhase.ApplyExtensionFunctions(context, root);
			ParserPhase.ImplementFunctions(context);

			Resolver.ResolveContext(context);
			report = ResolverPhase.GetReport(context);

			// Try again only if the errors have changed
			if (report != previous) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			var cursor = implementation.Node!.Find(i => i.Position != null && i.Position.IsCursor);

			if (cursor == null)
			{
				continue;
			}

			var environment = cursor.GetParentContext();

			if (request == DocumentRequestType.SIGNATURES)
			{
				FunctionList? overloads;

				if (cursor.Parent != null && cursor.Parent.Is(NodeType.LINK))
				{
					var primary = cursor.Parent.Left.TryGetType();

					if (primary != null)
					{
						// Try to get function overloads with the name equal to the filter
						overloads = primary.GetFunction(filter);

						if (overloads != null)
						{
							var signatures = GetFunctionSignatures(overloads.Overloads);

							SendResponse(socket, receiver, new DocumentAnalysisResponse(0, uri, JsonSerializer.Serialize<FunctionSignature[]>(signatures)));
							return;
						}

						// If no function could be found using the filter, return the global function candidates
					}
				}

				// Try to get function overloads with the name equal to the filter using the environment context
				overloads = environment.GetFunction(filter);

				if (overloads != null)
				{
					var signatures = GetFunctionSignatures(overloads.Overloads);

					SendResponse(socket, receiver, new DocumentAnalysisResponse(0, uri, JsonSerializer.Serialize<FunctionSignature[]>(signatures)));
					return;
				}

				SendEmptyResponse(socket, uri, receiver);
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
					items = GetCompletionItems(primary);
				}
			}
			else
			{
				items.AddRange(GetCompletionItems(environment));
			}

			if (string.IsNullOrEmpty(filter))
			{
				items = items.OrderBy(i => i.Identifier).ToList();
			}
			else
			{
				items = items.OrderBy(i => GetDamerauLevenshteinDistance(i.Identifier, filter)).ToList();
			}

			SendResponse(socket, receiver, new DocumentAnalysisResponse(0, uri, JsonSerializer.Serialize<CompletionItem[]>(items.ToArray())));
			return;
		}

		// Send an empty list of completions
		SendEmptyResponse(socket, uri, receiver);
	}

	private static Position? GetEndOfToken(Token token)
	{
		return token.Type switch
		{
			TokenType.CONTENT => token.To<ContentToken>().End ?? token.Position.Translate(1),
			TokenType.FUNCTION => token.To<FunctionToken>().Identifier.End,
			TokenType.IDENTIFIER => token.To<IdentifierToken>().End,
			TokenType.KEYWORD => token.To<KeywordToken>().End,
			TokenType.NUMBER => token.To<NumberToken>().End ?? token.Position.Translate(1),
			TokenType.OPERATOR => token.To<OperatorToken>().End,
			TokenType.STRING => token.To<StringToken>().End,
			_ => null
		};
	}

	private void Map(List<Token> tokens, List<DocumentDiagnostic> diagnostics)
	{
		foreach (var token in tokens)
		{
			for (var i = 0; i < diagnostics.Count; i++)
			{
				var diagnostic = diagnostics[i];

				if (!diagnostic.Range.Start.Equals(token.Position))
				{
					continue;
				}

				diagnostics.RemoveAt(i);

				var end = GetEndOfToken(token);

				if (end == null)
				{
					break;
				}

				diagnostic.Range.End = new DocumentPosition(end.Line, end.Character);
				break;
			}

			if (token.Is(TokenType.CONTENT))
			{
				Map(token.To<ContentToken>().Tokens, diagnostics);
			}
		}
	}

	private void Diagnose(UdpClient socket, IPEndPoint receiver, Uri uri, List<Token> tokens)
	{
		var source = new List<Token>(tokens);
		var file = new SourceFile("Source.v", string.Empty, 0);

		Lexer.RegisterFile(tokens, file);

		// Parse the document
		var context = Parser.Initialize(0);
		var root = Parser.CreateRootNode(context);

		Parser.Parse(root, context, tokens);

		// Find all the types under the parsed document and parse them completely
		var types = root.FindAll(i => i.Is(NodeType.TYPE)).Cast<TypeNode>().ToArray();
		types.ForEach(i => i.Parse());

		root.FindAll(i => i.Is(NodeType.NAMESPACE)).Cast<NamespaceNode>().ForEach(i => i.Parse(context));

		// Applies all the extension functions
		ParserPhase.ApplyExtensionFunctions(context, root);

		// Ensure exported and virtual functions are implemented
		ParserPhase.ImplementFunctions(context, true);

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);

		var report = ResolverPhase.GetReport(context);
		var evaluated = false;

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			// Try to resolve any problems in the node tree
			ParserPhase.ApplyExtensionFunctions(context, root);
			ParserPhase.ImplementFunctions(context);

			Resolver.ResolveContext(context);
			report = ResolverPhase.GetReport(context);

			// Try again only if the errors have changed
			if (report != previous) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		var diagnostics = ResolverPhase.GetDiagnostics(context, root).ToArray();
		Map(source, new List<DocumentDiagnostic>(diagnostics));

		SendResponse(socket, receiver, new DocumentAnalysisResponse(0, uri, JsonSerializer.Serialize<DocumentDiagnostic[]>(diagnostics)));
	}

	private void StartDiagnostics(UdpClient socket)
	{
		while (true)
		{
			var receiver = (IPEndPoint?)null;
			var bytes = socket.Receive(ref receiver);

			var data = Encoding.UTF8.GetString(bytes);
			var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

			if (request == null)
			{
				var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(1, new Uri(string.Empty), "Could not understand the request"), typeof(DocumentAnalysisResponse));
				bytes = Encoding.UTF8.GetBytes(payload);
				
				socket.Send(bytes, bytes.Length, receiver);
				continue;
			}

			var tokens = (List<Token>?)null;

			try
			{
				var document = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');
				tokens = Lexer.GetTokens(document);

				Diagnose(socket, receiver, request.Uri, new List<Token>(tokens));
			}
			catch (LexerException e)
			{
				var diagnostics = new[] { new DocumentDiagnostic(e.Position, e.Description, DocumentDiagnosticSeverity.ERROR) };
				SendResponse(socket, receiver, new DocumentAnalysisResponse(0, request.Uri, JsonSerializer.Serialize<DocumentDiagnostic[]>(diagnostics)));
			}
			catch (SourceException e)
			{
				var diagnostics = new[] { new DocumentDiagnostic(e.Position, e.Message, DocumentDiagnosticSeverity.ERROR) };
				
				if (tokens != null)
				{
					Map(tokens, new List<DocumentDiagnostic>(diagnostics));
				}

				SendResponse(socket, receiver, new DocumentAnalysisResponse(0, request.Uri, JsonSerializer.Serialize<DocumentDiagnostic[]>(diagnostics)));
			}
			catch
			{
				SendEmptyResponse(socket, request.Uri, receiver);
			}
		}
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Get(ConfigurationPhase.SERVICE_FLAG, false))
		{
			return Status.OK;
		}

		using var specified_request_socket = new UdpClient(7777);
		using var diagnostics_socket = new UdpClient(8888);

		var end_point = (specified_request_socket.Client.LocalEndPoint as IPEndPoint) ?? throw new ApplicationException("Could not create local service socket");
		Console.WriteLine(end_point.Port);

		end_point = (diagnostics_socket.Client.LocalEndPoint as IPEndPoint) ?? throw new ApplicationException("Could not create local service socket");
		//Console.WriteLine(end_point.Port);

		Task.Run(() => StartDiagnostics(diagnostics_socket));

		while (true)
		{
			var receiver = (IPEndPoint?)null;
			var bytes = specified_request_socket.Receive(ref receiver);

			var data = Encoding.UTF8.GetString(bytes);
			var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

			if (request == null)
			{
				var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(1, new Uri(string.Empty), "Could not understand the request"), typeof(DocumentAnalysisResponse));
				bytes = Encoding.UTF8.GetBytes(payload);

				specified_request_socket.Send(bytes, bytes.Length, receiver);
				continue;
			}

			try
			{
				Respond(specified_request_socket, receiver, request.Request, request.Uri, request.Document, request.Position.Line, request.Position.Character);
			}
			catch
			{
				SendEmptyResponse(specified_request_socket, request.Uri, receiver);
			}
		}
	}
}