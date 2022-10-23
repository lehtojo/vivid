using System.Collections.Generic;
using System;

public class LabelMergeInstruction : Instruction
{
	public string Primary { get; set; }
	public string? Secondary { get; set; }

	public LabelMergeInstruction(Unit unit, string primary) : base(unit, InstructionType.LABEL_MERGE)
	{
		this.Primary = primary;
		this.Secondary = null;
		this.IsAbstract = true;
	}

	public LabelMergeInstruction(Unit unit, string primary, string secondary) : base(unit, InstructionType.LABEL_MERGE)
	{
		this.Primary = primary;
		this.Secondary = secondary;
		this.IsAbstract = true;
	}

	public void PrepareFor(string? id)
	{
		if (id == null) return;

		var variables = Unit.Scopes[id].Inputs.Keys;

		foreach (var variable in variables)
		{
			// Packs variables are not merged, their members are instead
			if (variable.Type!.IsPack) continue;

			// Get the current value of the variable
			var result = Unit.GetVariableValue(variable);

			if (result == null || !result.IsActive()) throw new ApplicationException("Output variable was not active");

			// If the result represents a register, verify it owns it
			if (result.IsAnyRegister && !ReferenceEquals(result.Value.To<RegisterHandle>().Register.Value, result))
			{
				throw new ApplicationException("Output variable did not own the register");
			}

			// Load complex values into registers
			var instance = result.Value.Instance;
			var allowed = HandleInstanceType.REGISTER | HandleInstanceType.STACK_MEMORY | HandleInstanceType.STACK_VARIABLE | HandleInstanceType.TEMPORARY_MEMORY;

			if ((instance & allowed) == 0)
			{
				Memory.MoveToRegister(Unit, result, Settings.Size, result.Format.IsDecimal(), Trace.For(Unit, result));
			}
		}
	}

	public void RegisterState(string? id)
	{
		if (id == null) return;

		var variables = Unit.Scopes[id].Inputs.Keys;

		// Save the locations of the processed variables as a state
		var state = new List<VariableState>();

		foreach (var variable in variables)
		{
			// Packs variables are not merged, their members are instead
			if (variable.Type!.IsPack) continue;

			state.Add(new VariableState(variable, Unit.GetVariableValue(variable) ?? throw new ApplicationException("Missing variable value")));
		}

		Unit.States[id] = state;
	}

	public void MergeWithState(List<VariableState> state)
	{
		// Collect the destination handles and the current values of the corresponding variables
		var destinations = new List<Handle>();
		var sources = new List<Result>();

		foreach (var descriptor in state)
		{
			var source = Unit.GetVariableValue(descriptor.Variable);
			if (source == null) continue;

			destinations.Add(descriptor.Handle);
			sources.Add(source);
		}

		// Relocate the sources so that they match the destinations
		Unit.Add(new ReorderInstruction(Unit, destinations, sources, null));

		// Update the sources manually
		for (var i = 0; i < destinations.Count; i++)
		{
			var destination = destinations[i];
			var source = sources[i];

			source.Value = destination;
			source.Format = destination.Format;

			// If the destination is a register, attach the source to it
			if (destination.Instance == HandleInstanceType.REGISTER) { destination.To<RegisterHandle>().Register.Value = source; }
		}
	}

	public override void OnBuild()
	{
		// If the unit does not have a registered state for the label, then we must make one
		if (!Unit.States.ContainsKey(Primary))
		{
			PrepareFor(Primary);
			PrepareFor(Secondary);

			RegisterState(Primary);
			RegisterState(Secondary);
			return;
		}

		PrepareFor(Secondary);
		MergeWithState(Unit.States[Primary]);
		RegisterState(Secondary);
	}
}