using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public enum DocumentRequestType
{
	COMPLETIONS = 1,
	SIGNATURES = 2,
	DIAGNOSE = 3,
	OPEN = 4,
	DEFINITION = 5,
	INFORMATION = 6
}

public class DocumentRequest
{
	public DocumentRequestType Type { get; set; }
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
	MODULE = 8,
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

public static class DocumentResponseStatus
{
	public const int OK = 0;
	public const int INVALID_REQUEST = 1;
	public const int ERROR = 2;
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

	public DocumentAnalysisResponse(int status, string filename, string data)
	{
		Status = status;
		Uri = ServicePhase.ToUri(filename);
		Data = data;
	}
}

public class DocumentParse
{
	public List<Token> Tokens { get; }
	public Context? Context { get; set; }
	public Node? Root { get; set; }

	public DocumentParse(List<Token> tokens)
	{
		Tokens = tokens;
	}
}

public class ServicePhase : Phase
{
	public const string FILE_SCHEME = "file";
	public const string UNTITLED_FILE_SCHEME = "untitled";
	public const string SERVICE_BIND_ADDRESS = "localhost";

	private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	

	#region Utility and document analysis tools

	/// <summary>
	/// Converts the specified uri to string
	/// </summary>
	public static string ToPath(Uri uri)
	{
		if (string.IsNullOrEmpty(uri.ToString())) return string.Empty;

		if (uri.Scheme == FILE_SCHEME)
		{
			if (IsLinux) return uri.LocalPath;
			
			// If the uri starts with a separator, remove it
			var path = uri.LocalPath.FirstOrDefault() == '/' ? uri.LocalPath[1..] : uri.LocalPath;
			var i = path.IndexOf(':');
			
			// If the path does not have the drive name, return it
			if (i == -1) return path;

			// Ensure the drive name is in upper case
			return path.Substring(0, i).ToUpperInvariant() + path.Substring(i);
		}

		return uri.ToString();
	}

	/// <summary>
	/// Converts the specified path to uri
	/// </summary>
	public static Uri ToUri(string path)
	{
		if (!path.StartsWith(UNTITLED_FILE_SCHEME + ':'))
		{
			return new Uri(FILE_SCHEME + ":///" + path.Replace(":", "%3A"));
		}

		return new Uri(path);
	}

	/// <summary>
	/// Converts the specified line number and character number into an absolute offset from the start of the specified text
	/// </summary>
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

	/// <summary>
	/// Returns a list of token pairs which surround the specified cursor position.
	/// This functions returns a list of tokens pairs, because there can be tokens surrounding the cursor on multiple layers.
	/// </summary>
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
			break;
		}

