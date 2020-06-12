using System;
using System.Linq;
using System.Collections.Generic;

public class CacheVariablesInstruction : Instruction
{
	private List<VariableUsageInfo> Usages { get; set; }
	private Node Body { get; set; }
	private bool NonVolatileMode { get; set; }

	public CacheVariablesInstruction(Unit unit, Node body, List<VariableUsageInfo> variables, bool non_volatile_mode) : base(unit)
	{
		Usages = variables;
		Body = body;
		NonVolatileMode = non_volatile_mode;

		// Load all the variables before caching
		foreach (var usage in Usages)
		{
			usage.Reference = References.GetVariable(unit, usage.Variable, AccessMode.READ);
		}
	}

	private void RemoveAllReadonlyConstantVariables()
	{
		for (var i = Usages.Count - 1; i >= 0; i--)
		{
			var usage = Usages[i];

			// There's no need to move variables to registers that aren't edited in the body and are constants
			if (!usage.Variable.IsEditedInside(Body) && 
				usage.Reference?.Value.Type == HandleType.CONSTANT)
			{
				Usages.RemoveAt(i);
			}
		}
	}

	private void Release(VariableUsageInfo usage)
	{
		Unit.Release(usage.Reference!.Value.To<RegisterHandle>().Register);
	}

	public override void OnBuild()
	{
		RemoveAllReadonlyConstantVariables();

		var load_list = new List<VariableUsageInfo>(Usages);

		var available_registers = new List<Register>(NonVolatileMode ? Unit.NonVolatileRegisters : Unit.NonReservedRegisters);
		var removed_variables = new List<(VariableUsageInfo Info, Register Register)>();

		foreach (var usage in Usages)
		{
			// Try to find a register that contains the current variable
			var register = available_registers.Find(r => (r.Handle?.Metadata.Equals(usage.Variable) ?? false) && (!NonVolatileMode || !r.IsVolatile));

			if (register != null)
			{  
				// Remove this variable from the load list since it's in a correct location
				load_list.Remove(usage);

				// The current variable occupies the register so it's not available
				available_registers.Remove(register);

				removed_variables.Add((usage, register));
			}
		}

		// Sort the variables based on their number of usages (the least used variables first)
		removed_variables.Sort((a, b) => a.Info.Usages.CompareTo(b.Info.Usages));

		var remaining_usages = new List<VariableUsageInfo>();

		foreach (var usage in load_list)
		{
			// Try to find totally available register
			var register = available_registers.Find(r => r.Handle == null);

			if (register == null)
			{
				// Try to find a register which holds a value but it's not important anymore
				register = available_registers.Find(r => r.IsAvailable(Position));

				if (register == null)
				{
					// Try to get the next register
					register = available_registers.Pop();

					if (register != null)
					{
						// Clear the register safely if it holds something
						Unit.Release(register);
					}
				}
			}

			if (register == null)
			{
				if (removed_variables.Count == 0)
				{
					// There are no available registers
					remaining_usages.Add(usage);
					break;
				}

				var removed = removed_variables.First();

				// The current variable is only allowed to take over the used register if it will be more used
				if (removed.Info.Usages >= usage.Usages)
				{
					remaining_usages.Add(usage);
					continue;
				}

				removed_variables.RemoveAt(0);

				// Release the removed variable since its register will used with the current variable
				Release(removed.Info);

				register = removed.Register;
			}

			var destination = new Result(new RegisterHandle(register), usage.Reference!.Format);
			var source = usage.Reference!;

			Unit.Append(new MoveInstruction(Unit, destination, source)
			{
				Type = MoveType.RELOCATE
			});
		}

		// Release the remaining variables
		foreach (var usage in remaining_usages)
		{
			Release(usage);
		}
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Cache-Variables-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.CACHE_VARIABLES;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}
}