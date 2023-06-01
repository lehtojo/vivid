using System.Collections.Generic;

public class ParserState
{
	public List<Token> All { get; set; } = new List<Token>();
	public List<Token> Tokens { get; set; } = new List<Token>();
	public Pattern? Pattern { get; set; }
	public int Start { get; set; }
	public int End { get; set; }

	public ParserState() {}
	public ParserState(List<Token> all)
	{
		All = all;
	}

	public ParserState Save()
	{
		var result = new ParserState();
		result.All = new List<Token>(All);
		result.Tokens = new List<Token>(Tokens);
		result.Start = Start;
		result.End = End;
		return result;
	}

	public void Restore(ParserState from)
	{
		All.Clear();
		All.AddRange(from.All);
		Tokens.Clear();
		Tokens.AddRange(from.Tokens);
		Start = from.Start;
		End = from.End;
	}

	public void Consume()
	{
		Tokens.Add(All[End]);
		End++;
	}

	/// <summary>
	/// Consumes the next token, if its type is contained in the specified types. This function returns true, if the next token is consumed, otherwise false.
	/// </summary>
	public bool Consume(long types)
	{
		if (End >= All.Count) return false;
		var next = All[End];
		if (!Flag.Has(types, next.Type)) return false;
		Tokens.Add(next);
		End++;
		return true;
	}

	/// <summary>
	/// Consumes the next token, if its type is contained in the specified types. This function returns true, if the next token is consumed, otherwise false.
	/// </summary>
	public bool Consume(out Token? next, long types)
	{
		next = null;
		if (End >= All.Count) return false;
		next = All[End];
		if (!Flag.Has(types, next.Type)) return false;
		Tokens.Add(next);
		End++;
		return true;
	}

	/// <summary>
	/// Consumes the next token if it exists and it represents the specified operator
	/// </summary>
	public bool ConsumeOperator(Operator operation)
	{
		if (End >= All.Count) return false;
		var next = All[End];
		if (!next.Is(operation)) return false;
		Tokens.Add(next);
		End++;
		return true;
	}

	/// <summary>
	/// Consumes the next token if it exists and it represents the specified parenthesis
	/// </summary>
	public bool ConsumeParenthesis(ParenthesisType type)
	{
		if (End >= All.Count) return false;
		var next = All[End];
		if (!next.Is(type)) return false;
		Tokens.Add(next);
		End++;
		return true;
	}

	/// <summary>
	/// Consumes the next token, if its type is contained in the specified types. This function returns true, if the next token is consumed, otherwise an empty token is consumed and false is returned.
	/// </summary>
	public bool ConsumeOptional(long types)
	{
		if (End >= All.Count)
		{
			Tokens.Add(new Token(TokenType.NONE));
			return false;
		}

		var next = All[End];

		if (!Flag.Has(types, next.Type))
		{
			Tokens.Add(new Token(TokenType.NONE));
			return false;
		}

		Tokens.Add(next);
		End++;
		return true;
	}

	public Token? Peek()
	{
		if (All.Count > End) return All[End];
		return null;
	}
}

public static class Parser
{
	public static List<List<Pattern>> Patterns { get; set; } = new();

	public const Format Format = global::Format.UINT64;
	public const Format Signed = global::Format.INT64;
	public const int Bytes = 8;
	public const int Bits = 64;

	public const int MIN_PRIORITY = 0;
	public const int MAX_FUNCTION_BODY_PRIORITY = 19;
	public const int MAX_PRIORITY = 23;
	public const int PRIORITY_ALL = -1;

	public const string STANDARD_RANGE_TYPE = "Range";
	public const string STANDARD_LIST_TYPE = "List";
	public const string STANDARD_LIST_ADDER = "add";
	public const string STANDARD_STRING_TYPE = "String";
	public const string STANDARD_ALLOCATOR_FUNCTION = "allocate";

	public static List<Pattern> GetPatterns(int priority)
	{
		return Patterns[priority];
	}

	public static void Add(Pattern pattern)
	{
		if (pattern.Priority != -1)
		{
			Patterns[pattern.Priority].Add(pattern);
			return;
		}

		for (var i = 0; i < Patterns.Count; i++)
		{
			Patterns[i].Add(pattern);
		}
	}

