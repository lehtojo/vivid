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

	// Represents the instructions which pass the required parameters
	public List<Instruction> Instructions { get; } = new List<Instruction>();
	
	// Represents the destinations where the required parameters are passed to
	public List<Handle> Destinations { get; } = new List<Handle>();

	// This call is a tail call if it uses jump instruction
	public bool IsTailCall => Operation == global::Instructions.X64.JUMP || Operation == global::Instructions.Arm64.JUMP_LABEL || Operation == global::Instructions.Arm64.JUMP_REGISTER;

	private bool IsParameterInstructionListExtracted => IsBuilt;

	public CallInstruction(Unit unit, string function, Type? return_type) : base(unit, InstructionType.CALL)
	{
		Function = new Result(new DataSectionHandle(function, true), Assembler.Format);
		Dependencies = null;
		Description = "Calls function " + Function;
		IsUsageAnalyzed = false; // NOTE: Fixes an issue where the build system moves the function handle to volatile register even though it is needed later

		Result.Format = return_type?.Format ?? Assembler.Format;
	}

	public CallInstruction(Unit unit, Result function, Type? return_type) : base(unit, InstructionType.CALL)
	{
		Function = function;
		Dependencies = null;
		Description = "Calls the function handle";
		IsUsageAnalyzed = false; // NOTE: Fixes an issue where the build system moves the function handle to volatile register even though it is needed later

		Result.Format = return_type?.Format ?? Assembler.Format;
	}

	/// <summary>
	/// Iterates through the volatile registers and ensures that they don't contain any important values which are needed later
	/// </summary>
	private void ValidateEvacuation()
	{
		foreach (var register in Unit.VolatileRegisters)
		{
			/// NOTE: The availability of the register is not checked the standard way since they are usually locked at this stage
			if (register.Handle == null || !register.Handle.IsValid(Position + 1) || register.IsHandleCopy())
			{
				continue;
			}

			throw new ApplicationException("Register evacuation failed");
		}
	}

	/// <summary>
	/// Unpacks the parameter instructions by executing them
	/// </summary>
	private List<Register> ExecuteParameterInstructions()
	{
		var moves = Instructions.Select(i => i.To<MoveInstruction>()).ToList();

		Unit.Append(Memory.Align(Unit, moves, out List<Register> registers));

		return registers;
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

	public override void OnBuild()
	{
		var registers = ExecuteParameterInstructions();

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
				is_address ? global::Instructions.Arm64.CALL_LABEL : global::Instructions.Arm64.CALL_REGISTER,
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
				global::Instructions.X64.CALL,
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
		Unit.VolatileRegisters.ForEach(r => r.Reset());

		// Returns value is always in the following handle
		var register = Result.Format.IsDecimal() ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();

		Result.Value = new RegisterHandle(register);
		register.Handle = Result;
	}

	public override Result[] GetResultReferences()
	{
		// The source values of the parameter instructions must be referenced so that they are not overriden before this call
		if (!IsParameterInstructionListExtracted)
		{
			return Instructions
				.Where(i => i.Is(InstructionType.MOVE))
				.Select(i => i.To<DualParameterInstruction>().Second)
				.Concat(new[] { Result, Function }).ToArray();
		}

		return new[] { Result, Function };
	}
}