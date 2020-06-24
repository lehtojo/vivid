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
    public const int MIN_PRIORITY = 1;
    private const int FUNCTION_LENGTH = 2;

    private class Instance
    {
        public Pattern Pattern { get; private set; }
        private IList<Token> Tokens { get; set; }
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
    private static Instance? Next(Context context, List<Token> tokens, int priority)
    {
        for (var start = 0; start < tokens.Count; start++)
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
        CreateFunctionCalls(tokens);

        for (var priority = max; priority >= min; priority--)
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
            new Parameter() {Name = "bytes", Type = number}
        );

        var power = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "integer_power",
            number,
            new Parameter() {Name = "a", Type = number},
            new Parameter() {Name = "b", Type = number}
        );

        var system_print = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "sys_print",
            Types.UNKNOWN,
            new Parameter() {Name = "address", Type = Types.LINK},
            new Parameter() {Name = "count", Type = number}
        );

        var system_read = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "sys_read",
            number,
            new Parameter() {Name = "buffer", Type = Types.LINK},
            new Parameter() {Name = "count", Type = number}
        );

        var copy = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "copy",
            Types.UNKNOWN,
            new Parameter() {Name = "source", Type = Types.LINK},
            new Parameter() {Name = "bytes", Type = number},
            new Parameter() {Name = "destination", Type = Types.LINK}
        );

        var offset_copy = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "offset_copy",
            Types.UNKNOWN,
            new Parameter() {Name = "source", Type = Types.LINK},
            new Parameter() {Name = "bytes", Type = number},
            new Parameter() {Name = "destination", Type = Types.LINK},
            new Parameter() {Name = "offset", Type = number}
        );

        var deallocate = new Function
        (
            AccessModifier.PUBLIC | AccessModifier.EXTERNAL | AccessModifier.RESPONSIBLE,
            "deallocate",
            Types.UNKNOWN,
            new Parameter() {Name = "address", Type = Types.LINK},
            new Parameter() {Name = "bytes", Type = number}
        );

        context.Declare(allocate);
        context.Declare(power);
        context.Declare(system_print);
        context.Declare(system_read);
        context.Declare(copy);
        context.Declare(offset_copy);
        context.Declare(deallocate);

        return context;
    }
}