	static Parser()
	{
		Patterns = new List<List<Pattern>>();
		for (var i = 0; i < MAX_PRIORITY + 1; i++) { Patterns.Add(new List<Pattern>());  }

		Add(new CommandPattern());
		Add(new AssignPattern());
		Add(new FunctionPattern());
		Add(new OperatorPattern());
		Add(new TypePattern());
		Add(new ReturnPattern());
		Add(new IfPattern());
		Add(new InheritancePattern());
		Add(new LinkPattern());
		Add(new ListConstructionPattern());
		Add(new ListPattern());
		Add(new SingletonPattern());
		Add(new LoopPattern());
		Add(new ForeverLoopPattern());
		Add(new CastPattern());
		Add(new AccessorPattern());
		Add(new ImportPattern());
		Add(new ConstructorPattern());
		Add(new NotPattern());
		Add(new VariableDeclarationPattern());
		Add(new ElsePattern());
		Add(new UnarySignPattern());
		Add(new PostIncrementPattern());
		Add(new PreIncrementPattern());
		Add(new ExpressionVariablePattern());
		Add(new ModifierSectionPattern());
		Add(new SectionModificationPattern());
		Add(new NamespacePattern());
		Add(new IterationLoopPattern());
		Add(new TemplateFunctionPattern());
		Add(new TemplateFunctionCallPattern());
		Add(new TemplateTypePattern());
		Add(new VirtualFunctionPattern());
		Add(new SpecificModificationPattern());
		Add(new TypeInspectionPattern());
		Add(new CompilesPattern());
		Add(new IsPattern());
		Add(new OverrideFunctionPattern());
		Add(new PackConstructionPattern());
		Add(new LambdaPattern());
		Add(new RangePattern());
		Add(new HasPattern());
		Add(new ExtensionFunctionPattern());
		Add(new WhenPattern());
		Add(new UsingPattern());
		Add(new GlobalScopeAccessPattern());
	}

	/// <summary>
	/// Returns whether the specified pattern can be built at the specified position
	/// </summary>
	public static bool Fits(Pattern pattern, List<Token> tokens, int start, ParserState state)
	{
		var path = pattern.Path;
		var consumed = 0;

		// First, attempt fitting the pattern without collecting tokens, because most patterns fail
		for (var path_index = 0; path_index < path.Count; path_index++)
		{
			var types = path[path_index];
			var token_index = start + consumed;

			// Ensure there is a token available
			if (token_index >= tokens.Count)
			{
				// If the token type is optional on the path, we can add a none token even though there are no tokens available
				if (Flag.Has(types, TokenType.OPTIONAL)) continue;

				return false;
			}

			var type = tokens[token_index].Type;

			// Add the token if the allowed types contains its type
			if (Flag.Has(types, type))
			{
				consumed++;
				continue;
			}

			// If the allowed types contain optional, we can just ignore the current token
			if (Flag.Has(types, TokenType.OPTIONAL))
			{
				// NOTE: Do not skip the current token, since it was not consumed
				continue;
			}

			return false;
		}

		// Since we ended up here, it means the pattern was successfully consumed.
		// Now collect the tokens that were consumed by the pattern.
		var consumed_tokens = new List<Token>();

		consumed = 0;

		for (var path_index = 0; path_index < path.Count; path_index++)
		{
			var types = path[path_index];
			var token_index = start + consumed;

			if (token_index >= tokens.Count)
			{
				consumed_tokens.Add(new Token(TokenType.NONE));
				continue;
			}

			var token = tokens[token_index];

			if (!Flag.Has(types, token.Type)) 
			{
				consumed_tokens.Add(new Token(TokenType.NONE));
				continue;
			}

			consumed_tokens.Add(token);
			consumed++;
		}

		state.Tokens = consumed_tokens;
		state.Pattern = pattern;
		state.Start = start;
		state.End = start + consumed;

		return true;
	}

