using System;
using System.Collections.Generic;
using System.Linq;

public static class Calls
{
	public const int SHADOW_SPACE_SIZE = 32;
	public const int STACK_ALIGNMENT = 16;

	private const int UNIX_X64_MEDIA_REGISTER_PARAMETERS = 7;
	private const int WINDOWS_X64_MEDIA_REGISTER_PARAMETERS = 4;

	private const int UNIX_ARM64_MEDIA_REGISTER_PARAMETERS = 8;
	private const int WINDOWS_ARM64_MEDIA_REGISTER_PARAMETERS = 8;

	private static readonly string[] UNIX_X64_STANDARD_PARAMETER_REGISTERS = { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
	private static readonly string[] WINDOWS_X64_STANDARD_PARAMETER_REGISTERS = { "rcx", "rdx", "r8", "r9" };

	private static readonly string[] WINDOWS_ARM64_STANDARD_PARAMETER_REGISTERS = { "x0", "x1", "x2", "x3", "x4", "x5", "x6", "x7" };
	private static readonly string[] UNIX_ARM64_STANDARD_PARAMETER_REGISTERS = { "x0", "x1", "x2", "x3", "x4", "x5", "x6", "x7" };

	public static IEnumerable<string> GetStandardParameterRegisters()
	{
		if (Assembler.IsArm64)
		{
			return Assembler.IsTargetWindows ? WINDOWS_ARM64_STANDARD_PARAMETER_REGISTERS : UNIX_ARM64_STANDARD_PARAMETER_REGISTERS;
		}

		return Assembler.IsTargetWindows ? WINDOWS_X64_STANDARD_PARAMETER_REGISTERS : UNIX_X64_STANDARD_PARAMETER_REGISTERS;
	}

	public static int GetMaxMediaRegisterParameters()
	{
		if (Assembler.IsArm64)
		{
			return Assembler.IsTargetWindows ? WINDOWS_ARM64_MEDIA_REGISTER_PARAMETERS : UNIX_ARM64_MEDIA_REGISTER_PARAMETERS;
		}

		return Assembler.IsTargetWindows ? WINDOWS_X64_MEDIA_REGISTER_PARAMETERS : UNIX_X64_MEDIA_REGISTER_PARAMETERS;
	}

	public static Result Build(Unit unit, FunctionNode node)
	{
		unit.TryAppendPosition(node);

		Result? self = null;

		if (IsSelfPointerRequired(unit.Function, node.Function))
		{
			var local_self_type = unit.Function.FindTypeParent()!;
			var function_self_type = node.Function.FindTypeParent()!;

			self = References.GetVariable(unit, unit.Self!, AccessMode.READ);

			// If the function is not defined inside the type of the self pointer, it means it must have been defined in its supertypes, therefore casting is needed
			if (local_self_type != function_self_type)
			{
				self = Casts.Cast(unit, self, local_self_type, function_self_type);
			}
		}

		return Build(unit, self, node.Parameters, node.Function!);
	}

	public static Result Build(Unit unit, Result self, FunctionNode node)
	{
		unit.TryAppendPosition(node);
		return Build(unit, self, node.Parameters, node.Function!);
	}

	private static bool IsSelfPointerRequired(FunctionImplementation current, FunctionImplementation other)
	{
		if (other.IsStatic || other.IsConstructor || !current.IsMember || current.IsStatic || !other.IsMember || other.IsStatic)
		{
			return false;
		}

		var x = current.FindTypeParent()!;
		var y = other.FindTypeParent()!;

		return x == y || x.IsSuperTypeDeclared(y);
	}

	/// <summary>
	/// Passes the specified argument using a register or the specified stack position depending on the situation
	/// </summary>
	private static void PassArgument(List<Handle> destinations, List<Result> sources, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle position, Result value, Format format)
	{
		// Determine the parameter register
		var is_decimal = format.IsDecimal();
		var register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

		if (register != null)
		{
			// Even though the destination should be the same size as the parameter, an exception should be made in case of registers since it is easier to manage when all register values can support every format
			var destination = new RegisterHandle(register);
			destination.Format = is_decimal ? Format.DECIMAL : Assembler.Size.ToFormat(value.Format.IsUnsigned());

			destinations.Add(destination);
		}
		else
		{
			// Since there is no more room for parameters in registers, this parameter must be pushed to stack
			position.Format = format;
			destinations.Add(position.Finalize());

			position.Offset += Assembler.Size.Bytes;
		}

		sources.Add(value);
	}

	/// <summary>
	/// Passes the specified parameters to the function using the specified calling convention
	/// </summary>
	/// <returns>Returns the amount of parameters moved to stack</returns>
	private static void PassArguments(Unit unit, CallInstruction call, Result? self_pointer, Type? self_type, bool is_self_pointer_required, Node[] parameters, List<Type> parameter_types)
	{
		var standard_parameter_registers = GetStandardParameterRegisters().Select(name => unit.Registers.Find(i => i[Size.QWORD] == name)!).ToList();
		var decimal_parameter_registers = unit.MediaRegisters.Take(GetMaxMediaRegisterParameters()).ToList();

		// Retrieve the this pointer if it is required and it is not loaded
		if (self_pointer == null && is_self_pointer_required)
		{
			self_pointer = References.GetVariable(unit, unit.Self!, AccessMode.READ);
		}

		var destinations = new List<Handle>();
		var sources = new List<Result>();

		// On Windows x64 a 'shadow space' is allocated for the first four parameters
		var position = new StackMemoryHandle(unit, Assembler.IsTargetWindows ? SHADOW_SPACE_SIZE : 0, false);

		if (self_pointer != null)
		{
			if (self_type == null) throw new InvalidOperationException("Missing self pointer type");

			PassArgument(destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, self_pointer, Assembler.Format);
		}

		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			var value = References.Get(unit, parameters[i]);
			var type = parameter_types[i];

			value = Casts.Cast(unit, value, parameter.GetType(), type);
			PassArgument(destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, value, type.GetRegisterFormat());
		}

		call.Destinations.AddRange(destinations);
		unit.Append(new ReorderInstruction(unit, destinations, sources));
	}

