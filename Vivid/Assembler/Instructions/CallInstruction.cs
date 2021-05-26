using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The instruction calls the specified value (for example function label or a register).
/// This instruction is works on all architectures
/// </summary>
public class CallInstruction : Instruction
{
	public Result Function { get; }
	public Type? ReturnType { get; private set; }
	
	// Represents the destination handles where the required parameters are passed to
	public List<Handle> Destinations { get; } = new List<Handle>();

	// This call is a tail call if it uses jump instruction
	public bool IsTailCall => Operation == Instructions.X64.JUMP || Operation == Instructions.Arm64.JUMP_LABEL || Operation == Instructions.Arm64.JUMP_REGISTER;
	
	public List<Result> Values { get; private set; } = new List<Result>();
	
	public CallInstruction(Unit unit, string function, Type? return_type) : base(unit, InstructionType.CALL)
	{
		Function = new Result(new DataSectionHandle(function, true), Assembler.Format);
		ReturnType = return_type;
		Dependencies = null;
		Description = "Calls function " + Function;
		IsUsageAnalyzed = false; // NOTE: Fixes an issue where the build system moves the function handle to volatile register even though it is needed later

		Result.Format = return_type?.Format ?? Assembler.Format;

		// Handle pack return types
		if (ReturnType == null || !ReturnType.IsPack) return;
		CreatePackValues(ReturnType);
		ReceivePackReturnType();
	}

	public CallInstruction(Unit unit, Result function, Type? return_type) : base(unit, InstructionType.CALL)
	{
		Function = function;
		ReturnType = return_type;
		Dependencies = null;
		Description = "Calls the function handle";
		IsUsageAnalyzed = false; // NOTE: Fixes an issue where the build system moves the function handle to volatile register even though it is needed later

		Result.Format = return_type?.Format ?? Assembler.Format;

		// Handle pack return types
		if (ReturnType == null || !ReturnType.IsPack) return;
		CreatePackValues(ReturnType);
		ReceivePackReturnType();
	}

	/// <summary>
	/// Iterates through the volatile registers and ensures that they don't contain any important values which are needed later
	/// </summary>
	private void ValidateEvacuation()
	{
		foreach (var register in Unit.VolatileRegisters)
		{
			/// NOTE: The availability of the register is not checked the standard way since they are usually locked at this stage
			if (register.Handle == null || !register.Handle.IsValid(Position + 1) || register.IsHandleCopy()) continue;
			throw new ApplicationException("Register evacuation failed");
		}
	}

	/// <summary>
	/// Prepares the memory handle for use by relocating its inner handles into registers, therefore its use does not require additional steps, except if it is in invalid format
	/// </summary>
	/// <returns>
	/// Returns a list of register locks which must be active while the handle is in use
	/// </returns>
	public List<RegisterLock> ValidateMemoryHandle(Handle handle)
	{
		var results = handle.GetRegisterDependentResults();
		var locks = new List<RegisterLock>();

		foreach (var result in results)
		{
			// 1. If the function handle lifetime extends over this instruction, all the inner handles must extend over this instruction as well, therefore a non-volatile register is needed
			// 2. If lifetime of a inner handle extends over this instruction, it needs a non-volatile register
			var non_volatile = Function.IsValid(Position + 1) || result.IsValid(Position + 1);

			if (result.IsStandardRegister && (!non_volatile || !result.Value.To<RegisterHandle>().Register.IsVolatile))
			{
				continue;
			}

			// Request an available register, which is volatile based on the lifetime of the function handle and its inner handles
			var register = non_volatile ? Unit.GetNextNonVolatileRegister(false, true) : Unit.GetNextRegister();

			// There should always be a register available, since the function call above can release values into memory
			if (register == null) throw new ApplicationException("Could not validate call handle");

			var destination = new RegisterHandle(register);

			Unit.Append(new MoveInstruction(Unit, new Result(destination, Assembler.Format), result)
			{
				Description = $"Evacuate an important value into '{destination}'",
				Type = MoveType.RELOCATE
			});

			locks.Add(RegisterLock.Create(result));
		}

		return locks;
	}

	/// <summary>
	/// Moves the specified result into a register
	/// </summary>
	/// <returns>
	/// Returns a list of register locks which must be active while the handle is in use
	/// </returns>
	public List<RegisterLock> MoveToRegister()
	{
		Memory.MoveToRegister(Unit, Function, Assembler.Size, false, Trace.GetDirectives(Unit, Function));
		return new List<RegisterLock> { RegisterLock.Create(Function) };
	}

	/// <summary>
	/// Generates the results which are used by the returned pack
	/// </summary>
	private void CreatePackValues(Type type)
	{
		foreach (var member in type.Variables.Values)
		{
			Values.Add(new Result());

			if (member.Type!.IsPack)
			{
				CreatePackValues(member.Type!);
			}
		}
	}

