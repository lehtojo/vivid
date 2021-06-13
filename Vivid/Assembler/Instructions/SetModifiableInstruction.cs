using System;

/// <summary>
/// Loads the specified variable into a modifiable location if it is constant
/// This instruction works on all architectures
/// </summary>
public class SetModifiableInstruction : Instruction
{
	private Variable Variable { get; }

	public SetModifiableInstruction(Unit unit, Variable variable) : base(unit, InstructionType.SET_MODIFIABLE)
	{
		Variable = variable;
		Description = $"Loads the variable '{variable.Name}' into a register or memory if it is a constant";
		IsAbstract = true;

		Result.Format = Variable.GetRegisterFormat();
	}

	public override void OnBuild()
	{
		var handle = Unit.GetVariableValue(Variable);
		if (handle == null || !handle.IsConstant) return;

		var directives = Trace.GetDirectives(Unit, handle);
		var is_media_register = handle.Format.IsDecimal();

		var register = Memory.Consider(Unit, directives, is_media_register);

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
}