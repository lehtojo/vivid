using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Sublist<T> : IList<T>
{
	private List<T> Parent { get; set; }
	private int Start { get; set; }
	private int End { get; set; }

	public Sublist(List<T> parent, int start, int end)
	{
		Parent = parent;
		Start = start;
		End = end;
	}

	public T this[int index]
	{
		get => Parent[Start + index];
		set => Parent[Start + index] = value;
	}

	public int Count => End - Start;

	public bool IsReadOnly => false;

	public void Add(T item)
	{
		Parent.Insert(End++, item);
	}

	public void Clear()
	{
		Parent.RemoveRange(Start, End - Start);
		End = Start;
	}

	public bool Contains(T item)
	{
		return Parent.GetRange(Start, End).Contains(item);
	}

	public void CopyTo(T[] array, int index)
	{
		Parent.CopyTo(Start, array, index, Count);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Parent.GetRange(Start, End - Start).GetEnumerator();
	}

	public int IndexOf(T item)
	{
		return Parent.GetRange(Start, End - Start).IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		Parent.Insert(Start + index, item);
		End++;
	}

	public bool Remove(T item)
	{
		var i = IndexOf(item);

		if (i == -1)
		{
			return false;
		}

		Parent.RemoveAt(i);
		End--;

		return true;
	}

	public void RemoveAt(int index)
	{
		Parent.RemoveAt(Start + index);
		End--;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Parent.GetEnumerator();
	}
}

public static class Parser
{
	public static FunctionImplementation? AllocationFunction { get; set; }
	public static FunctionImplementation? DeallocationFunction { get; set; }
	public static FunctionImplementation? InheritanceFunction { get; set; }
	public static Function? LinkFunction { get; set; }
	public static Function? UnlinkFunction { get; set; }

	public static Size Size { get; set; } = Size.QWORD;
	public static Format Format => Size.ToFormat();
	public static int Bytes => Size.Bytes;

	public const int MAX_PRIORITY = 23;
	public const int MAX_FUNCTION_BODY_PRIORITY = 19;
	public const int MIN_PRIORITY = 0;
	public const int PRIORITY_ALL = -1;
	public const int PRIORITY_NEVER = -2;

	private const int FUNCTION_LENGTH = 2;

	private class Instance
	{
		public Pattern Pattern { get; private set; }
		public IList<Token> Tokens { get; set; }
		public List<Token> Formatted { get; private set; }
		public PatternState State { get; private set; }

		public Instance(Pattern pattern, IList<Token> tokens, List<Token> formatted, PatternState state)
		{
			Pattern = pattern;
			Tokens = tokens;
			Formatted = formatted;
			State = state;
		}

		public void Remove()
		{
			var start = Pattern.GetStart();
			var end = Pattern.GetEnd();
			var count = (end == -1 ? Tokens.Count : end) - start;

			for (var i = 0; i < count; i++)
			{
				Tokens.RemoveAt(start);
			}
		}

		private int GetAbsolutePosition(int p)
		{
			var x = 0;

			for (var i = 0; i < p; i++)
			{
				if (Formatted[i].Type != TokenType.NONE)
				{
					x++;
				}
			}

			return x;
		}

		public Range GetRange()
		{
			var start = Pattern.GetStart();
			var end = Pattern.GetEnd();

			return new Range(
				start == -1 ? Tokens.Count : GetAbsolutePosition(start),
				end == -1 ? Tokens.Count : GetAbsolutePosition(end)
			);
		}

		public void Replace(DynamicToken token)
		{
			var range = GetRange();
			var length = range.End.Value - range.Start.Value;

			for (var i = 0; i < length; i++)
			{
				Tokens.RemoveAt(range.Start.Value);
			}

			Tokens.Insert(range.Start.Value, token);
		}
	}

	private static List<Token> Mold(List<int> indices, IList<Token> candidate)
	{
		var formatted = new List<Token>(candidate);

		foreach (var index in indices)
		{
			formatted.Insert(index, new Token(TokenType.NONE));
		}

		return formatted;
	}

	/// <summary>
	/// Tries to find the next pattern from the tokens by comparing the priority
	/// </summary>
	/// <returns>Success: Next important pattern in tokens, Failure null</returns>
	private static Instance? Next(Context context, List<Token> tokens, int min, int priority, List<System.Type> allowlist, int start = 0)
	{
		for (; start < tokens.Count; start++)
		{
			// Start from the root
			var patterns = Patterns.Root;

			if (patterns == null)
			{
				throw new ApplicationException("The root of the patterns was not initialized");
			}

			Instance? instance = null;

			// Try finding the next pattern
			for (var end = start; end < tokens.Count; end++)
			{
				// Navigate forward on the tree
				patterns = patterns.Navigate(tokens[end].Type);

				// When tree becomes null the end of the tree is reached
				if (patterns == null)
				{
					break;
				}

				if (patterns.HasOptions)
				{
					var candidate = tokens.Sublist(start, end + 1);

					foreach (var option in patterns.Options)
					{
						var pattern = option.Pattern;

						// Skip this candidate if it is denylisted
						if (allowlist.Any() && !allowlist.Exists(i => i == pattern.GetType()))
						{
							continue;
						}

						var formatted = Mold(option.Missing, candidate);
						var pattern_priority = pattern.GetPriority(formatted);

						if (pattern_priority == priority || pattern_priority == Parser.PRIORITY_ALL)
						{
							var state = new PatternState(tokens, formatted, start, end + 1, min, priority);

							if (!pattern.Passes(context, state, formatted))
							{
								continue;
							}

							candidate = tokens.Sublist(state.Start, state.End);
							instance = new Instance(pattern, candidate, state.Formatted, state);
							break;
						}
					}
				}
			}

			if (instance != null)
			{
				return instance;
			}
		}

		return null;
	}

