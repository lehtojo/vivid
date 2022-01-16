using System.Collections.Generic;
using System.Linq;
using System;

public class LabelMergeInstruction : Instruction
{
	public Label Label { get; set; }
	public List<Variable> Actives { get;}

	public LabelMergeInstruction(Unit unit, Label label, List<Variable> actives) : base(unit, InstructionType.LABEL_MERGE)
	{
		Label = label;
		Actives = actives;
		IsAbstract = true;
	}

	public override void OnBuild()
	{
		if (!Unit.States.ContainsKey(Label))
		{
			var variables = new List<Variable>();

			foreach (var variable in Scope!.Variables.Keys)
			{
				var result = Unit.GetVariableValue(variable) ?? throw new ApplicationException("Missing active variable value");
				
				// Ensure the variable is still active
				if (!result.IsValid(Unit.Position)) continue; 
				
				// Ensure the variable is still valid
				if (result.IsAnyRegister && !ReferenceEquals(result.Value.To<RegisterHandle>().Register.Handle, result)) continue;
				
				// Load complex values into registers
				if (!(result.IsAnyRegister || result.Value.Is(HandleInstanceType.STACK_MEMORY) || result.Value.Is(HandleInstanceType.STACK_VARIABLE) || result.Value.Is(HandleInstanceType.TEMPORARY_MEMORY)))
				{
					Memory.MoveToRegister(Unit, result, Assembler.Size, result.Format.IsDecimal(), Trace.GetDirectives(Unit, result));
				}

				variables.Add(variable);
			}

			Unit.States[Label] = variables.Select(i => new VariableLocation(i, Unit.GetVariableValue(i)!)).ToList();
			return;
		}

		var state = Unit.States[Label];

		var destinations = new List<Handle>();
		var sources = new List<Result>();

		foreach (var iterator in state)
		{
			var source = Unit.GetVariableValue(iterator.Variable);
			if (source == null) continue;

			destinations.Add(iterator.Handle);
			sources.Add(source);
		}

		Unit.Append(new ReorderInstruction(Unit, destinations, sources, null));

		// Apply the reordering manually
		for (var i = 0; i < destinations.Count; i++)
		{
			var destination = destinations[i];
			var source = sources[i];
			
			source.Value = destination;
			source.Format = destination.Format;

			if (destination.Is(HandleInstanceType.REGISTER))
			{
				var register = destination.To<RegisterHandle>().Register;
				register.Handle = source;
			}
		}
	}
}