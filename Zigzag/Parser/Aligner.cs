public class Aligner
{
	private const int MEMBER_FUNCTION_PARAMETER_OFFSET = 4;
	private const int GLOBAL_FUNCTION_PARAMETER_OFFSET = 0;

	/**
     * Aligns all variables and parameters recursively in the given context
     * @param context Context to process
     */
	public static void Align(Context context)
	{
		// Align types and their subtypes
		foreach (Type type in context.Types.Values)
		{
			Aligner.Align(type);
		}

		// Align function variables and parameters
		foreach (FunctionList functions in context.Functions.Values)
		{
			foreach (Function function in functions.Instances)
			{
				Aligner.Align(function, GLOBAL_FUNCTION_PARAMETER_OFFSET);
			}
		}
	}

	/**
     * Aligns member variables, functions and subtypes of the given type
     * @param type Type to align
     */
	private static void Align(Type type)
	{
		int position = 0;

		// Align member variables
		foreach (Variable variable in type.Variables.Values)
		{
			variable.Alignment = position;
			position += variable.Type.Size;
		}

		foreach (FunctionList functions in type.Functions.Values)
		{
			foreach (Function function in functions.Instances)
			{
				Aligner.Align(function, MEMBER_FUNCTION_PARAMETER_OFFSET);
			}
		}

		// Align constructors
		foreach (Function constructor in type.GetConstructors().Instances)
		{
			Aligner.Align(constructor, MEMBER_FUNCTION_PARAMETER_OFFSET);
		}

		// Align subtypes
		foreach (Type subtype in type.Types.Values)
		{
			Aligner.Align(subtype);
		}
	}

	/**
     * Aligns function variables
     * @param function Function to align
     * @param offset Parameter offset in stack
     */
	private static void Align(Function function, int offset)
	{
		int position = offset;

		// Align parameters
		foreach (Variable variable in function.Parameters)
		{
			if (variable.Category == VariableCategory.PARAMETER)
			{
				variable.Alignment = position;
				position += variable.Type.Size;
			}
		}

		position = 0;

		// Align local variables
		foreach (Variable variable in function.Locals)
		{
			if (variable.Category == VariableCategory.LOCAL)
			{
				variable.Alignment = position;
				position += variable.Type.Size;
			}
		}
	}
}