	/// <summary>
	/// Parses tokens with minimum and maximum priority range
	/// </summary>
	/// <returns>Parsed node tree</returns>
	public static Node Parse(Context context, List<Token> tokens, int min = MIN_PRIORITY, int max = MAX_PRIORITY)
	{
		var node = new Node();
		Parse(node, context, tokens, min, max);

		return node;
	}

	private class Consumption
	{
		public DynamicToken Token { get; set; }
		public int Count { get; set; }

		public Consumption(DynamicToken token, int count)
		{
			Token = token;
			Count = count;
		}
	}

	public static bool TryConsume(PatternState state, out List<Token> consumed, params int[] path)
	{
		consumed = new List<Token>();

		var i = state.End;

		// Ensure there are enough tokens
		if (state.Tokens.Count - i < path.Length)
		{
			return false;
		}

		foreach (var type in path)
		{
			var token = state.Tokens[i];

			if (!Flag.Has(type, token.Type))
			{
				if (Flag.Has(type, TokenType.OPTIONAL))
				{
					consumed.Add(new Token(TokenType.NONE));
					continue;
				}

				consumed.Clear();
				return false;
			}
			else
			{
				i++;
			}
		}

		var count = i - state.End;

		consumed = state.Tokens.GetRange(state.End, count);

		state.Formatted.AddRange(consumed);
		state.End += count;

		return true;
	}

	public static bool TryConsume(PatternState state, out Token? consumed, int mask)
	{
		consumed = null;

		var i = state.End;

		// Ensure there are enough tokens
		if (state.Tokens.Count - i < 1)
		{
			return false;
		}

		if (!Flag.Has(mask, state.Tokens[i].Type))
		{
			if (!Flag.Has(mask, TokenType.OPTIONAL))
			{
				return false;
			}

			consumed = new Token(TokenType.NONE);
		}
		else
		{
			consumed = state.Tokens[i];
			state.End++;
		}

		state.Formatted.Add(consumed);

		return true;
	}

	public static bool TryConsume(PatternState state)
	{
		var i = state.End;

		// Ensure there are enough tokens
		if (state.Tokens.Count - i < 1)
		{
			return false;
		}

		state.Formatted.Add(state.Tokens[i]);
		state.End = i + 1;

		return true;
	}

	public static List<Token> Consume(Context context, PatternState state, List<System.Type> patterns)
	{
		if (patterns.Exists(i => !i.IsSubclassOf(typeof(Pattern)))) throw new ArgumentException("Pattern list contained a non-pattern type");

		var clone = new List<Token>(state.Tokens.Select(i => (Token)i.Clone()));
		var consumption = new List<Consumption>();

		for (var priority = state.Max; priority >= state.Min;)
		{
			var instance = Next(context, clone, state.Min, priority, patterns, state.End);

			if (instance == null)
			{
				priority--;
				continue;
			}

			// Build the pattern into a node
			var pattern = instance.Pattern;
			var node = pattern.Build(context, instance.State, instance.Formatted);

			instance.Tokens = clone.Sublist(instance.State.Start, instance.State.End);

			if (node != null)
			{
				// Calculate how many tokens the pattern holds inside
				var range = instance.GetRange();
				var start = range.Start.Value;
				var length = range.End.Value - start;
				var count = instance.Tokens.Skip(start).Take(length).Sum(t => consumption.FirstOrDefault(i => i.Token == t)?.Count ?? 1);

				// Replace the pattern with a dynamic token
				var token = new DynamicToken(node);
				consumption.Add(new Consumption(token, count));

				instance.Replace(token);
			}
			else
			{
				Console.WriteLine("Warning: Consumption encountered unverified state");
				instance.Remove();
			}
		}

		// Return an empty list if nothing was consumed
		if (state.End >= clone.Count) return new List<Token>();

		List<Token>? consumed;

		if (clone[state.End].Type == TokenType.DYNAMIC)
		{
			var count = consumption.Find(i => i.Token == clone[state.End].To<DynamicToken>())?.Count ?? 1;
			consumed = state.Tokens.GetRange(state.End, count);

			state.Formatted.AddRange(consumed);
			state.End += count;

			return consumed;
		}

		consumed = new List<Token> { state.Tokens[state.End++] };
		state.Formatted.AddRange(consumed);

		return consumed;
	}

