public class Aligner
{
	private const int MEMBER_FUNCTION_PARAMETER_OFFSET = 4;
	private const int GLOBAL_FUNCTION_PARAMETER_OFFSET = 0;
	
	/// <summary>
	/// Aligns all variable and parameters recursively in the context
	/// </summary>
	/// <param name="context">Context to scan through</param>
	public static void Align(Context context)
	{
		// Align types and subtypes
		foreach (var type in context.Types.Values)
		{
			Aligner.Align(type);
		}

		// Align function variables in memory
		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				foreach (var implementation in overload.Implementations)
				{
					// Align function parameters using global function offset
					Aligner.Align(implementation, GLOBAL_FUNCTION_PARAMETER_OFFSET);
				}
			}
		}
	}

	/// <summary>
	/// Aligns member variables, function and subtypes
	/// </summary>
	/// <param name="type">Type to scan through</param>
	private static void Align(Type type)
	{
		var position = 0;

		// Member variables:
		foreach (var variable in type.Variables.Values)
		{
			variable.Alignment = position;
			position += variable.Type.Size;
		}

		// Member functions:
		foreach (var function in type.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				foreach (var implementation in overload.Implementations)
				{
					Aligner.Align(implementation, MEMBER_FUNCTION_PARAMETER_OFFSET);
				}
			}
		}

		// Constructors:
		foreach (var constructor in type.GetConstructors().Overloads)
		{
			foreach (var implementation in constructor.Implementations)
			{
				Aligner.Align(implementation, MEMBER_FUNCTION_PARAMETER_OFFSET);
			}
		}

		// Destructors:
		foreach (var destructor in type.GetDestructors().Overloads)
		{
			foreach (var implementation in destructor.Implementations)
			{
				Aligner.Align(implementation, MEMBER_FUNCTION_PARAMETER_OFFSET);
			}
		}

		// Align subtypes
		foreach (var subtype in type.Types.Values)
		{
			Aligner.Align(subtype);
		}
	}

	/// <summary>
	/// Aligns function variables
	/// </summary>
	/// <param name="function">Function to scan through</param>
	/// <param name="offset">Base offset to apply to all variables</param>
	private static void Align(FunctionImplementation function, int offset)
	{
		var position = offset;

		// Parameters:
		foreach (var variable in function.Parameters)
		{
			if (variable.Category == VariableCategory.PARAMETER)
			{
				variable.Alignment = position;
				position += variable.Type.Size;
			}
		}

		position = 0;

		// Local variables:
		foreach (var variable in function.Locals)
		{
			if (variable.Category == VariableCategory.LOCAL)
			{
				variable.Alignment = position;
				position += variable.Type.Size;
			}
		}
	}
}