using System.Linq;
using System.Collections.Generic;
using System;

public static class Calls
{
	public const int SHADOW_SPACE_SIZE = 32;
	public const int STACK_ALIGNMENT = 16;

	private const int MAX_MEDIA_REGISTERS_UNIX_X64 = 7;
	private const int MAX_MEDIA_REGISTERS_WINDOWS_X64 = 4;

	private static readonly string[] StandardParameterRegistersUnixX64 = { "rdi", "rsi", "rdx", "rcx", "r8", "r9" };
	private static readonly string[] StandardParameterRegistersWindowsX64 = { "rcx", "rdx", "r8", "r9" };

	public static IEnumerable<string> GetStandardParameterRegisters()
	{
		return Assembler.IsTargetWindows ? StandardParameterRegistersWindowsX64 : StandardParameterRegistersUnixX64;
	}

	public static int GetMaxMediaRegisterParameters()
	{
		return Assembler.IsTargetWindows ? MAX_MEDIA_REGISTERS_WINDOWS_X64 : MAX_MEDIA_REGISTERS_UNIX_X64;
	}

	public static Result Build(Unit unit, FunctionNode node)
	{
		return Build(unit, null, node.Parameters, node.Function!);
	}

	public static Result Build(Unit unit, Result self, FunctionNode node)
	{
		return Build(unit, self, node.Parameters, node.Function!);
	}

	public static Result Build(Unit unit, Node? parameters, FunctionImplementation implementation)
	{
		return Build(unit, null, parameters, implementation);
	}

	private static bool IsThisPointerRequired(FunctionImplementation current, FunctionImplementation other)
	{
		return !other.IsConstructor && current.IsMember && other.IsMember && Equals(current.GetTypeParent(), other.GetTypeParent());
	}
	
	/// <summary>
	/// Passes the specified parameters to the function using the specified calling convention
	/// </summary>
	/// <returns>Returns the amount of parameters moved to stack</returns>
	private static int PassParameters(Unit unit, CallInstruction call, CallingConvention convention, Result? this_pointer, bool is_this_pointer_required, Node[] parameters)
	{
		var stack_parameter_count = 0;

		if (convention == CallingConvention.X64)
		{
			var decimal_parameter_registers = unit.MediaRegisters.Take(GetMaxMediaRegisterParameters()).ToList();
			var standard_parameter_registers = GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

			// Retrieve the this pointer if it's required and it's not loaded
			if (this_pointer == null && is_this_pointer_required)
			{
				this_pointer = new GetVariableInstruction(unit, null, unit.Self!, AccessMode.READ).Execute();
			}

			var register = (Register?)null;
			var instructions = new List<Instruction>();

			// On Windows x64 a 'shadow space' is allocated for the first four parameters
			var stack_position = new StackMemoryHandle(unit, Assembler.IsTargetWindows ? SHADOW_SPACE_SIZE : 0, false);

			if (this_pointer != null)
			{
				register = standard_parameter_registers.Pop();

				if (register != null)
				{
					var destination = new RegisterHandle(register);

					instructions.Add(new MoveInstruction(unit, new Result(destination, this_pointer.Format), this_pointer)
					{
						IsSafe = true
					});
				}
				else
				{
					// Since there's no more room for parameters in registers, this parameter must be pushed to stack
					instructions.Add(new MoveInstruction(unit, new Result(stack_position, this_pointer.Format), this_pointer));
					stack_position.Offset += Size.FromFormat(this_pointer.Format).Bytes;
				}
			}

			foreach (var parameter in parameters)
			{
				var source = References.Get(unit, parameter);
				var is_decimal = Equals(parameter.GetType(), Types.DECIMAL);

				// Determine the parameter register
				register = is_decimal ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

				if (register != null)
				{
					var destination = new RegisterHandle(register);

					instructions.Add(new MoveInstruction(unit, new Result(destination, source.Format), source)
					{
						IsSafe = true
					});
				}
				else
				{
					// Since there's no more room for parameters in registers, this parameter must be pushed to stack
					instructions.Add(new MoveInstruction(unit, new Result(stack_position.Finalize(), source.Format), source));
					stack_position.Offset += Size.FromFormat(source.Format).Bytes;
				}
			}
			
			// Save the parameter instructions for inspection
			call.ParameterInstructions = instructions;
		}
		else
		{
			var instructions = new List<Instruction>();

			for (var i = parameters.Length - 1; i >= 0; i--)
			{
				var parameter = parameters[i];
				var handle = References.Get(unit, parameter);

				var push = new PushInstruction(unit, handle);

				instructions.Add(push);
				unit.Append(push);
			}

			// Retrieve the this pointer if it's required and it's not loaded
			if (this_pointer == null && is_this_pointer_required)
			{
				this_pointer = new GetVariableInstruction(unit, null, unit.Self!, AccessMode.READ).Execute();
			}

			if (this_pointer != null)
			{
				var push = new PushInstruction(unit, this_pointer);

				instructions.Add(push);
				unit.Append(push);

				stack_parameter_count++;
			}

			instructions.ForEach(i => i.Execute());

			// Save the parameter instructions for inspection
			call.ParameterInstructions = instructions;
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
		var call = new CallInstruction(unit, implementation.Metadata!.GetFullname(), implementation.Convention, implementation.ReturnType);
		
		// Pass the parameters to the function and then execute it
		var is_this_pointer_required = IsThisPointerRequired(unit.Function, implementation);
		var stack_parameter_count = PassParameters(unit, call, implementation.Convention, self, is_this_pointer_required, CollectParameters(parameters));
		
		var result = call.Execute();
		
		// Remove the passed parameters from the stack
		StackMemoryInstruction.Shrink(unit, stack_parameter_count * Assembler.Size.Bytes, implementation.IsResponsible).Execute();

		return result;
	}

	public static Result Build(Unit unit, Function function, CallingConvention convention, Type return_type, params Node[] parameters)
	{
		var call = new CallInstruction(unit, function.GetFullname(), convention, return_type);
		
		// Pass the parameters to the function and then execute it
		var stack_parameter_count = PassParameters(unit, call, convention, null, false, parameters);

		var result = call.Execute();

		// Remove the passed parameters from the stack
		StackMemoryInstruction.Shrink(unit, stack_parameter_count * Assembler.Size.Bytes, function.IsResponsible).Execute();

		return result;
	}
}