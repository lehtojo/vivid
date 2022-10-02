using System;
using System.Collections.Generic;

/// <summary>
/// Updates the current value of the variable
/// This instruction works on all architectures
/// </summary>
public class SetVariableInstruction : Instruction
{
	public Variable Variable { get; set; }
	public Result Value { get; set; }
	public bool IsCopied { get; set; } = false;

	public SetVariableInstruction(Unit unit, Variable variable, Result value) : base(unit, InstructionType.SET_VARIABLE)
	{
		if (!variable.IsPredictable) throw new ApplicationException("Setting value for unpredictable variables is not allowed");

		this.Variable = variable;
		this.Value = value;
		this.Dependencies!.Add(value);
		this.Description = "Updates the value of the variable " + variable.Name;
		this.IsAbstract = true;
		this.Result.Format = variable.Type!.GetRegisterFormat();

		// If the value represents another variable or is a constant, it must be copied
		this.IsCopied = unit.IsVariableValue(value) || value.IsConstant;

		if (IsCopied)
		{
			unit.SetVariableValue(variable, Result);
			return;
		}

		unit.SetVariableValue(variable, value);
	}

	public void CopyValue()
	{
		var format = Variable.Type!.GetRegisterFormat();
		var register = Memory.GetNextRegisterWithoutReleasing(Unit, format.IsDecimal(), Trace.For(Unit, Result));

		if (register != null)
		{
			Result.Value = new RegisterHandle(register);
			Result.Format = format;

			// Attach the result to the register
			register.Value = Result;
		}
		else {
			Result.Value = References.CreateVariableHandle(Unit, Variable);
			Result.Format = Variable.Type!.Format;
		}

		// Copy the value to the result of this instruction
		var instruction = new MoveInstruction(Unit, Result, Value);
		instruction.Type = MoveType.LOAD;
		Unit.Add(instruction);
	}

	public override void OnBuild()
	{
		// If the value represents another variable or is a constant, it must be copied
		if (IsCopied)
		{
			CopyValue();

			Unit.SetVariableValue(Variable, Result);
			return;
		}

		Unit.SetVariableValue(Variable, Value);
	}
}