		return surroundings;
	}

	/// <summary>
	/// Tries to return the two tokens which surround the specified cursor position.
	/// If the function fails, it returns an empty array.
	/// </summary>
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

	/// <summary>
	/// Tries to find the parenthesis where the specified to cursor position is inside.
	/// If the function fails, it returns null.
	/// </summary>
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

	/// <summary>
	/// Returns the position which represents the end of the specified token
	/// </summary>
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

	/// <summary>
	/// Figures out the ranges of the specified diagnostics by examining the specified tokens
	/// </summary>
	private void Map(List<Token> tokens, List<DocumentDiagnostic> diagnostics)
	{
		foreach (var token in tokens)
		{
			for (var i = 0; i < diagnostics.Count; i++)
			{
				var diagnostic = diagnostics[i];

				if (!diagnostic.Range.Start.Equals(token.Position)) continue;

				diagnostics.RemoveAt(i);

				var end = GetEndOfToken(token);
				if (end == null) break;

				diagnostic.Range.End = new DocumentPosition(end.Line, end.Character);
				break;
			}

			if (token.Is(TokenType.CONTENT))
			{
				Map(token.To<ContentToken>().Tokens, diagnostics);
			}
		}
	}

	/// <summary>
	/// Creates completion items from the contents of the specified context
	/// </summary>
	private static List<CompletionItem> GetCompletionItems(Context context)
	{
		var types = context.Types.Select(i => new CompletionItem(i.Key, i.Value.IsStatic ? CompletionItemType.MODULE : CompletionItemType.TYPE)).ToList();
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

	/// <summary>
	/// Returns common completion items such as keywords
	/// </summary>
	private static List<CompletionItem> GetCommonCompletionItems()
	{
		return Keywords.Values.Select(i => new CompletionItem(i.Key, CompletionItemType.KEYWORD)).ToList();
	}

	/// <summary>
	/// Creates function signatures from the specified function overloads.
	/// </summary>
	private static FunctionSignature[] GetFunctionSignatures(IEnumerable<Function> overloads)
	{
		return overloads.Select(i => new FunctionSignature(i.ToString(), string.Empty, i.Parameters.Select(i => new FunctionParameter(i.Name, string.Empty)).ToArray())).ToArray();
	}

	/// <summary>
	/// Compares the two specified strings and returns an integer which describes how similar they are.
	/// This can be used for ordering completion items for example.
	/// </summary>
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

	/// <summary>
	/// Sends an empty response to the specified receiver by serializing the reponse to JSON-format
	/// </summary>
	private static void SendStatusCode(UdpClient socket, IPEndPoint receiver, Uri uri, int code, string message = "")
	{
		var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(code, uri, message), typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		socket.Send(bytes, bytes.Length, receiver);
	}

	/// <summary>
	/// Sends the specified reponse to the specified receiver by serializing the reponse to JSON-format
	/// </summary>
	private static void SendResponse(UdpClient socket, IPEndPoint receiver, DocumentAnalysisResponse response)
	{
		var payload = JsonSerializer.Serialize(response, typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		socket.Send(bytes, bytes.Length, receiver);
	}

	#endregion

	/// <summary>
	/// Edits the specified tokens so that they can provide information for completions later.
	/// This function has also the authority to cancel the completions if the preparation can not be done for some reason.
	/// If the preparation can not be done, this function returns null.
	/// </summary>
	private string? PrepareTokensForCompletions(UdpClient socket, IPEndPoint receiver, DocumentRequest request, List<Token> tokens)
	{
		var type = request.Type;
		var uri = request.Uri;
		var document = request.Document;
		var line = request.Position.Line;
		var character = request.Position.Character;

		if (type == DocumentRequestType.COMPLETIONS)
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

					// Now the filter must be the value of the identifier token
					return left.Token.To<IdentifierToken>().Value;
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

						// Now the filter must be the value of the identifier token
						return left.Token.To<IdentifierToken>().Value;
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
		else if (type == DocumentRequestType.DEFINITION || type == DocumentRequestType.INFORMATION)
		{
			var surroundings = GetCursorSurroundings(document, tokens, line, character);

			if (!surroundings.Any())
			{
				SendStatusCode(socket, receiver, uri, DocumentResponseStatus.ERROR);
				return null;
			}

			// The first token is the selected token, so register it as the cursor
			surroundings.First().Token.Position.IsCursor = true;
		}
		else
		{
			// The request is to send the current function signature, so try to find the parenthesis where the cursor is inside
			var parenthesis = FindCursorCallParenthesis(tokens, line, character);

			// If the cursor is not inside parenthesis, nothing can be done
			if (parenthesis == null)
			{
				SendStatusCode(socket, receiver, uri, DocumentResponseStatus.ERROR);
				return null;
			}

			Lexer.Join(tokens);

			// Require the previous token to be an identifier token
			if (parenthesis.Index - 1 < 0 || !parenthesis.Container[parenthesis.Index - 1].Is(TokenType.IDENTIFIER))
			{
				SendStatusCode(socket, receiver, uri, DocumentResponseStatus.ERROR);
				return null;
			}

			var identifier = parenthesis.Container[parenthesis.Index - 1];

			// Register the function name as the cursor position
			identifier.Position.IsCursor = true;

			// Now the filter must be the value of the identifier token
			return identifier.To<IdentifierToken>().Value;
		}

		// Return an empty filter
		return string.Empty;
	}

	/// <summary>
	/// Sends completions to the requester based on the specified request
	/// </summary>
	private void SendCompletions(Dictionary<SourceFile, DocumentParse> files, UdpClient socket, IPEndPoint receiver, DocumentRequest request)
	{
		var filename = ToPath(request.Uri);
		var document = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');
		
		// Tokenize the document
		var tokens = Lexer.GetTokens(document, false);
		var source = new List<Token>(tokens);

		// Join the tokens now, because the 'source' list now contains the most accurate version of the document
		Lexer.Join(tokens);

		// Prepare the tokens for completions
		var filter = PrepareTokensForCompletions(socket, receiver, request, tokens);

		// Ensure the preparation succeeded
		if (filter == null) return;

		// Find the source file which has the same filename as the specified filename
		var file = files.Keys.FirstOrDefault(i => i.Fullname == filename);
		var index = file != null ? file.Index : files.Count;
		
		if (file == null)
		{
			file = new SourceFile(filename, document, index);
		}

		files[file] = new DocumentParse(tokens);

		ParseAll(files);

		// Finally, build the document
		var parse = Build(files, filename);

		// Now find the cursor and return completions based on its position
		foreach (var implementation in Common.GetAllFunctionImplementations(parse.Context))
		{
			var cursor = implementation.Node!.Find(i => i.Position != null && i.Position.IsCursor);

			if (cursor == null)
			{
				continue;
			}

			var environment = cursor.GetParentContext();

			if (request.Type == DocumentRequestType.SIGNATURES)
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

							SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, JsonSerializer.Serialize(signatures)));
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
					SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, JsonSerializer.Serialize(signatures)));
					return;
				}

				SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
				return;
			}
			else if (request.Type == DocumentRequestType.DEFINITION)
			{
				var position = (Position?)null;
				var length = 0;

				// The cursor must be a function or a variable, so that its definition can be found
				if (cursor.Is(NodeType.FUNCTION))
				{
					position = cursor.To<FunctionNode>().Function.Metadata.Start;
					length = cursor.To<FunctionNode>().Function.Name.Length;
				}
				else if (cursor.Is(NodeType.VARIABLE))
				{
					position = cursor.To<VariableNode>().Variable.Position;
					length = cursor.To<VariableNode>().Variable.Name.Length;
				}
				else if (cursor.Is(NodeType.TYPE))
				{
					position = cursor.To<TypeNode>().Type.Position;
					length = cursor.To<TypeNode>().Type.Identifier.Length;
				}
				else if (cursor.Is(NodeType.CONSTRUCTION))
				{
					position = cursor.To<ConstructionNode>().Constructor.Function.Metadata.Start;
					length = cursor.To<ConstructionNode>().Constructor.Function.Name.Length;
				}

				if (position == null || position.File == null)
				{
					SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
					return;
				}

				var start = new DocumentPosition(position.Line, position.Character);
				var end = new DocumentPosition(position.Line, position.Character + length);

				var response = new DocumentAnalysisResponse(DocumentResponseStatus.OK, position.File.Fullname, JsonSerializer.Serialize(new DocumentRange(start, end)));
				SendResponse(socket, receiver, response);
				return;
			}
			else if (request.Type == DocumentRequestType.INFORMATION)
			{
				var information = string.Empty;

				// The cursor must be a function or a variable, so that its definition can be found
				if (cursor.Is(NodeType.FUNCTION))
				{
					information = $"Function {cursor.To<FunctionNode>().Function.ToString()}";
				}
				else if (cursor.Is(NodeType.VARIABLE))
				{
					information = $"Variable {cursor.To<VariableNode>().Variable.ToString()}";
				}
				else if (cursor.Is(NodeType.TYPE))
				{
					information = $"Type {cursor.To<TypeNode>().Type.ToString()}";
				}
				else if (cursor.Is(NodeType.CONSTRUCTION))
				{
					information = $"Function {cursor.To<ConstructionNode>().Constructor.Function.Metadata.ToString()}";
				}

				if (string.IsNullOrEmpty(information))
				{
					SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
					return;
				}

				var response = new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, information);
				SendResponse(socket, receiver, response);
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

			SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, JsonSerializer.Serialize(items.ToArray())));
			return;
		}

		// Send an empty list of completions
		SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
	}

	
	/// <summary>
	/// Removes all the information from previous sessions and analyzes the files in the specified folder and in the standard library
	/// </summary>
	private static void Open(Dictionary<SourceFile, DocumentParse> files, string folder)
	{
		// Remove all the files which were prepared previously
		files.Clear();

		// Prepare the standard library
		LoadAndParseAll(files, Environment.CurrentDirectory + "/libv/");

		// Prepare the openened folder if it is not empty
		if (!string.IsNullOrEmpty(folder)) LoadAndParseAll(files, folder);
	}

	/// <summary>
	/// Loads the specified file and creates tokens from its content
	/// </summary>
	private static void Load(Dictionary<SourceFile, DocumentParse> files, string filename)
    {
		var content = File.ReadAllText(filename).Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

		// Find the source file which has the same filename as the specified filename
		var file = files.Keys.FirstOrDefault(i => i.Fullname == filename);
		var index = file != null ? file.Index : files.Count;

		if (file == null)
		{
			file = new SourceFile(filename, content, index);
		}

		files[file] = new DocumentParse(Lexer.GetTokens(content));
	}

	/// <summary>
	/// Builds all the source files in the specified folder
	/// </summary>
	private static void LoadAndParseAll(Dictionary<SourceFile, DocumentParse> files, string folder)
	{
		var filenames = Directory.GetFiles(folder, $"*{ConfigurationPhase.EXTENSION}", SearchOption.AllDirectories);

		for (var i = 0; i < filenames.Length; i++)
		{
			Load(files, filenames[i]);
		}

		ParseAll(files);
	}

	/// <summary>
	/// Prepare the specified file for building
	/// </summary>
	private static void ParseAll(Dictionary<SourceFile, DocumentParse> files)
	{
		foreach (var file in files.Keys.ToArray())
		{
			Parse(file, files[file]);
		}
	}

	/// <summary>
	/// Prepare the specified file for building
	/// </summary>
	private static void Parse(SourceFile file, DocumentParse parse)
	{
		var tokens = new List<Token>(parse.Tokens);

		// Join the tokens now, because the 'source' list now contains the most accurate version of the document
		Lexer.Join(tokens);
		Lexer.RegisterFile(tokens, file);

		// Parse the document
		var context = Parser.Initialize(file.Index);
		var root = new ScopeNode(context, null, null);

		Parser.Parse(root, context, tokens);

		parse.Root = root;
		parse.Context = context;
	}

	/// <summary>
	/// Builds the specified file
	/// </summary>
	private static Parse Build(Dictionary<SourceFile, DocumentParse> files, string filename)
	{
		// Find the source file which has the same filename as the specified filename
		var filter = files.Keys.First(i => i.Fullname == filename);

		var root = (Node?)null;
		var context = (Context?)null;

		foreach (var file in files.Values)
		{
			root = file.Root;
			context = file.Context;

			if (root == null || context == null) continue;

			// Find all the types under the parsed document and parse them completely
			var types = root.FindAll(i => i.Is(NodeType.TYPE)).Cast<TypeNode>().ToArray();
			types.ForEach(i => i.Parse());

			root.FindAll(i => i.Is(NodeType.NAMESPACE)).Cast<NamespaceNode>().ForEach(i => i.Parse(context));

			// Applies all the extension functions
			ParserPhase.ApplyExtensionFunctions(context, root);
		}

		// Merge all parsed files
		context = new Context(ParserPhase.ROOT_CONTEXT_IDENTITY.ToLowerInvariant());
		root = Parser.CreateRootNode(context);

		// Now merge all the parsed source files
		foreach (var file in files.Values)
		{
			if (file.Root == null || file.Context == null) continue;

			context.Merge(file.Context);
			root.Merge(file.Root.Clone());
		}

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
			ParserPhase.ImplementFunctions(context, filter, true);

			Resolver.ResolveContext(context);
			report = ResolverPhase.GetReport(context);

			// Try again only if the errors have changed
			if (report != previous) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		return new Parse(context, root, files[filter].Tokens);
	}

	/// <summary>
	/// Diagnoses the specified file using its content
	/// </summary>
	private void Diagnose(Dictionary<SourceFile, DocumentParse> files, UdpClient socket, IPEndPoint receiver, Uri uri)
	{
		var parse = Build(files, ToPath(uri));

		var diagnostics = ResolverPhase.GetDiagnostics(parse.Context, parse.Node).ToArray();
		Map(parse.Tokens, new List<DocumentDiagnostic>(diagnostics));

		SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, uri, JsonSerializer.Serialize(diagnostics)));
	}

	/// <summary>
	/// Start a service which diagnoses projects for the specified client
	/// </summary>
	private void StartDiagnostics(UdpClient socket)
	{
		var files = new Dictionary<SourceFile, DocumentParse>();
		
		while (true)
		{
			var receiver = (IPEndPoint?)null;
			var bytes = socket.Receive(ref receiver);

			var data = Encoding.UTF8.GetString(bytes);
			var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

			// If the request can not be deserialized, send back a reponse which states the request is invalid
			if (request == null)
			{
				SendStatusCode(socket, receiver, new Uri(string.Empty), DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
				continue;
			}

			var tokens = (List<Token>?)null;
			var start = DateTime.Now;

			try
			{
				if (request.Type == DocumentRequestType.OPEN)
				{
					var folder = ToPath(request.Uri);

					Console.WriteLine($"Opening project folder '{folder}'");

					Open(files, folder);
					SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.OK);

					Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
					continue;
				}
				else
				{
					var filename = ToPath(request.Uri);
					var content = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

					// Find the source file which has the same filename as the specified filename
					var file = files.Keys.FirstOrDefault(i => i.Fullname == filename);
					var index = file != null ? file.Index : files.Count;
					
					if (file == null)
					{
						file = new SourceFile(filename, content, index);
					}

					var parse = new DocumentParse(Lexer.GetTokens(content));
					files[file] = parse;

					// Prepare the specified file using its content
					ParseAll(files);

					// Find the tokens of the prepared file
					tokens = parse.Tokens;

					// Finally diagnose the file
					Diagnose(files, socket, receiver, request.Uri);
				}
			}
			catch (LexerException e)
			{
				var diagnostics = new[] { new DocumentDiagnostic(e.Position, e.Description, DocumentDiagnosticSeverity.ERROR) };
				SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, JsonSerializer.Serialize(diagnostics)));
			}
			catch (SourceException e)
			{
				var diagnostics = new[] { new DocumentDiagnostic(e.Position, e.Message, DocumentDiagnosticSeverity.ERROR) };
				
				if (tokens != null)
				{
					Map(tokens, new List<DocumentDiagnostic>(diagnostics));
				}

				SendResponse(socket, receiver, new DocumentAnalysisResponse(DocumentResponseStatus.OK, request.Uri, JsonSerializer.Serialize(diagnostics)));
			}
			catch
			{
				SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
			}

			Console.WriteLine($"Analysis took {(DateTime.Now - start).TotalMilliseconds} ms");
		}
	}


	/// <summary>
	/// Start a service which send completions to the specified client
	/// </summary>
	private void StartCompleting(UdpClient socket)
	{
		var files = new Dictionary<SourceFile, DocumentParse>();
		
		while (true)
		{
			var receiver = (IPEndPoint?)null;
			var bytes = socket.Receive(ref receiver);

			var data = Encoding.UTF8.GetString(bytes);
			var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

			// If the request can not be deserialized, send back a reponse which states the request is invalid
			if (request == null)
			{
				SendStatusCode(socket, receiver, new Uri(string.Empty), DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
				continue;
			}

			try
			{
				if (request.Type == DocumentRequestType.OPEN)
				{
					var start = DateTime.Now;
					var folder = ToPath(request.Uri);

					Console.WriteLine($"Opening project folder '{folder}'");

					Open(files, folder);
					SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.OK);

					Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
					continue;
				}

				SendCompletions(files, socket, receiver, request);
			}
			catch
			{
				SendStatusCode(socket, receiver, request.Uri, DocumentResponseStatus.ERROR);
			}
		}
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Get(ConfigurationPhase.SERVICE_FLAG, false))
		{
			return Status.OK;
		}

		using var completions_socket = new UdpClient(1111);
		using var diagnostics_socket = new UdpClient(2222);

		var end_point = (completions_socket.Client.LocalEndPoint as IPEndPoint) ?? throw new ApplicationException("Could not create local service socket");
		Console.WriteLine(end_point.Port);

		end_point = (diagnostics_socket.Client.LocalEndPoint as IPEndPoint) ?? throw new ApplicationException("Could not create local service socket");
		//Console.WriteLine(end_point.Port);

		// Diagnose projects on another thread
		Task.Run(() => StartDiagnostics(diagnostics_socket));

		// Send code completions and signatures on the current thread 
		StartCompleting(completions_socket);

		return Status.OK;
	}
}