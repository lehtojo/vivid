public class Functions
{

	public const string HEADER = "{0}:" + "\n" +
								 "push ebp" + "\n" +
								 "mov ebp, esp" + "\n";

	public const string RESERVE = "sub esp, {0}";

	public const string FOOTER = "mov esp, ebp" + "\n" +
								 "pop ebp" + "\n" +
								 "ret";

	public static string Build(FunctionNode node)
	{
		Function function = node.Function;
		Builder builder = new Builder();

		if (Flag.Has(function.Modifiers, AccessModifier.EXTERNAL))
		{
			builder.Append("extern {0}", function.GetFullname());
			return builder.ToString();
		}

		if (function.IsGlobal)
		{
			builder.Comment($"Represents global function '{function.Name}'");
		}
		else if (function is Constructor constructor)
		{
			builder.Comment($"Constructor of type {constructor.GetTypeParent().Name}");
		}
		else
		{
			builder.Comment($"Member function '{function.Name}' of type '{function.GetTypeParent().Name}'");
		}

		// Append the function stack frame
		builder.Append(HEADER, function.GetFullname());

		// Add instructions for local variables
		int memory = function.LocalMemorySize;

		if (memory > 0)
		{
			builder.Append(RESERVE, memory);
		}

		Unit unit = new Unit(function.GetFullname());

		// Assemble the body of this function
		Node iterator = node.Body.First;

		while (iterator != null)
		{
			Instructions instructions = unit.Assemble(iterator);

			if (instructions != null)
			{
				builder.Append(instructions.ToString());
			}

			unit.Step();

			iterator = iterator.Next;
		}

		// Append the stack frame cleanup
		builder = builder.Append(FOOTER);

		return builder.ToString();
	}
}