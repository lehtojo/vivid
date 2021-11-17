using System.Collections.Generic;
using System.Linq;

public class ParameterAligner
{
	public int StandardRegisters { get; set; }
	public int DecimalRegisters { get; set; }
	public int Position { get; set; } = 0;

	public ParameterAligner(int position)
	{
		StandardRegisters = Calls.GetStandardParameterRegisters().Count();
		DecimalRegisters = Calls.GetMaxMediaRegisterParameters();
		Position = position;
	}

	/// <summary>
	/// Consumes the specified type while taking into account if it is a pack
	/// </summary>
	private void Align(Variable parameter)
	{
		var type = parameter.Type!;

		if (type.IsPack)
		{
			var representives = Common.GetPackRepresentives(parameter);
			foreach (var representive in representives) { Align(representive); }
			return;
		}

		// First, try to consume a register for the parameter
		if (type.Format.IsDecimal() && DecimalRegisters-- > 0 || !type.Format.IsDecimal() && StandardRegisters-- > 0)
		{
			// On Windows even though the first parameters are passed in registers, they still need have their own stack alignment (shadow space)
			if (!Assembler.IsTargetWindows) return;
		}

		// Normal parameters consume one stack unit
		parameter.LocalAlignment = Position;
		Position += Parser.Bytes;
	}

	/// <summary>
	/// Aligns the specified parameters
	/// </summary>
	public void Align(List<Variable> parametes)
	{
		foreach (var parameter in parametes) { Align(parameter); }
	}
}

public static class Aligner
{
	private static int GlobalFunctionParameterOffset => Assembler.IsArm64 ? 0 : 1;

	/// <summary>
	/// Aligns all variable and parameters recursively in the context
	/// </summary>
	public static void Align(Context context)
	{
		// Align types and subtypes
		foreach (var type in context.Types.Values)
		{
			Align(type);
		}

		// Align function variables in memory
		foreach (var implementation in context.Functions.Values.SelectMany(i => i.Overloads).SelectMany(i => i.Implementations))
		{
			if (implementation.Node == null) continue;

			// Align function parameters using global function offset
			Align(implementation, GlobalFunctionParameterOffset);
		}
	}

	/// <summary>
	/// Align all used local variables and allocate memory for other kinds of local memory such as temporary handles and stack allocation handles
	/// </summary>
	public static void AlignLocalMemory(IEnumerable<Variable> variables, List<TemporaryMemoryHandle> temporary_handles, List<InlineHandle> inline_handles, int top)
	{
		var position = -top;

		foreach (var variable in variables)
		{
			position -= variable.Type!.AllocationSize;
			variable.LocalAlignment = position;
		}

		while (temporary_handles.Count > 0)
		{
			var first = temporary_handles.First();
			var identifier = first.Identifier;

			position -= first.Size.Bytes;

			var copies = temporary_handles.Where(i => i.Identifier.Equals(identifier)).ToList();

			copies.ForEach(i => i.Offset = position);
			copies.ForEach(i => temporary_handles.Remove(i));
		}

		foreach (var temporary_handle in temporary_handles)
		{
			temporary_handle.Offset = position;
		}

		foreach (var iterator in inline_handles.GroupBy(i => i.Identity))
		{
			position -= iterator.First().Bytes;

			foreach (var inline_handle in iterator)
			{
				inline_handle.Offset = position;
			}
		}
	}

	/// <summary>
	/// Aligns member variables, function and subtypes
	/// </summary>
	public static void Align(Type type)
	{
		var position = 0;

		// Member variables:
		foreach (var variable in type.Variables.Values)
		{
			if (variable.IsStatic) continue;
			variable.LocalAlignment = position;
			position += variable.Type!.AllocationSize;
		}

		var implementations = type.Functions.Values.SelectMany(i => i.Overloads)
			.Concat(type.Constructors.Overloads)
			.Concat(type.Destructors.Overloads)
			.Concat(type.Overrides.Values.SelectMany(i => i.Overloads))
			.SelectMany(i => i.Implementations);

		// Member functions:
		foreach (var implementation in implementations)
		{
			if (implementation.Node == null) continue;
			Align(implementation, GlobalFunctionParameterOffset);
		}

		// Align subtypes
		foreach (var subtype in type.Types.Values)
		{
			Align(subtype);
		}
	}

	/// <summary>
	/// Aligns function variables
	/// </summary>
	private static void Align(FunctionImplementation function, int offset)
	{
		// Align all lambdas
		Align(function);

		var parameters = new List<Variable>(function.Parameters);

		// Align the self pointer as well, if it exists
		if (function.Variables.ContainsKey(Function.SELF_POINTER_IDENTIFIER))
		{
			parameters.Insert(0, function.Variables[Function.SELF_POINTER_IDENTIFIER]);
		}
		else if (function.Variables.ContainsKey(Lambda.SELF_POINTER_IDENTIFIER))
		{
			parameters.Insert(0, function.Variables[Lambda.SELF_POINTER_IDENTIFIER]);
		}

		var aligner = new ParameterAligner(Assembler.IsArm64 ? 0 : Parser.Bytes);
		aligner.Align(parameters);
	}
}