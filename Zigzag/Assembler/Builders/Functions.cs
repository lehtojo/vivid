public class Functions
{

	public const string HEADER = "{0}:" + "\n" +
								 "push ebp" + "\n" +
								 "mov ebp, esp" + "\n";

	public const string RESERVE = "sub esp, {0}";

	public const string FOOTER = "mov esp, ebp" + "\n" +
								 "pop ebp" + "\n" +
								 "ret";

	private static string GetConstructorFooter(Unit unit)
	{
		Reference source;

		var register = unit.GetObjectPointer();

		if (register != null)
		{
			source = new RegisterReference(register);
		}
		else
		{
			source = References.GetObjectPointer(unit);
		}

		var instructions = new Instructions();
		var reference = Memory.Move(unit, instructions, source, new RegisterReference(unit.EAX));
		instructions.SetReference(reference);

		return instructions.ToString();
	}

	public static string Build(Function function)
	{
		if (Flag.Has(function.Modifiers, AccessModifier.EXTERNAL))
		{
			return $"extern {function.GetFullname()}";
		}

		var result = new Builder();

		foreach (var implementation in function.Implementations)
		{
			if (implementation.Node == null || implementation.IsInline)
			{
				continue;
			}

			var builder = new Builder();

			if (function.IsGlobal)
			{
				builder.Comment($"Represents global function '{function.Name}'");
			}
			else if (function.IsConstructor)
			{
				builder.Comment($"Constructor of type {function.GetTypeParent().Name}");
			}
			else
			{
				builder.Comment($"Member function '{function.Name}' of type '{function.GetTypeParent().Name}'");
			}

			// Append the function stack frame
			builder.Append(HEADER, function.GetFullname());

			// Add instructions for local variables
			var memory = implementation.LocalMemorySize;

			if (memory > 0)
			{
				builder.Append(RESERVE, memory);
			}

			var unit = new Unit(function.GetFullname());

			// Assemble the body of this function
			var iterator = implementation.Node;

			while (iterator != null)
			{
				var instructions = unit.Assemble(iterator);
				unit.Step(instructions);

				if (instructions != null)
				{
					builder.Append(instructions.ToString());
				}

				iterator = iterator.Next;
			}

			if (function.IsConstructor)
			{
				var footer = GetConstructorFooter(unit);
				builder.Comment("Return the object pointer to the caller");
				builder.Append(footer);
			}

			var stack = unit.Stack;
			{
				var instructions = new Instructions();
				stack.Restore(instructions);

				builder.Append(instructions.ToString());
			}

			// Append the stack frame cleanup
			builder = builder.Append(FOOTER);

			result.Append(builder);
		}

		return result.ToString();
	}
}