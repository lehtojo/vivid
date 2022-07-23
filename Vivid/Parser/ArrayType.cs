using System.Collections.Generic;

public class ArrayType : Number, IResolvable
{
	public Type Element { get; private set; }
	private List<Token> Tokens { get; set; }
	public Node? Expression { get; private set; }
	public long Count => (long)Expression!.To<NumberNode>().Value;

	public ArrayType(Context context, Type element, ContentToken count, Position? position) : base(Parser.Format, Size.QWORD.Bits, true, element.ToString() + "[]")
	{
		Modifiers = Modifier.DEFAULT | Modifier.PRIMITIVE | Modifier.INLINE;
		Element = element;
		Tokens = count.Tokens;
		Position = position;
		TemplateArguments = new[] { element };

		TryParse(context);
	}

	public override int GetAllocationSize()
	{
		if (IsUnresolved) throw Errors.Get(Position, "Array size was not resolved");

		var count = (long)Expression!.To<NumberNode>().Value;
		return Element.AllocationSize * (int)count;
	}

	public override int GetContentSize()
	{
		return GetAllocationSize();
	}

	/// <summary>
	/// Try to parse the expression using the internal tokens
	/// </summary>
	private void TryParse(Context context)
	{
		try { Expression = Parser.Parse(context, Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY); } catch {}
	}
	
	public override Type GetOffsetType()
	{
		return Element;
	}

	public Node? Resolve(Context context)
	{
		// Ensure the expression is created
		if (Expression == null)
		{
			TryParse(context);
			if (Expression == null) return null;
		}

		if (Expression.First == null) return null;

		// Insert values of constants manually
		Analyzer.ApplyConstantsInto(Expression);

		// Try to convert the expression into a constant number
		if (Expression.First.Instance != NodeType.NUMBER)
		{
			var evaluated = Analysis.GetSimplifiedValue(Expression.First);
			if (evaluated.Instance != NodeType.NUMBER || evaluated.To<NumberNode>().Type == Format.DECIMAL) return null;

			Expression.First.Replace(evaluated);
		}

		Expression = new NumberNode(Parser.Format, (long)Expression.First.To<NumberNode>().Value);
		return null;
	}

	public override bool IsResolved()
	{
		return Expression != null && Expression.Is(NodeType.NUMBER);
	}

	public Status GetStatus()
	{
		return IsUnresolved ? Status.Error(Position, "Can not convert the size of the array to a constant number") : Status.OK;
	}

	public override string ToString()
	{
		var size = Expression != null && Expression.Instance == NodeType.NUMBER
			? Expression.To<NumberNode>().Value.ToString()
			: "?";

		return Element.ToString() + $"[{size}]";
	}
}