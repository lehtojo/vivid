using System;
using System.Collections.Generic;

public class Scope
{
	public const string ENTRY = ".entry";

	public Unit Unit { get; set; }
	public string Id { get; set; }
	public int Index { get; set; }

	public Dictionary<Variable, Result> Variables { get; set; } = new();
	public Dictionary<Variable, Result> Inputs { get; set; } = new();
	public Dictionary<Variable, bool> Outputs { get; set; } = new();

	public RequireVariablesInstruction Inputter;
	public RequireVariablesInstruction Outputter;

	public Scope(Unit unit, string id)
	{
		this.Unit = unit;
		this.Id = id;
		this.Index = unit.Scopes.Count;
		this.Inputter = new RequireVariablesInstruction(unit, true);
		this.Outputter = new RequireVariablesInstruction(unit, false);

		// Register this scope
		unit.Scopes[id] = this;

		Enter();

		unit.Add(Inputter);
	}

	public Result SetOrCreateInput(Variable variable, Handle handle, Format format)
	{
		if (!variable.IsPredictable) throw new ApplicationException("Unpredictable variable can not be an input");

		handle = handle.Finalize();
		var input = (Result?)null;

		if (Inputs.ContainsKey(variable))
		{
			input = Inputs[variable];
			input.Value = handle;
			input.Format = format;
		}
		else
		{
			input = new Result(handle, format);
			Inputs.Add(variable, input);
		}

		// Update the current handle to the variable
		Variables[variable] = input;

		// If the input is a register, the input value must be attached there
		if (input.Value.Instance == HandleInstanceType.REGISTER)
		{
			input.Value.To<RegisterHandle>().Register.Value = input;
		}

		return input;
	}

	// Summary: Assigns a register or a stack address for the specified parameter depending on the situation
	public void ReceiveParameter(List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, Variable parameter)
	{
		if (parameter.Type!.IsPack)
		{
			var proxies = Common.GetPackProxies(parameter);

			foreach (var proxy in proxies)
			{
				ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, proxy);
			}

			return;
		}

		var register = (Register?)null;

		if (parameter.Type.Format == Format.DECIMAL)
		{
			if (decimal_parameter_registers.Count > 0) { register = decimal_parameter_registers.Pop(); }
		}
		else
		{
			if (standard_parameter_registers.Count > 0) { register = standard_parameter_registers.Pop(); }
		}

		AddInput(parameter);

		if (register != null)
		{
			register.Value = SetOrCreateInput(parameter, new RegisterHandle(register), parameter.Type.GetRegisterFormat());
		}
		else
		{
			SetOrCreateInput(parameter, References.CreateVariableHandle(Unit, parameter), parameter.Type.GetRegisterFormat());
		}
	}

	public Result AddInput(Variable variable)
	{
		// If the variable is already in the input list, do nothing
		if (Inputs.ContainsKey(variable)) return Inputs[variable];

		// Create a placeholder handle for the variable
		var handle = new Handle();
		handle.Format = variable.Type!.GetRegisterFormat();

		var input = SetOrCreateInput(variable, new Handle(), handle.Format);
		Inputter.Dependencies!.Add(input);
		input.Lifetime.Usages.Add(Inputter);

		return input;
	}

	public void AddOutput(Variable variable, Result value)
	{
		// If the variable is already in the output list, do nothing
		if (Outputs.ContainsKey(variable)) return;

		Outputter.Dependencies!.Add(value);
		value.Lifetime.Usages.Add(Outputter);

		// Register the variable as an output
		Outputs[variable] = true;
	}

	public void Enter()
	{
		// Exit the current scope before entering the new one
		if (Unit.Scope != null) Unit.Scope.Exit();

		// Reset variable data
		Variables.Clear();

		// Switch the current unit scope to be this scope
		Unit.Scope = this;

		// Reset all registers
		foreach (var register in Unit.NonReservedRegisters)
		{
			register.Reset();
		}

		// Set the inputs as initial values for the corresponding variables
		foreach (var input in Inputs)
		{
			var variable = input.Key;
			var result = input.Value;

			// Reset the input value
			result.Value = new Handle();
			result.Format = variable.Type!.GetRegisterFormat();

			Variables[variable] = result;
		}

		if (Id == ENTRY && !Assembler.IsDebuggingEnabled)
		{
			// Move all parameters to their expected registers since this is the first scope
			var standard_parameter_registers = Calls.GetStandardParameterRegisters(Unit);
			var decimal_parameter_registers = Calls.GetDecimalParameterRegisters(Unit);

			if ((Unit.Function.IsMember && !Unit.Function.IsStatic) || Unit.Function.IsLambdaImplementation)
			{
				ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, Unit.Self!);
			}

			foreach (var parameter in Unit.Function.Parameters)
			{
				ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, parameter);
			}
		}
	}

	public void Exit()
	{
		if (Unit.Mode == UnitMode.ADD)
		{
			Unit.Add(Outputter);
		}
	}
}