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
	/// Passes the specified disposable pack by passing its member one by one
	/// </summary>
	public static void PassPack(Unit unit, List<Handle> destinations, List<Result> sources, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle position, DisposablePackHandle pack)
	{
		foreach (var iterator in pack.Members)
		{
			var member = iterator.Key;
			var value = iterator.Value;

			if (member.Type!.IsPack)
			{
				PassPack(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, value.Value.To<DisposablePackHandle>());
			}
			else
			{
				PassArgument(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, iterator.Value, member.Type!, member.GetRegisterFormat());
			}
		}
	}

	/// <summary>
	/// Passes the specified argument using a register or the specified stack position depending on the situation
	/// </summary>
	public static void PassArgument(Unit unit, List<Handle> destinations, List<Result> sources, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle position, Result value, Type type, Format format)
	{
		if (value.Value.Instance == HandleInstanceType.DISPOSABLE_PACK)
		{
			PassPack(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, value.Value.To<DisposablePackHandle>());
			return;
		}

		// Determine the parameter register
		var is_decimal = format.IsDecimal();
		var register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

		if (register != null)
		{
			// Even though the destination should be the same size as the parameter, an exception should be made in case of registers since it is easier to manage when all register values can support every format
			var destination = new RegisterHandle(register);
			destination.Format = is_decimal ? Format.DECIMAL : Assembler.Size.ToFormat(type.Format.IsUnsigned());

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
			PassArgument(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, self_pointer, self_type!, Assembler.Format);
		}

		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			var value = References.Get(unit, parameters[i]);
			var type = parameter_types[i];

			value = Casts.Cast(unit, value, parameter.GetType(), type);
			PassArgument(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, value, type, type.GetRegisterFormat());
		}

		call.Destinations.AddRange(destinations);
		unit.Append(new ReorderInstruction(unit, destinations, sources, call.ReturnType));
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

	private static void MovePackToStack(Unit unit, Variable parameter, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle stack_position)
	{
		foreach (var representive in Common.GetPackRepresentives(parameter))
		{
			// Do not use the default parameter alignment, use local stack memory, because we want the pack members to be sequentially
			representive.LocalAlignment = null;

			var is_decimal = representive.Type!.Format.IsDecimal();
			var register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();
			var source = (Result?)null;

			if (register != null)
			{
				source = new Result(new RegisterHandle(register), representive.GetRegisterFormat());
			}
			else
			{
				source = new Result(stack_position.Finalize(), representive.Type!.Format);
			}

			var destination = new Result(References.CreateVariableHandle(unit, representive), representive.Type!.Format);
			unit.Append(new MoveInstruction(unit, destination, source) { Type = MoveType.RELOCATE });

			// Windows: Even though the first parameters are passed in registers, they still require their own stack memory (shadow space)
			if (register != null && !Assembler.IsTargetWindows) continue;

			// Normal parameters consume one stack unit
			stack_position.Offset += Parser.Bytes;
		}
	}

	/// <summary>
	/// Moves the specified parameter or its representives to their own stack locations, if they are not already in the stack.
	/// The location of the parameter is determined by using the specified registers.
	/// This is used for debugging purposes.
	/// </summary>
	public static void MoveParameterToStack(Unit unit, Variable parameter, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle stack_position)
	{
		if (parameter.Type!.IsPack)
		{
			MovePackToStack(unit, parameter, standard_parameter_registers, decimal_parameter_registers, stack_position);
			return;
		}

		var is_decimal = parameter.Type!.Format.IsDecimal();
		var register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

		if (register != null)
		{
			var destination = new Result(References.CreateVariableHandle(unit, parameter), parameter.Type!.Format);
			var source = new Result(new RegisterHandle(register), parameter.GetRegisterFormat());

			unit.Append(new MoveInstruction(unit, destination, source) { Type = MoveType.RELOCATE });

			// Windows: Even though the first parameters are passed in registers, they still need their own stack memory (shadow space)
			if (!Assembler.IsTargetWindows) return;
		}

		// Normal parameters consume one stack unit
		stack_position.Offset += Parser.Bytes;
	}

	/// <summary>
	/// Moves the specified parameters or their representives to their own stack locations, if they are not already in the stack.
	/// This is used for debugging purposes.
	/// </summary>
	public static void MoveParametersToStack(Unit unit)
	{
		var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
		var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(i => i[Size.QWORD] == name)!).ToList();
		var stack_position = new StackMemoryHandle(unit, Assembler.IsArm64 ? 0 : Parser.Bytes);

		var parameters = new List<Variable>(unit.Function.Parameters);

		if ((unit.Function.IsMember && !unit.Function.IsStatic) || unit.Function.IsLambdaImplementation)
		{
			parameters.Insert(0, unit.Self ?? throw new ApplicationException("Missing self pointer"));
		}

		foreach (var parameter in parameters)
		{
			MoveParameterToStack(unit, parameter, standard_parameter_registers, decimal_parameter_registers, stack_position);
		}
	}
}