	/// <summary>
	/// Parses tokens into a node tree and attaches it to the parent node. 
	/// Parsing is done by looking for prioritized patterns which are filtered using the min and max priority parameters.
	/// </summary>
	public static void Parse(Node parent, Context context, List<Token> tokens, int min = MIN_PRIORITY, int max = MAX_PRIORITY)
	{
		RemoveLineEndingDuplications(tokens);
		CreateFunctionTokens(tokens);

		var denylist = new List<System.Type>();

		for (var priority = max; priority >= min; priority--)
		{
			Instance? instance;

			// Find all patterns with the current priority
			while ((instance = Next(context, tokens, min, priority, denylist)) != null)
			{
				// Build the pattern into a node
				var pattern = instance.Pattern;
				var node = pattern.Build(context, instance.State, instance.Formatted);

				instance.Tokens = tokens.Sublist(instance.State.Start, instance.State.End);

				if (node != null)
				{
					// Replace the pattern with a dynamic token
					instance.Replace(new DynamicToken(node));
				}
				else
				{
					instance.Remove();
				}
			}
		}

		// Try to find drifting tokens
		var i = tokens.FindIndex(j => !j.Is(TokenType.DYNAMIC, TokenType.END));

		if (i != -1)
		{
			throw Errors.Get(tokens[i].Position, "Can not understand");
		}

		// Combine all dynamic tokens in order
		foreach (var dynamic in tokens.Where(token => token.Type == TokenType.DYNAMIC).Cast<DynamicToken>())
		{
			parent.Add(dynamic.Node);
		}
	}

	/// <summary>
	/// Forms function tokens from the specified tokens
	/// </summary>
	public static void CreateFunctionTokens(List<Token> tokens)
	{
		if (tokens.Count < FUNCTION_LENGTH) return;

		for (var i = tokens.Count - 2; i >= 0;)
		{
			var current = tokens[i];

			if (current.Type == TokenType.IDENTIFIER)
			{
				var next = tokens[i + 1];

				if (next.Type == TokenType.CONTENT)
				{
					var parameters = (ContentToken)next;

					if (Equals(parameters.Type, ParenthesisType.PARENTHESIS))
					{
						var name = (IdentifierToken)current;
						var function = new FunctionToken(name, parameters)
						{
							Position = name.Position
						};

						tokens[i] = function;
						tokens.RemoveAt(i + 1);

						i -= FUNCTION_LENGTH;
						continue;
					}
				}
			}

			i--;
		}
	}

	/// <summary>
	/// Removes all line ending duplications
	/// </summary>
	public static void RemoveLineEndingDuplications(List<Token> tokens)
	{
		for (var i = 0; i < tokens.Count - 1;)
		{
			if (tokens[i].Type != TokenType.END || tokens[i + 1].Type != TokenType.END)
			{
				i++;
				continue;
			}

			tokens.RemoveAt(i);
		}
	}

	/// <summary>
	/// Creates a root context
	/// </summary>
	public static Context CreateRootContext(int identity)
	{
		var context = new Context(identity.ToString(CultureInfo.InvariantCulture));
		Primitives.Inject(context);

		return context;
	}

	/// <summary>
	/// Creates a root node
	/// </summary>
	public static Node CreateRootNode(Context context)
	{
		var positive_infinity = Variable.Create(context, Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL), VariableCategory.GLOBAL, Lexer.POSITIVE_INFINITY_CONSTANT, Modifier.DEFAULT | Modifier.CONSTANT);
		var negative_infinity = Variable.Create(context, Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL), VariableCategory.GLOBAL, Lexer.NEGATIVE_INFINITY_CONSTANT, Modifier.DEFAULT | Modifier.CONSTANT);

		var true_constant = Variable.Create(context, Primitives.CreateBool(), VariableCategory.GLOBAL, "true", Modifier.DEFAULT | Modifier.CONSTANT);
		var false_constant = Variable.Create(context, Primitives.CreateBool(), VariableCategory.GLOBAL, "false", Modifier.DEFAULT | Modifier.CONSTANT);

		return new ScopeNode(context, null, null)
		{
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(positive_infinity),
				new NumberNode(Format.DECIMAL, double.PositiveInfinity)
			),
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(negative_infinity),
				new NumberNode(Format.DECIMAL, double.NegativeInfinity)
			),
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(true_constant),
				new CastNode(new NumberNode(Format.UINT64, 1L), new TypeNode(Primitives.CreateBool()))
			),
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(false_constant),
				new CastNode(new NumberNode(Format.UINT64, 0L), new TypeNode(Primitives.CreateBool()))
			)
		};
	}

	public static int Print(Node root, int identation = 0, int total = 0)
	{
		var padding = new char[identation * 2];
		Array.Fill(padding, ' ');
		
		Console.Write(new string(padding));
		Console.WriteLine(root.ToString());

		total++;

		foreach (var child in root)
		{
			total += Print(child, identation + 1);
		}

		return total;
	}
}
