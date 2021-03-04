using System;
using System.Collections.Generic;
using System.Linq;

public static class Calls
{
	public const int SHADOW_SPACE_SIZE = 32;
	public const int STACK_ALIGNMENT = 16;

	private const int X64_MAX_MEDIA_REGISTERS_UNIX_X64 = 7;
	private const int X64_MAX_MEDIA_REGISTERS_WINDOWS_X64 = 4;

	private const int ARM64_MAX_MEDIA_REGISTERS_UNIX_X64 = 8;
	private const int ARM64_MAX_MEDIA_REGISTERS_WINDOWS_X64 = 8;

	private static readonly string[] X64_StandardParameterRegisters_Unix_X64 = { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
	private static readonly string[] X64_StandardParameterRegisters_Windows_X64 = { "rcx", "rdx", "r8", "r9" };

	private static readonly string[] ARM64_StandardParameterRegisters_Windows_X64 = { "x0", "x1", "x2", "x3", "x4", "x5", "x6", "x7" };
	private static readonly string[] ARM64_StandardParameterRegisters_Unix_X64 = { "x0", "x1", "x2", "x3", "x4", "x5", "x6", "x7" };

	public static IEnumerable<string> GetStandardParameterRegisters()
	{
		if (Assembler.IsArm64)
		{
			return Assembler.IsTargetWindows ? ARM64_StandardParameterRegisters_Windows_X64 : ARM64_StandardParameterRegisters_Unix_X64;
		}

		return Assembler.IsTargetWindows ? X64_StandardParameterRegisters_Windows_X64 : X64_StandardParameterRegisters_Unix_X64;
	}

	public static int GetMaxMediaRegisterParameters()
	{
		if (Assembler.IsArm64)
		{
			return Assembler.IsTargetWindows ? ARM64_MAX_MEDIA_REGISTERS_WINDOWS_X64 : ARM64_MAX_MEDIA_REGISTERS_UNIX_X64;
		}

		return Assembler.IsTargetWindows ? X64_MAX_MEDIA_REGISTERS_WINDOWS_X64 : X64_MAX_MEDIA_REGISTERS_UNIX_X64;
	}

	public static Result Build(Unit unit, FunctionNode node)
	{
		unit.TryAppendPosition(node);

		Result? self = null;

		if (IsSelfPointerRequired(unit.Function, node.Function))
		{
			var local_self_type = unit.Function.GetTypeParent()!;
			var function_self_type = node.Function.GetTypeParent()!;

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
		if (other.IsStatic || other.IsConstructor || !current.IsMember || !other.IsMember)
		{
			return false;
		}

		var x = current.GetTypeParent()!;
		var y = other.GetTypeParent()!;

		return x == y || x.IsSuperTypeDeclared(y);
	}

	/// <summary>
	/// Passes the specified parameters to the function using the specified calling convention
	/// </summary>
	/// <returns>Returns the amount of parameters moved to stack</returns>
	private static int PassParameters(Unit unit, CallInstruction call, Result? self_pointer, bool is_self_pointer_required, Node[] parameters, List<Type> parameter_types)
	{
		var stack_parameter_count = 0;

		var decimal_parameter_registers = unit.MediaRegisters.Take(GetMaxMediaRegisterParameters()).ToList();
		var standard_parameter_registers = GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

		// Retrieve the this pointer if it is required and it is not loaded
		if (self_pointer == null && is_self_pointer_required)
		{
			self_pointer = new GetVariableInstruction(unit, unit.Self!, AccessMode.READ).Execute();
		}

		var register = (Register?)null;

		// Save the parameter instructions for inspection
		call.Instructions.Clear();
		call.Destinations.Clear();

		// On Windows x64 a 'shadow space' is allocated for the first four parameters
		var stack_position = new StackMemoryHandle(unit, Assembler.IsTargetWindows ? SHADOW_SPACE_SIZE : 0, false);

		if (self_pointer != null)
		{
			register = standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new RegisterHandle(register);

				// Even though the destination should be the same size as the parameter, an exception should be made in case of registers since it is easier to manage when all register values can support every format
				call.Instructions.Add(new MoveInstruction(unit, new Result(destination, Assembler.Format), self_pointer) { IsSafe = true });
				call.Destinations.Add(destination);
			}
			else
			{
				// Since there is no more room for parameters in registers, this parameter must be pushed to stack
				call.Instructions.Add(new MoveInstruction(unit, new Result(stack_position, self_pointer.Format), self_pointer));
				call.Destinations.Add(stack_position.Finalize());

				stack_position.Offset += Size.FromFormat(self_pointer.Format).Bytes;
			}
		}

		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];

			var source = References.Get(unit, parameter);
			source = Casts.Cast(unit, source, parameter.GetType(), parameter_types[i]);

			var is_decimal = Equals(parameter_types[i], Types.DECIMAL);

			// Determine the parameter register
			register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new RegisterHandle(register);

				// Even though the destination should be the same size as the parameter, an exception should be made in case of registers since it is easier to manage when all register values can support every format
				var format = is_decimal ? Format.DECIMAL : Assembler.Size.ToFormat(source.Format.IsUnsigned());

				call.Instructions.Add(new MoveInstruction(unit, new Result(destination, format), source) { IsSafe = true });
				call.Destinations.Add(destination.Finalize());
			}
			else
			{
				var destination = new Result(stack_position.Finalize(), parameter_types[i].GetRegisterFormat());

				// Since there is no more room for parameters in registers, this parameter must be pushed to stack
				call.Instructions.Add(new MoveInstruction(unit, destination, source));
				call.Destinations.Add(stack_position.Finalize());

				stack_position.Offset += Size.FromFormat(source.Format).Bytes;
			}
		}

		return stack_parameter_count;
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

	private static Result Build(Unit unit, Result? self, Node? parameters, FunctionImplementation implementation)
	{
		if (self == null && IsSelfPointerRequired(unit.Function, implementation))
		{
			throw new InvalidOperationException("The self pointer was needed but not passed among the parameters");
		}

		var call = new CallInstruction(unit, implementation.GetFullname(), implementation.ReturnType);

		// Pass the parameters to the function and then execute it
		PassParameters(unit, call, self, false, CollectParameters(parameters), implementation.ParameterTypes);

		return call.Execute();
	}

	public static Result Build(Unit unit, Function function, Type return_type, params Node[] parameters)
	{
		var implementation = function.Implementations.FirstOrDefault() ?? throw new ApplicationException("Tried to create a function call but the function did not have any implementations");
		var call = new CallInstruction(unit, implementation.GetFullname(), return_type);

		// Pass the parameters to the function and then execute it
		PassParameters(unit, call, null, false, parameters, function.Parameters.Select(p => p.Type!).ToList());

		return call.Execute();
	}

	public static Result Build(Unit unit, Result? self, Result function, Type? return_type, Node parameters, List<Type> parameter_types)
	{
		var call = new CallInstruction(unit, function, return_type);

		// Pass the parameters to the function and then execute it
		PassParameters(unit, call, self, true, CollectParameters(parameters), parameter_types);

		return call.Execute();
	}

	public static void MoveParametersToStack(Unit unit)
	{
		var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
		var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

		var register = (Register?)null;

		if (unit.Function.IsMember || unit.Function.IsLambdaImplementation)
		{
			var self = unit.Self ?? throw new ApplicationException("Missing self pointer");

			register = standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new Result(References.CreateVariableHandle(unit, self), self.Type!.Format);
				var source = new Result(new RegisterHandle(register), self.GetRegisterFormat());

				unit.Append(new MoveInstruction(unit, destination, source)
				{
					Type = MoveType.RELOCATE
				});
			}
			else
			{
				throw new ApplicationException("Self pointer should not be in stack (x64 calling convention)");
			}
		}

		foreach (var parameter in unit.Function.Parameters)
		{
			register = parameter.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

			if (register != null)
			{
				var destination = new Result(References.CreateVariableHandle(unit, parameter), parameter.Type!.Format);
				var source = new Result(new RegisterHandle(register), parameter.GetRegisterFormat());

				unit.Append(new MoveInstruction(unit, destination, source)
				{
					Type = MoveType.RELOCATE
				});
			}
		}
	}
}