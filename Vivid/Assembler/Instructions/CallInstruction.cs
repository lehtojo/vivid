using System;
using System.Linq;
using System.Collections.Generic;

public class CallInstruction : Instruction
{
	private const string X64_INSTRUCTION = "call";
	private const string ARM64_CALL_LABEL = "bl";
	private const string ARM64_CALL_REGISTER = "blr";

	public Result Function { get; }
	public CallingConvention Convention { get; }
	public List<Instruction> ParameterInstructions { get; } = new List<Instruction>();

	private bool IsParameterInstructionListExtracted => IsBuilt;

	public CallInstruction(Unit unit, string function, CallingConvention convention, Type? return_type) : base(unit)
	{
		Function = new Result(new DataSectionHandle(function, true), Assembler.Format);
		Convention = convention;
		Result.Format = return_type?.Format ?? Assembler.Format;

		Description = "Calls function " + Function;
	}

	public CallInstruction(Unit unit, Result function, CallingConvention convention, Type? return_type) : base(unit)
	{
		Function = function;
		Convention = convention;
		Result.Format = return_type?.Format ?? Assembler.Format;

		Description = "Calls the function handle";
	}

	/// <summary>
	/// Iterates through the volatile registers and ensures that they don't contain any important values which are needed later
	/// </summary>
	private void ValidateEvacuation()
	{
		foreach (var register in Unit.VolatileRegisters)
		{
			if (register.IsAvailable(Position + 1))
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
		Unit.Append(new EvacuateInstruction(Unit, this));

		return registers;
	}

	public override void OnBuild()
	{
		var registers = ExecuteParameterInstructions();

		// Validate evacuation since it's very important to be correct
		ValidateEvacuation();
		
		Unit.Append(registers.Select(i => LockStateInstruction.Lock(Unit, i)).ToList());

		if (Assembler.IsArm64)
		{
			var is_address = Function.IsDataSectionHandle && Function.Value.To<DataSectionHandle>().Address;

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
			Build(
				X64_INSTRUCTION,
				new InstructionParameter(
					Function,
					ParameterFlag.BIT_LIMIT_64 | ParameterFlag.ALLOW_ADDRESS,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}

		Unit.Append(registers.Select(i => LockStateInstruction.Unlock(Unit, i)).ToList());

		// After a call all volatile registers might be changed
		Unit.VolatileRegisters.ForEach(r => r.Reset());

		// Returns value is always in the following handle
		var register = Result.Format.IsDecimal() ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();
		
		Result.Value = new RegisterHandle(register);
		register.Handle = Result;
	}

	public override Result GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.CALL;
	}

	public override Result[] GetResultReferences()
	{
		// If this call follows the x64 calling convention, the parameter instructions' source values must be referenced so that they aren't overriden before this call
		if (!IsParameterInstructionListExtracted && Convention == CallingConvention.X64)
		{
			return ParameterInstructions
				.Where(i => i.Type == InstructionType.MOVE)
				.Select(i => i.To<DualParameterInstruction>().Second)
				.Concat(new Result[] { Result, Function }).ToArray();
		}

		return new[] { Result, Function };
	}
}