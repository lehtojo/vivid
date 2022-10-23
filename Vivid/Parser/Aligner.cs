using System.Collections.Generic;
using System.Linq;

public class ParameterAligner
{
	public int StandardRegisters { get; set; }
	public int DecimalRegisters { get; set; }
	public int Position { get; set; } = 0;

	public ParameterAligner(int position)
	{
		StandardRegisters = Calls.GetStandardParameterRegisterCount();
		DecimalRegisters = Calls.GetDecimalParameterRegisterCount();
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
			var proxies = Common.GetPackProxies(parameter);

			foreach (var proxy in proxies)
			{
				Align(proxy);
			}

			return;
		}

		// First, try to consume a register for the parameter
		if (type.Format.IsDecimal() && DecimalRegisters-- > 0 || !type.Format.IsDecimal() && StandardRegisters-- > 0)
		{
			// On Windows even though the first parameters are passed in registers, they still need have their own stack alignment (shadow space)
			if (!Settings.IsTargetWindows) return;
		}

		// Normal parameters consume one stack unit
		parameter.LocalAlignment = Position;
		Position += Parser.Bytes;
	}

	/// <summary>
	/// Aligns the specified parameters
	/// </summary>
	public void Align(List<Variable> parameters)
	{
		foreach (var parameter in parameters) { Align(parameter); }
	}
}

public static class Aligner
{
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
			AlignFunction(implementation);
		}
	}

	/// <summary>
	/// Align all used local packs and their proxies sequentially.
	/// Returns the stack position after aligning.
	/// NOTE: Available only in debugging mode, because in optimized builds pack proxies might not be available
	/// </summary>
	private static int AlignPacksForDebugging(FunctionImplementation context, List<Variable> variables, int position)
	{
		// Do nothing if debugging mode is not enabled
		if (!Settings.IsDebuggingEnabled) return position;

		foreach (var local in context.Locals.Concat(context.Parameters))
		{
			// Skip variables that are not packs
			if (!local.Type!.IsPack) continue;

			// Align the whole pack if it is used
			var proxies = Common.GetPackProxies(local);
			if (proxies.All(i => !variables.Contains(i))) continue;

			// Allocate stack memory for the whole pack
			position -= local.Type!.AllocationSize;
			local.LocalAlignment = position;

			// Keep track of the position inside the pack, so that we can align the members properly
			var subposition = position;

			// Align the pack proxies inside the allocated stack memory
			foreach (var proxy in proxies)
			{
				proxy.LocalAlignment = subposition;
				subposition += proxy.Type!.AllocationSize;

				// Remove the proxy from the variable list that will be aligned later
				variables.Remove(proxy);
			}
		}

		return position;
	}

	/// <summary>
	/// Align all used local variables and allocate memory for other kinds of local memory such as temporary handles and stack allocation handles
	/// </summary>
	public static void AlignLocalMemory(FunctionImplementation context, List<Variable> variables, List<TemporaryMemoryHandle> temporary_handles, List<StackAllocationHandle> inline_handles, int top)
	{
		var position = -top;

		position = AlignPacksForDebugging(context, variables, position);

		foreach (var variable in variables)
		{
			position -= variable.Type!.AllocationSize;
			variable.LocalAlignment = position;
		}

		while (temporary_handles.Count > 0)
		{
			var first = temporary_handles.First();
			var identifier = first.Identity;

			position -= first.Size.Bytes;

			var copies = temporary_handles.Where(i => i.Identity.Equals(identifier)).ToList();

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
			if (variable.IsStatic || variable.IsConstant) continue;
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
			AlignFunction(implementation);
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
	private static void AlignFunction(FunctionImplementation function)
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

		var aligner = new ParameterAligner(Settings.IsArm64 ? 0 : Parser.Bytes);
		aligner.Align(parameters);
	}
}