	/// <summary>
	/// Collects all parameters from the specified node tree into an array
	/// </summary>
	private static Node[] CollectParameters(Node? parameters)
	{
		if (parameters == null)
		{
			return Array.Empty<Node>();
		}

		var result = new List<Node>();
		var parameter = parameters.First;

		while (parameter != null)
		{
			result.Add(parameter);
			parameter = parameter.Next;
		}

		return result.ToArray();
	}

	public static Result Build(Unit unit, Result? self, Node? parameters, FunctionImplementation implementation)
	{
		if (self == null && IsSelfPointerRequired(unit.Function, implementation))
		{
			throw new InvalidOperationException("The self pointer was needed but not passed among the parameters");
		}

		var call = new CallInstruction(unit, implementation.GetFullname(), implementation.ReturnType);
		var self_type = self == null ? null : implementation.FindTypeParent();

		// Pass the parameters to the function and then execute it
		PassArguments(unit, call, self, self_type, false, CollectParameters(parameters), implementation.ParameterTypes);

		return call.Execute();
	}

	public static Result Build(Unit unit, Result? self, Type? self_type, Result function, Type? return_type, Node parameters, List<Type> parameter_types)
	{
		var call = new CallInstruction(unit, function, return_type);

		// Pass the parameters to the function and then execute it
		PassArguments(unit, call, self, self_type, true, CollectParameters(parameters), parameter_types);

		return call.Execute();
	}

	public static void MoveParametersToStack(Unit unit)
	{
		var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
		var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

		var register = (Register?)null;

		if ((unit.Function.IsMember && !unit.Function.IsStatic) || unit.Function.IsLambdaImplementation)
		{
			var self = unit.Self ?? throw new ApplicationException("Missing self pointer");

			register = standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new Result(References.CreateVariableHandle(unit, self), self.Type!.Format);
				var source = new Result(new RegisterHandle(register), self.GetRegisterFormat());

				unit.Append(new MoveInstruction(unit, destination, source) { Type = MoveType.RELOCATE });
			}
			else
			{
				throw new ApplicationException("Self pointer should not be in stack");
			}
		}

		foreach (var parameter in unit.Function.Parameters)
		{
			register = parameter.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new Result(References.CreateVariableHandle(unit, parameter), parameter.Type!.Format);
				var source = new Result(new RegisterHandle(register), parameter.GetRegisterFormat());

				unit.Append(new MoveInstruction(unit, destination, source) { Type = MoveType.RELOCATE });
			}
		}
	}
}