	/// <summary>
	/// Tries to find the next pattern from the specified tokens, which has the specified priority
	/// </summary>
	public static bool Next(Context context, List<Token> tokens, int priority, int start, ParserState state)
	{
		var all = Patterns[priority];

		for (; start < tokens.Count; start++)
		{
			// NOTE: Patterns all sorted so that the longest pattern is first, so if it passes, it takes priority over all the other patterns
			for (var i = 0; i < all.Count; i++)
			{
				var pattern = all[i];
				if (Fits(pattern, tokens, start, state) && pattern.Passes(context, state, state.Tokens, priority)) return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Tries to find the next pattern from the specified tokens, which has the specified priority
	/// </summary>
	public static bool NextConsumable(Context context, List<Token> tokens, int priority, int start, ParserState state, long disabled)
	{
		var all = Patterns[priority];

		for (; start < tokens.Count; start++)
		{
			// NOTE: Patterns all sorted so that the longest pattern is first, so if it passes, it takes priority over all the other patterns
			for (var i = 0; i < all.Count; i++)
			{
				var pattern = all[i];

				// Ensure the pattern is consumable and is not disabled
				if (!pattern.IsConsumable || (disabled & pattern.Id) != 0) continue;

				if (Fits(pattern, tokens, start, state) && pattern.Passes(context, state, state.Tokens, priority)) return true;
			}
		}

		return false;
	}

	public static void Parse(Node root, Context context, List<Token> tokens)
	{
		Parse(root, context, tokens, Parser.MIN_PRIORITY, Parser.MAX_PRIORITY);
	}

	/// <summary>
	/// Forms function tokens from the specified tokens
	/// </summary>
	public static void CreateFunctionTokens(List<Token> tokens)
	{
		if (tokens.Count < 2) return;

		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			var name = tokens[i];
			if (name.Type != TokenType.IDENTIFIER) continue;
			var parameters = tokens[i + 1];
			if (!parameters.Is(ParenthesisType.PARENTHESIS)) continue;

			tokens[i] = new FunctionToken(name.To<IdentifierToken>(), parameters.To<ParenthesisToken>(), name.Position);
			tokens.RemoveAt(i + 1);

			i--;
		}
	}

	public static bool IsLineRelated(List<Token> tokens, int i, int j, int k)
	{
		var first_line_end_index = j - 1;
		var second_line_start_index = j + 1;

		var first_line_end = (Token?)null;
		if (first_line_end_index >= 0) { first_line_end = tokens[first_line_end_index]; }

		var second_line_start = (Token?)null;
		if (second_line_start_index < tokens.Count) { second_line_start = tokens[second_line_start_index]; }

		if (first_line_end != null && first_line_end.Is(TokenType.OPERATOR, TokenType.KEYWORD)) return true;
		if (second_line_start != null && (second_line_start.Is(TokenType.OPERATOR, TokenType.KEYWORD) || second_line_start.Is(ParenthesisType.CURLY_BRACKETS))) return true;

		for (var l = i; l < j; l++)
		{
			if (tokens[l].Type != TokenType.KEYWORD) continue;

			var keyword = tokens[l].To<KeywordToken>().Keyword;
			if (keyword.Type == KeywordType.FLOW) return true;
		}

		return false;
	}

	public static List<Token>? IsConsumingNamespace(List<Token> tokens, int i)
	{
		// Save the position of the namespace keyword
		var start = i;

		// Move to the next token
		i++;

		// Find the start of the body by skipping the name
		for (; i < tokens.Count && tokens[i].Is(TokenType.IDENTIFIER | TokenType.OPERATOR); i++) {}

		// If we reached the end, stop and return none
		if (i >= tokens.Count) return (List<Token>?)null;

		// Optionally consume a line ending
		if (tokens[i].Type == TokenType.END)
		{
			i++;

			// If we reached the end, stop and return none
			if (i >= tokens.Count) return (List<Token>?)null;
		}

		// If this namespace is a consuming section, then the next token is not curly brackets
		if (tokens[i].Is(ParenthesisType.CURLY_BRACKETS)) return (List<Token>?)null;

		var section = tokens.GetRange(start, tokens.Count - start);
		tokens.RemoveRange(start, tokens.Count - start);

		return section;
	}

	/// <summary>
	/// Returns the first section from the specified tokens that consumes all the lines below it.
	/// If such section can not be found, none is returned.
	/// </summary>
	public static List<Token>? FindConsumingSection(List<Token> tokens)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			var section = (List<Token>?)null;
			var next = tokens[i];

			if (next.Is(Keywords.NAMESPACE))
			{
				section = IsConsumingNamespace(tokens, i);
			}

			if (section != null) return section;
		}

		return null;
	}

