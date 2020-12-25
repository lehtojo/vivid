using System;

/// <summary>
/// Loads the specified variable into a modifiable location if it's constant
/// This instruction works on all architectures
/// </summary>
public class SetModifiableInstruction : Instruction
{
	private Variable Variable { get; }

	public SetModifiableInstruction(Unit unit, Variable variable) : base(unit)
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
		return InstructionType.SET_MODIFIABLE;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}