using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public static class Aligner
{
	private static int MemberFunctionParameterOffset => Assembler.IsArm64 ? 1 : 2;
	private static int GlobalFunctionParameterOffset => Assembler.IsArm64 ? 0 : 1;

	/// <summary>
	/// Aligns all variable and parameters recursively in the context
	/// </summary>
	/// <param name="context">Context to scan through</param>
	public static void Align(Context context)
	{
		// Align types and subtypes
		foreach (var type in context.Types.Values)
		{
			Align(type);
		}

		// Align function variables in memory
		foreach (var implementation in context.GetImplementedFunctions())
		{
			// Align function parameters using global function offset
			Align(implementation, GlobalFunctionParameterOffset);
		}
	}

	/// <summary>
	/// Aligns the local memory used by a function
	/// </summary>
	public static void AlignLocalMemory(IEnumerable<Variable> variables, List<TemporaryMemoryHandle> temporary_handles, List<InlineHandle> inline_handles, int top)
	{
		var position = -top;

		foreach (var variable in variables)
		{
			position -= variable.Type!.ReferenceSize;
			variable.LocalAlignment = position;
		}

		while (temporary_handles.Count > 0)
		{
			var first = temporary_handles.First();
			var identifier = first.Identifier;

			position -= first.Size.Bytes;

			var copies = temporary_handles.Where(t => t.Identifier.Equals(identifier)).ToList();

			copies.ForEach(c => c.Offset = position);
			copies.ForEach(c => temporary_handles.Remove(c));
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
	/// <param name="type">Type to scan through</param>
	public static void Align(Type type)
	{
		var position = 0;

		// Member variables:
		foreach (var variable in type.Variables.Values)
		{
			variable.LocalAlignment = position;
			position += variable.Type!.ReferenceSize;
		}

		// Member functions:
		foreach (var implementation in type.GetImplementedFunctions())
		{
			Align(implementation, GlobalFunctionParameterOffset);
		}

		// Constructors:
		foreach (var constructor in type.GetConstructors().Overloads)
		{
			foreach (var implementation in constructor.Implementations)
			{
				Align(implementation, GlobalFunctionParameterOffset);
			}
		}

		// Destructors:
		foreach (var destructor in type.GetDestructors().Overloads)
		{
			foreach (var implementation in destructor.Implementations)
			{
				Align(implementation, MemberFunctionParameterOffset);
			}
		}

		// Align subtypes
		foreach (var subtype in type.Types.Values)
		{
			Align(subtype);
		}
	}

	private static void AlignLocals(FunctionImplementation implementation)
	{
		var locals = implementation.Locals;
		locals.Reverse();

		var position = 0;

		foreach (var local in locals)
		{
			position -= local.Type!.ReferenceSize;
			local.LocalAlignment = position;
		}

		foreach (var iterator in implementation.GetImplementedFunctions())
		{
			AlignLocals(iterator);
		}
	}

	public static void AlignLocals(Context context)
	{
		// Align types and subtypes
		foreach (var type in context.Types.Values)
		{
			AlignLocals(type);
		}

		foreach (var implementation in context.GetImplementedFunctions())
		{
			AlignLocals(implementation);
		}
	}

	/// <summary>
	/// Aligns function variables
	/// </summary>
	/// <param name="function">Function to scan through</param>
	/// <param name="offset">Base offset to apply to all variables</param>
	private static void Align(FunctionImplementation function, int offset)
	{
		// Align all lambdas
		Align(function);

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			var standard_register_count = Calls.GetStandardParameterRegisters().Count();
			var media_register_count = Calls.GetMaxMediaRegisterParameters();

			if (standard_register_count == 0)
			{
				throw new ApplicationException("There were no standard registers reserved for parameter passage");
			}

			// The self pointer uses one standard register
			if (function.Variables.ContainsKey(Function.SELF_POINTER_IDENTIFIER) || function.Variables.ContainsKey(Lambda.SELF_POINTER_IDENTIFIER))
			{
				standard_register_count--;
			}

			var position = Assembler.IsArm64 ? 0 : Parser.Bytes;

			foreach (var parameter in function.Parameters)
			{
				if (!parameter.IsParameter) continue;

				var type = parameter.Type!;

				if (type.Format.IsDecimal() && media_register_count-- > 0 || !type.Format.IsDecimal() && standard_register_count-- > 0)
				{
					continue;
				}

				parameter.LocalAlignment = position;
				position += Parser.Bytes; // Stack elements each require the same amount of memory
			}
		}
		else
		{
			var position = offset * Parser.Bytes;

			// Align the this pointer if it exists
			if (function.Variables.TryGetValue(Function.SELF_POINTER_IDENTIFIER, out Variable? x))
			{
				x.LocalAlignment = position;
				position += Parser.Bytes;
			}
			else if (function.Variables.TryGetValue(Lambda.SELF_POINTER_IDENTIFIER, out Variable? y))
			{
				y.LocalAlignment = position - Parser.Bytes;
				position += Parser.Bytes;
			}

			// Parameters:
			foreach (var variable in function.Parameters)
			{
				if (variable.Category == VariableCategory.PARAMETER)
				{
					variable.LocalAlignment = position;
					position += Parser.Bytes; // Stack elements each require the same amount of memory
				}
			}
		}
	}
}