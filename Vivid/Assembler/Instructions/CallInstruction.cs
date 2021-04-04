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

		Result.Format = return_type?.Format ?? Assembler.Format;
	}

	public CallInstruction(Unit unit, Result function, Type? return_type) : base(unit, InstructionType.CALL)
	{
		Function = function;
		Dependencies = null;
		Description = "Calls the function handle";

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

	public override void OnBuild()
	{
		var registers = ExecuteParameterInstructions();

		Unit.Append(registers.Select(i => LockStateInstruction.Lock(Unit, i)).ToList());

		if (Assembler.IsArm64)
		{
			var is_address = Function.IsDataSectionHandle && Function.Value.To<DataSectionHandle>().Address;

			if (!is_address)
			{
				Memory.MoveToRegister(Unit, Function, Assembler.Size, false, Trace.GetDirectives(Unit, Function));
			}

			Unit.Append(new EvacuateInstruction(Unit, this));

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
				ValidateHandle(Function.Value);
			}
			else if (!Function.IsStandardRegister)
			{
				Memory.MoveToRegister(Unit, Function, Assembler.Size, false, Trace.GetDirectives(Unit, Function));
			}

			Unit.Append(new EvacuateInstruction(Unit, this));

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

		// Validate evacuation since it is very important to be correct
		ValidateEvacuation();

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