using System;

public class LoadOnlyIfConstantInstruction : Instruction
{
	private Variable Variable { get; }

	public LoadOnlyIfConstantInstruction(Unit unit, Variable variable) : base(unit)
	{
		Variable = variable;
		Result.Format = Variable.GetRegisterFormat();
	}

	public override void OnBuild()
	{
		var handle = new GetVariableInstruction(Unit, Variable, AccessMode.READ).Execute();

		if (handle == null)
		{
			throw new ApplicationException("Scope tried to edit an external variable which was not defined yet");
		}

		if (!handle.IsConstant)
		{
			return;
		}

		var recommendation = handle.GetRecommendation(Unit);
		var is_media_register = handle.Format.IsDecimal();

		Register? register = null;

		if (recommendation != null)
		{
			register = Memory.Consider(Unit, recommendation, is_media_register);
		}

		if (register == null)
		{
			register = is_media_register ? Unit.GetNextMediaRegisterWithoutReleasing() : Unit.GetNextRegisterWithoutReleasing();
		}

		Result.Value = register == null ? References.CreateVariableHandle(Unit, Variable) : new RegisterHandle(register);

		Unit.Append(new MoveInstruction(Unit, Result, handle)
		{
			Type = MoveType.RELOCATE
		});
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.LOAD_ONLY_IF_CONSTANT;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}