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
	public const int MAX_PRIORITY = 21;
	public const int MEMBERS = 20;
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

		public void Replace(DynamicToken token)
		{
			int start = Pattern.GetStart();
			int end = Pattern.GetEnd();
			int count = (end == -1 ? Tokens.Count : end) - start;

			for (int i = 0; i < count; i++)
			{
				Tokens.RemoveAt(start);
			}

			Tokens.Add(token);
		}
	}

	private static List<Token> Mold(List<int> indices, IList<Token> candidate)
	{
		List<Token> molded = new List<Token>(candidate);

		foreach (int index in indices)
		{
			molded.Insert(index, null);
		}

		return molded;
	}

	private static Instance Next(List<Token> tokens, int priority)
	{
		for (int start = 0; start < tokens.Count; start++)
		{
			// Start from the root
			Patterns patterns = Patterns.Root;
			Instance instance = null;

			// Try finding the next pattern
			for (int end = start; end < tokens.Count; end++)
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
					IList<Token> candidate = tokens.Sublist(start, end + 1);

					foreach (Patterns.Option option in patterns.Options)
					{
						Pattern pattern = option.Pattern;
						List<Token> molded = Mold(option.Missing, candidate);

						if (pattern.GetPriority(molded) == priority && pattern.Passes(molded))
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

	public static Node Parse(Context context, List<Token> section)
	{
		return Parser.Parse(context, section, MIN_PRIORITY, MAX_PRIORITY);
	}

	public static Node Parse(Context context, List<Token> tokens, int priority)
	{
		return Parser.Parse(context, tokens, priority, priority);
	}

	public static Node Parse(Context context, List<Token> tokens, int minPriority, int maxPriority)
	{
		Node node = new Node();
		Parser.Parse(node, context, tokens, minPriority, maxPriority);

		return node;
	}

	public static void Parse(Node parent, Context context, List<Token> tokens)
	{
		Parser.Parse(parent, context, tokens, MIN_PRIORITY, MAX_PRIORITY);
	}

	public static void Parse(Node parent, Context context, List<Token> tokens, int priority)
	{
		Parser.Parse(parent, context, tokens, priority, priority);
	}

	public static void Parse(Node parent, Context context, List<Token> tokens, int minPriority, int maxPriority)
	{
		for (int priority = maxPriority; priority >= minPriority; priority--)
		{
			Instance instance;

			// Find all patterns with the current priority
			while ((instance = Next(tokens, priority)) != null)
			{
				// Build the pattern into a node
				Pattern pattern = instance.Pattern;
				Node node = pattern.Build(context, instance.Molded);

				// Replace the pattern with a processed token
				DynamicToken token = new DynamicToken(node);
				instance.Replace(token);
			}
		}

		// Combine all processed tokens in order
		foreach (Token token in tokens)
		{
			if (token.Type == TokenType.DYNAMIC)
			{
				DynamicToken dynamic = (DynamicToken)token;
				parent.Add(dynamic.Node);
			}
		}
	}

	public static void Hull(Node parent, Context context, List<Token> tokens)
	{
		Parser.Parse(parent, context, tokens, Parser.MEMBERS, Parser.MAX_PRIORITY);
	}

	public static Context Initialize()
	{
		Context context = new Context();
		Types.Inject(context);

		Function allocate = new Function(context, "allocate", AccessModifier.PUBLIC | AccessModifier.EXTERNAL, Types.LINK);
		Variable bytes = new Variable(allocate, Types.NORMAL, VariableCategory.PARAMETER, "bytes", AccessModifier.PUBLIC);
		allocate.SetParameters(bytes);

		Function power = new Function(context, "integer_power", AccessModifier.PUBLIC | AccessModifier.EXTERNAL, Types.NORMAL);
		Variable @base = new Variable(power, Types.NORMAL, VariableCategory.PARAMETER, "a", AccessModifier.PUBLIC);
		Variable exponent = new Variable(power, Types.NORMAL, VariableCategory.PARAMETER, "b", AccessModifier.PUBLIC);
		power.SetParameters(@base, exponent);

		return context;
	}
}
