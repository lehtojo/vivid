using System;
using System.Collections;
using System.Collections.Generic;
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

        if (i == -1) return false;
        
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
    public static Size Size { get; set; } = Size.QWORD;

    public const int MAX_PRIORITY = 23;
    public const int MEMBERS = 19;
    public const int MIN_PRIORITY = 0;
    private const int FUNCTION_LENGTH = 2;

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
            var start = Pattern.GetStart();
            var end = Pattern.GetEnd();
            var count = (end == -1 ? Tokens.Count : end) - start;

            for (var i = 0; i < count; i++)
            {
                Tokens.RemoveAt(start);
            }
        }

        public int GetStart(List<Token> tokens)
        {
            return tokens.IndexOf(Tokens.First());
        }

        private int GetAbsolutePosition(int virtual_position)
        {
            var absolute_position = 0;

            for (var i = 0; i < virtual_position; i++)
            {
                if (Molded[i].Type != TokenType.NONE)
                {
                    absolute_position++;
                }
            }

            return absolute_position;
        }

        public void Replace(DynamicToken token)
        {
            var absolute_start = GetAbsolutePosition(Pattern.GetStart());
            var virtual_end = Pattern.GetEnd();
            var count = (virtual_end == -1 ? Tokens.Count : GetAbsolutePosition(virtual_end)) - absolute_start;

            for (var i = 0; i < count; i++)
            {
                Tokens.RemoveAt(absolute_start);
            }

            Tokens.Insert(absolute_start, token);
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
    /// <returns>Success: Next important pattern in tokens, Failure null</returns>
    private static Instance? Next(Context context, List<Token> tokens, int min, int priority, List<System.Type> whitelist, int start = 0)
    {
        for (; start < tokens.Count; start++)
        {
            // Start from the root
            var patterns = Patterns.Root;

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

                    foreach (var option in patterns.Options)
                    {
                        var pattern = option.Pattern;
                        
                        // Skip this candidate if it's blacklisted
                        if (whitelist.Any() && !whitelist.Exists(i => i == pattern.GetType()))
                        {
                            continue;
                        }

                        var molded = Mold(option.Missing, candidate);

                        if (pattern.GetPriority(molded) == priority)
                        {
                            var state = new PatternState(
                                tokens,
                                start,
                                end + 1,
                                min,
                                priority
                            );

                            if (!pattern.Passes(context, state, molded))
                            {
                                continue;
                            }

                            // Determine how many tokens were consumed in the pass function
                            var count = state.End - (end + 1);

                            // Rearrange the consumed tokens if tokens where consumed
                            if (count > 0)
                            {
                                candidate = tokens.Sublist(start, state.End);
                                molded.AddRange(tokens.GetRange(end + 1, count));

                                end = state.End;
                            }

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

    /// /// <summary>
    /// Parses tokens with minimum and maximum priority range
    /// </summary>
    /// <param name="context">Current context</param>
    /// <param name="tokens">Tokens to iterate</param>
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
            if (!Flag.Has(type, state.Tokens[i++].Type))
            {
                return false;
            }
        }

        consumed = state.Tokens.GetRange(state.End, i - state.End);
        state.End += path.Length;

        return true;
    }

    public static List<Token> Consume(Context context, PatternState state, List<System.Type> patterns)
    {
        if (patterns.Exists(p => !p.IsSubclassOf(typeof(Pattern))))
		{
			throw new ArgumentException("Patterns whitelist contained a non-pattern type");
		}

        var result = new List<Token>();
        var clone = new List<Token>(state.Tokens);

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
            var node = pattern.Build(context, instance.Molded);

            if (node != null)
            {
                // Calculate how many tokens the pattern holds inside
                var count = instance.Tokens
                    .Sum(t => consumption.FirstOrDefault(i => i.Token == t)?.Count ?? 1);

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

        if (clone[state.End].Type == TokenType.DYNAMIC)
        {
            var count = consumption.Find(i => i.Token == clone[state.End].To<DynamicToken>())?.Count ?? 1;
            var consumed = state.Tokens.GetRange(state.End, count);

            state.End += count;

            return consumed;
        }

        return new List<Token> { state.Tokens[state.End++] };
    }

    private static List<Token> Consume(Context context, List<Token> tokens, int min, int max, List<System.Type> whitelist, int start)
    {
        var result = new List<Token>();
        var clone = new List<Token>(tokens);

        var consumption = new List<Consumption>();

        for (var priority = max; priority >= min;)
        {
            var instance = Next(context, clone, min, priority, whitelist, start);

            if (instance == null)
            {
                priority--;
                continue;
            }

            // Build the pattern into a node
            var pattern = instance.Pattern;
            var node = pattern.Build(context, instance.Molded);

            if (node != null)
            {
                // Calculate how many tokens the pattern holds inside
                var count = instance.Tokens
                    .Sum(t => consumption.FirstOrDefault(i => i.Token == t)?.Count ?? 1);

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

        if (clone[start].Type == TokenType.DYNAMIC)
        {
            var count = consumption.Find(i => i.Token == clone[start].To<DynamicToken>())?.Count ?? 1;
            
            var consumed = tokens.GetRange(start, count);
            tokens.RemoveRange(start, count);

            return consumed;
        }
        else
        {
            var consumed = new List<Token> { tokens[start] };
            tokens.RemoveAt(start);

            return consumed;
        }
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
    public static void Parse(Node parent, Context context, List<Token> tokens, int min = MIN_PRIORITY,
        int max = MAX_PRIORITY)
    {
        RemoveLineEndingDuplications(tokens);
        CreateFunctionCalls(tokens);

        var blacklist = new List<System.Type>();

        for (var priority = max; priority >= min; priority--)
        {
            Instance? instance;

            // Find all patterns with the current priority
            while ((instance = Next(context, tokens, min, priority, blacklist)) != null)
            {
                // Build the pattern into a node
                var pattern = instance.Pattern;
                
                Node? node;

                if (pattern is ConsumingPattern consumer)
                {
                    var state = new ConsumptionState();
                    node = consumer.Build(context, instance.Molded, state);

                    if (state.IsConsuming)
                    {
                        var start = tokens.IndexOf(instance.Tokens.First()) + instance.Tokens.Count;
                        var consumed = Consume(context, tokens, min, priority, state.Whitelist, start);
                        
                        state.Status = ConsumptionStatus.CONSUMED;
                        node = consumer.Build(context, instance.Molded.Concat(consumed).ToList(), state);
                    }
                }
                else
                {
                    node = pattern.Build(context, instance.Molded);
                }

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
        foreach (var dynamic in tokens.Where(token => token.Type == TokenType.DYNAMIC).Cast<DynamicToken>())
        {
            parent.Add(dynamic.Node);
        }
    }

    /// <summary>
    /// Forms function tokens from the given tokens
    /// </summary>
    /// <param name="tokens">Tokens to iterate</param>
    private static void CreateFunctionCalls(List<Token> tokens)
    {
        if (tokens.Count < FUNCTION_LENGTH)
        {
            return;
        }

        for (var i = tokens.Count - 2; i >= 0;)
        {
            var current = tokens[i];

            if (current.Type == TokenType.IDENTIFIER)
            {
                var next = tokens[i + 1];

                if (next.Type == TokenType.CONTENT)
                {
                    var parameters = (ContentToken) next;

                    if (Equals(parameters.Type, ParenthesisType.PARENTHESIS))
                    {
                        var name = (IdentifierToken) current;
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
    private static void RemoveLineEndingDuplications(List<Token> tokens)
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
    /// Creates a base context
    /// </summary>
    /// <returns>Base context</returns>
    public static Context Initialize()
    {
        var context = new Context();
        Types.Inject(context);

        var number = context.GetType("num") ?? throw new ApplicationException("Could not find type 'num'");

        var allocate = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "allocate",
            Types.LINK,
            new Parameter("bytes", number)
        );

        var copy = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "copy",
            Types.UNKNOWN,
            new Parameter("source", Types.LINK),
            new Parameter("bytes", number),
            new Parameter("destination", Types.LINK)
        );

        var offset_copy = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "offset_copy",
            Types.UNKNOWN,
            new Parameter("source", Types.LINK),
            new Parameter("bytes", number),
            new Parameter("destination", Types.LINK),
            new Parameter("offset", number)
        );

        var deallocate = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "deallocate",
            Types.UNKNOWN,
            new Parameter("address", Types.LINK),
            new Parameter("bytes", number)
        );

        context.Declare(allocate);
        context.Declare(copy);
        context.Declare(offset_copy);
        context.Declare(deallocate);

        return context;
    }
}
