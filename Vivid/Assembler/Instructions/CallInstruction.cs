using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// The instruction calls the specified value (for example function label or a register).
/// This instruction is works on all architectures
/// </summary>
public class CallInstruction : Instruction
{
	private const string X64_INSTRUCTION = "call";
	private const string ARM64_CALL_LABEL = "bl";
	private const string ARM64_CALL_REGISTER = "blr";

	public Result Function { get; }
	public List<Instruction> ParameterInstructions { get; } = new List<Instruction>();

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

	private List<Register> ExecuteParameterInstructions()
	{
		var moves = ParameterInstructions.Select(i => i.To<MoveInstruction>()).ToList();

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
				is_address ? ARM64_CALL_LABEL : ARM64_CALL_REGISTER,
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
				X64_INSTRUCTION,
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
		// If this call follows the x64 calling convention, the parameter instructions' source values must be referenced so that they aren't overriden before this call
		if (!IsParameterInstructionListExtracted)
		{
			return ParameterInstructions
				.Where(i => i.Type == InstructionType.MOVE)
				.Select(i => i.To<DualParameterInstruction>().Second)
				.Concat(new Result[] { Result, Function }).ToArray();
		}

		return new[] { Result, Function };
	}
}