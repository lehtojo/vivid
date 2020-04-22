using System;
using System.Collections;
using System.Collections.Generic;

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

	public T this[int index] { get => Parent[Start + index]; set => Parent[Start + index] = value; }

	public int Count => End - Start;

	public bool IsReadOnly => throw new NotImplementedException();

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
		return Parent.GetRange(Start, End).GetEnumerator();
	}

	public int IndexOf(T item)
	{
		return Parent.GetRange(Start, End).IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		Parent.Insert(Start + index, item);
		End++;
	}

	public bool Remove(T item)
	{
		int i = IndexOf(item);

		if (i != -1)
		{
			Parent.RemoveAt(i);
			End--;

			return true;
		}

		return false;
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

public static class Extensions
{
	public static IList<T> Sublist<T>(this List<T> list, int start, int end)
	{
		return new Sublist<T>(list, start, end);
	}
}

public class Parser
{
	public static Size Size { get; private set; } = Size.QWORD;

	public const int MAX_PRIORITY = 21;
	public const int MEMBERS = 19;
	public const int MIN_PRIORITY = 1;

	private class Instance
	{
		public Pattern Pattern { get; private set; }
		public IList<Token> Tokens { get; private set; }
		public List<Token> Molded { get; private set; }

		public Instance(Pattern pattern, IList<Token> tokens, List<Token> molded)
		{
			Pattern = pattern;
			Tokens = tokens;
			Molded = molded;
		}

		public void Remove()
		{
			int start = Pattern.GetStart();
			int end = Pattern.GetEnd();
			int count = (end == -1 ? Tokens.Count : end) - start;

			for (int i = 0; i < count; i++)
			{
				Tokens.RemoveAt(start);
			}
		}

		public void Replace(DynamicToken token)
		{
			int start = Pattern.GetStart();
			int end = Pattern.GetEnd();
			int count = (end == -1 ? Tokens.Count : end) - start;

			for (int i = 0; i < count; i++)
			{
				Tokens.RemoveAt(start);
			}

			Tokens.Insert(start, token);
		}
	}

	private static List<Token> Mold(List<int> indices, IList<Token> candidate)
	{
		var molded = new List<Token>(candidate);

		foreach (var index in indices)
		{
			molded.Insert(index, new Token(TokenType.NONE));
		}

		return molded;
	}

	/// <summary>
	/// Tries to find the next pattern from the tokens by comparing the priority
	/// </summary>
	/// <param name="context">Current context</param>
	/// <param name="tokens">Tokens to scan through</param>
	/// <param name="priority">Pattern priority used for filtering</param>
	/// <returns>Success: Next important patterin in tokens, Failure null</returns>
	private static Instance? Next(Context context, List<Token> tokens, int priority)
	{
		for (var start = 0; start < tokens.Count; start++)
		{
			// Start from the root
			var patterns = (Patterns?)Patterns.Root;

			if (patterns == null)
			{
				throw new ApplicationException("The root of the patterns wasn't initialized");
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

					foreach (Patterns.Option option in patterns.Options)
					{
						var pattern = option.Pattern;
						var molded = Mold(option.Missing, candidate);

						if (pattern.GetPriority(molded) == priority && pattern.Passes(context, molded))
						{
							instance = new Instance(pattern, candidate, molded);
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
	/// Parses tokens with the default minimum and maximum priority range
	/// </summary>
	/// <param name="context">Current context</param>
	/// <param name="tokens">Tokens to iterate</param>
	/// <returns>Parsed node tree</returns>
	public static Node Parse(Context context, List<Token> tokens)
	{
		return Parser.Parse(context, tokens, MIN_PRIORITY, MAX_PRIORITY);
	}

	/// /// <summary>
	/// Parses tokens with minimum and maximum priority range
	/// </summary>
	/// <param name="context">Current context</param>
	/// <param name="tokens">Tokens to iterate</param>
	/// <returns>Parsed node tree</returns>
	public static Node Parse(Context context, List<Token> tokens, int min, int max)
	{
		var node = new Node();
		Parser.Parse(node, context, tokens, min, max);

		return node;
	}

	/// <summary>
	/// Parses tokens and adds the resulting nodes to the given node
	/// </summary>
	/// <param name="parent">Node which receives the parsed nodes</param>
	/// <param name="context">Current context</param>
	/// <param name="tokens">Tokens to iterate</param>
	public static void Parse(Node parent, Context context, List<Token> tokens)
	{
		Parser.Parse(parent, context, tokens, MIN_PRIORITY, MAX_PRIORITY);
	}

	/// <summary>
	/// Parses tokens into a node tree and attaches it to the parent node. 
	/// Parsing is done by looking for prioritized patterns which are filtered using the min and max priority parameters.
	/// </summary>
	/// <param name="parent">Node which will receive the node tree</param>
	/// <param name="context">Context to append metadata of the parsed content</param>
	/// <param name="tokens">Tokens to iterate</param>
	/// <param name="min">Minimum priority for pattern filtering</param>
	/// <param name="max">Maximum priority for pattern filtering</param>
	public static void Parse(Node parent, Context context, List<Token> tokens, int min, int max)
	{
		for (int priority = max; priority >= min; priority--)
		{
			Instance? instance;

			// Find all patterns with the current priority
			while ((instance = Next(context, tokens, priority)) != null)
			{
				// Build the pattern into a node
				var pattern = instance.Pattern;
				var node = pattern.Build(context, instance.Molded);

				if (node != null)
				{
					// Replace the pattern with a dynamic token
					var token = new DynamicToken(node);
					instance.Replace(token);
				}
				else
				{
					instance.Remove();
				}
			}
		}

		// Combine all processed tokens in order
		foreach (var token in tokens)
		{
			if (token.Type == TokenType.DYNAMIC)
			{
				var dynamic = (DynamicToken)token;
				parent.Add(dynamic.Node);
			}
		}
	}

	/// <summary>
	/// Creates a base context
	/// </summary>
	/// <returns>Base context</returns>
	public static Context Initialize()
	{
		var context = new Context();
		Types.Inject(context);

		var number = context.GetType("num") ?? throw new ApplicationException("Couldn't find type 'num'");

		var allocate = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE, 
			"allocate", 
			Types.LINK, 
			new Parameter() { Name = "bytes", Type = number }
		);
		
		var power = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"integer_power",
			number,
			new Parameter() { Name = "a", Type = number },
			new Parameter() { Name = "b", Type = number }
		);

		var system_print = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"sys_print",
			Types.UNKNOWN,
			new Parameter() { Name = "address", Type = Types.LINK },
			new Parameter() { Name = "count", Type = number }
		);

		var system_read = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"sys_read",
			number,
			new Parameter() { Name = "buffer", Type = Types.LINK },
			new Parameter() { Name = "count", Type = number }
		);

		var copy = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"copy",
			Types.UNKNOWN,
			new Parameter() { Name = "source", Type = Types.LINK },
			new Parameter() { Name = "bytes", Type = number },
			new Parameter() { Name = "destination", Type = Types.LINK }
		);

		var offset_copy = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"offset_copy",
			Types.UNKNOWN,
			new Parameter() { Name = "source", Type = Types.LINK },
			new Parameter() { Name = "bytes", Type = number },
			new Parameter() { Name = "destination", Type = Types.LINK },
			new Parameter() { Name = "offset", Type = number }
		);

		var free = new Function
		(
			AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
			"free",
			Types.UNKNOWN,
			new Parameter() { Name = "address", Type = Types.LINK }
		);

		context.Declare(allocate);
		context.Declare(power);
		context.Declare(system_print);
		context.Declare(system_read);
		context.Declare(copy);
		context.Declare(offset_copy);
		context.Declare(free);

		return context;
	}
}
