using System;

/// <summary>
/// Updates the current value of the variable
/// This instruction works on all architectures
/// </summary>
public class SetVariableInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Value { get; private set; }

	public SetVariableInstruction(Unit unit, Variable variable, Result value) : base(unit, InstructionType.SET_VARIABLE)
	{
		if (!variable.IsPredictable) throw new ArgumentException("Setting value for unpredictable variables is not allowed");

		Variable = variable;
		Value = value;

		// If the variable has a previous value, hold it until this instruction is executed
		if (Unit.Scope!.Variables.TryGetValue(Variable, out Result? previous))
		{
			Dependencies = new[] { Result, Value, previous };
		}
		else
		{
			Dependencies = new[] { Result, Value };
		}
		
		Description = $"Updates the value of the variable '{variable.Name}'";
		IsAbstract = true;

		Result.Format = Variable.GetRegisterFormat();
		OnSimulate();
	}

	public override void OnSimulate()
	{
		// If the value does not represent another variable, it does not need to be copied
		if (!Unit.Scope!.Variables.ContainsValue(Value))
		{
			Unit.Scope!.Variables[Variable] = Value;
			return;
		}
		
		// Since the value represents another variable, the value has been copied to the result of this instruction
		Unit.Scope!.Variables[Variable] = Result;
	}

	public override void OnBuild() 
	{
		// Do not copy the value if it does not represent another variable
		if (!Unit.Scope!.Variables.ContainsValue(Value)) return;

		// Try to get the current location of the variable to be updated
		var current = Unit.GetVariableValue(Variable);

		// Use the location if it is available
		if (current != null)
		{
			Result.Value = current.Value;
			Result.Format = current.Format;
		}
		else
		{
			// Set the default values since the location is not available
			Result.Value = new Handle();
			Result.Format = Variable.GetRegisterFormat();
		}

		// Copy the value to the result of this instruction
		// NOTE: If the result is empty, the system will reserve a register
		Unit.Append(new MoveInstruction(Unit, Result, Value) { Type = MoveType.LOAD });
	}
}