	/// <summary>
	/// Sets the return value to represent a pack type
	/// </summary>
	private void ReceivePackReturnType()
	{
		var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => Unit.Registers.Find(i => i[Size.QWORD] == name)!).ToList();
		var decimal_parameter_registers = Unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
		var position = new StackMemoryHandle(Unit, Assembler.IsTargetWindows ? Calls.SHADOW_SPACE_SIZE : 0, false);
		var handle = new DisposablePackHandle(new Dictionary<Variable, Result>());

		ReceivePackReturnType(handle, standard_parameter_registers, decimal_parameter_registers, position, ReturnType!, 0);

		Result.Value = handle;
		Result.Format = Assembler.Format;
	}

	/// <summary>
	/// Sets the return value to represent a pack type
	/// </summary>
	private int ReceivePackReturnType(DisposablePackHandle pack, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle position, Type type, int i)
	{
		var members = type.Variables.Values.ToArray();

		foreach (var member in members)
		{
			var result = Values[i];
			i++;

			pack.Variables[member] = result;

			if (member.Type!.IsPack)
			{
				var handle = new DisposablePackHandle(new Dictionary<Variable, Result>());
				result.Value = handle;
				
				i = ReceivePackReturnType(handle, standard_parameter_registers, decimal_parameter_registers, position, member.Type!, i);
				continue;
			}

			var register = member.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

			if (register != null)
			{
				result.Value = new RegisterHandle(register);
				register.Handle = result;
			}
			else
			{
				result.Value = position.Finalize();
				position.Offset += Assembler.Size.Bytes;
			}

			result.Format = member.GetRegisterFormat();
		}

		return i;
	}
	
	public override void OnSimulate()
	{
		// Handle pack return types
		if (ReturnType == null || !ReturnType.IsPack) return;
		ReceivePackReturnType();
	}

	public override void OnBuild()
	{
		var registers = Destinations.Where(i => i.Is(HandleType.REGISTER)).Select(i => i.To<RegisterHandle>().Register).ToArray();

		// Lock the parameter registers
		Unit.Append(registers.Select(i => LockStateInstruction.Lock(Unit, i)).ToList());

		var locks = new List<RegisterLock>();

		if (Assembler.IsArm64)
		{
			var is_address = Function.IsDataSectionHandle && Function.Value.To<DataSectionHandle>().Address;

			if (!is_address)
			{
				locks = MoveToRegister();
			}

			// Ensure the function handle is in the correct format
			if (Function.Format != Assembler.Format)
			{
				locks.ForEach(i => i.Dispose());
				locks = MoveToRegister();
			}

			// Now evacuate all the volatile registers before the call
			Unit.Append(new EvacuateInstruction(Unit, this));

			// If the format of the function handle changes, it means its format is registered incorrectly somewhere
			if (Function.Format != Assembler.Format) throw new ApplicationException("Invalid function handle format");

			Build(
				is_address ? Instructions.Arm64.CALL_LABEL : Instructions.Arm64.CALL_REGISTER,
				new InstructionParameter(
					Function,
					ParameterFlag.ALLOW_ADDRESS,
					is_address ? HandleType.MEMORY : HandleType.REGISTER
				)
			);
		}
		else
		{
			if (Function.IsMemoryAddress)
			{
				locks = ValidateMemoryHandle(Function.Value);
			}
			else if (!Function.IsStandardRegister)
			{
				locks = MoveToRegister();
			}

			// Ensure the function handle is in the correct format
			if (Function.Format != Assembler.Format)
			{
				locks.ForEach(i => i.Dispose());
				locks = MoveToRegister();
			}

			// Now evacuate all the volatile registers before the call
			Unit.Append(new EvacuateInstruction(Unit, this));

			// If the format of the function handle changes, it means its format is registered incorrectly somewhere
			if (Function.Format != Assembler.Format) throw new ApplicationException("Invalid function handle format");

			Build(
				Instructions.X64.CALL,
				new InstructionParameter(
					Function,
					ParameterFlag.BIT_LIMIT_64 | ParameterFlag.ALLOW_ADDRESS,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}

		locks.ForEach(i => i.Dispose());

		// Validate evacuation since it is very important to be correct
		ValidateEvacuation();

		// Unlock the parameter registers
		Unit.Append(registers.Select(i => LockStateInstruction.Unlock(Unit, i)).ToList());

		// After a call all volatile registers might be changed
		Unit.VolatileRegisters.ForEach(i => i.Reset());

		if (ReturnType != null && ReturnType.IsPack)
		{
			ReceivePackReturnType();
			return;
		}

		// Returns value is always in the following handle
		var register = Result.Format.IsDecimal() ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();

		Result.Value = new RegisterHandle(register);
		register.Handle = Result;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Function }.Concat(Values).ToArray();
	}
}