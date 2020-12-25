using System;

/// <summary>
/// Updates the current value of the variable
/// This instruction works on all architectures
/// </summary>
public class SetVariableInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Value { get; private set; }

	public SetVariableInstruction(Unit unit, Variable variable, Result value) : base(unit)
	{
		if (!variable.IsPredictable)
		{
			throw new ArgumentException("Tried to use Set-Variable-Instruction to update the value of an unpredictable variable");
		}

		Variable = variable;
		Value = value;
		Description = $"Updates the value of the variable '{variable.Name}'";

		Result.Format = Value.Format;
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Set-Variable-Instruction");
	}

	public override void OnSimulate()
	{
		Unit.Scope!.Variables[Variable] = Value;
	}

	public override void OnBuild()
	{
		if (Unit.Scope!.Variables.TryGetValue(Variable, out Result? current) && current.IsAnyRegister)
		{
			Unit.Append(new MoveInstruction(Unit, current, Value)
			{
				Type = MoveType.RELOCATE
			});
		}
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SET_VARIABLE;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Value };
	}
}