	public static List<List<Token>> Split(List<Token> tokens)
	{
		var consuming_section = FindConsumingSection(tokens);

		var sections = new List<List<Token>>();
		var i = 0;

		while (i < tokens.Count)
		{
			// Look for the first line ending after i
			var j = i + 1;
			for (; j < tokens.Count && tokens[j].Type != TokenType.END; j++) { }

			// If we reached the end here, we can just add the active section and stop
			if (j == tokens.Count)
			{
				var section = tokens.GetRange(i, j - i);
				sections.Add(section);
				break;
			}

			// Start consuming lines after j
			var k = j + 1;

			for (; k < tokens.Count; k++)
			{
				if (tokens[k].Type != TokenType.END) continue;

				// If the line is related to the active section, we can just consume it and continue
				if (IsLineRelated(tokens, i, j, k))
				{
					j = k;
					continue;
				}

				// Since the line is not related to the active section, the active section ends at j
				var section = tokens.GetRange(i, j - i);
				sections.Add(section);

				i = j; // Start over in a situation where the line i+1..j is the first
				break;
			}

			if (k != tokens.Count) continue;

			if (IsLineRelated(tokens, i, j, k))
			{
				var section = tokens.GetRange(i, k - i);
				sections.Add(section);
			}
			else
			{
				var section = tokens.GetRange(i, j - i);
				sections.Add(section);

				section = tokens.GetRange(j, k - j);
				sections.Add(section);
			}

			break;
		}

		// Add the consuming section to the end of all sections, if such was found
		if (consuming_section != null) sections.Add(consuming_section);

		return sections;
	}

	public static void ParseSection(Node root, Context context, List<Token> tokens, int min, int max)
	{
		CreateFunctionTokens(tokens);

		var state = new ParserState();
		state.All = tokens;

		for (var priority = max; priority >= min; priority--)
		{
			while (true)
			{
				if (!Next(context, tokens, priority, 0, state)) break;

				var node = state.Pattern!.Build(context, state, state.Tokens);

				// Remove the consumed tokens
				var length = state.End - state.Start;
				while (length-- > 0) { tokens.RemoveAt(state.Start); }

				// Remove the consumed tokens from the state
				state.Tokens.Clear();

				// Replace the consumed tokens with the a dynamic token if a node was returned
				if (node != null) tokens.Insert(state.Start, new DynamicToken(node));
			}
		}

		foreach (var token in tokens)
		{
			if (token.Type == TokenType.DYNAMIC)
			{
				root.Add(token.To<DynamicToken>().Node);
				continue;
			}

			if (token.Type != TokenType.END)
			{
				throw Errors.Get(token.Position, "Can not understand");
			}
		}
	}

	/// <summary>
	/// Creates the root context, which might contain some default types
	/// </summary>
	public static Context CreateRootContext(int index)
	{
		var context = new Context(index.ToString());
		Primitives.Inject(context);
		return context;
	}

	/// <summary>
	/// Creates the root context, which might contain some default types
	/// </summary>
	public static Context CreateRootContext(string identity)
	{
		var context = new Context(identity);
		Primitives.Inject(context);
		return context;
	}

	public static Node CreateRootNode(Context context)
	{
		var root = new ScopeNode(context, null, null, false);

		var positive_infinity = new Variable(context, Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL), VariableCategory.GLOBAL, Settings.POSITIVE_INFINITY_CONSTANT, Modifier.PRIVATE | Modifier.CONSTANT);
		var negative_infinity = new Variable(context, Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL), VariableCategory.GLOBAL, Settings.NEGATIVE_INFINITY_CONSTANT, Modifier.PRIVATE | Modifier.CONSTANT);

		var true_constant = new Variable(context, Primitives.CreateBool(), VariableCategory.GLOBAL, "true", Modifier.PRIVATE | Modifier.CONSTANT);
		var false_constant = new Variable(context, Primitives.CreateBool(), VariableCategory.GLOBAL, "false", Modifier.PRIVATE | Modifier.CONSTANT);

		var position = (Position?)null;

		root.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
			new VariableNode(positive_infinity, position),
			new NumberNode(Format.DECIMAL, double.MaxValue, position)
		));

		root.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
			new VariableNode(negative_infinity, position),
			new NumberNode(Format.DECIMAL, double.MinValue, position)
		));

		root.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
			new VariableNode(true_constant, position),
			new CastNode(new NumberNode(Settings.Format, 1L, position), new TypeNode(Primitives.CreateBool(), position), position)
		));

		root.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
			new VariableNode(false_constant, position),
			new CastNode(new NumberNode(Settings.Format, 0L, position), new TypeNode(Primitives.CreateBool(), position), position)
		));

		return root;
	}

	public static void Parse(Node root, Context context, List<Token> tokens, int min, int max)
	{
		var sections = Split(tokens);

		foreach (var section in sections)
		{
			ParseSection(root, context, section, min, max);
		}
	}

	public static Node Parse(Context context, List<Token> tokens, int min, int max)
	{
		var result = new Node();
		Parse(result, context, tokens, min, max);
		return result;
	}
}