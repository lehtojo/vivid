public class Functions
{

	public const string HEADER = "{0}:" + "\n" +
								 "push ebp" + "\n" +
								 "mov ebp, esp";

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
			builder.Append("extern {0}", function.Fullname);
			return builder.ToString();
		}

		if (function.IsGlobal)
		{
			builder.comment("Represents global function '{0}'", function.Name);
		}
		else
		{
			builder.comment("Member function '{0}' of type '{1}'", function.Name, function.GetTypeParent().Name);
		}

		// Append the function stack frame
		builder.Append(HEADER, function.Fullname);

		// Add instructions for local variables
		int memory = function.LocalMemorySize;

		if (memory > 0)
		{
			builder.Append(RESERVE, memory);
		}

		Unit unit = new Unit(function.Fullname);

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