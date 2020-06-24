using System;
using System.Linq;
using System.Collections.Generic;

public class CacheVariablesInstruction : Instruction
{
	private List<VariableUsageInfo> Usages { get; }
	private Node Body { get; }
	private bool NonVolatileMode { get; }

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
		var available_media_registers = NonVolatileMode
			? Unit.MediaRegisters.Where(r => !r.IsVolatile).ToList()
			: new List<Register>(Unit.MediaRegisters);
		
		var removed_variables = new List<(VariableUsageInfo Info, Register Register)>();

		// Find all the usages which are already cached
		foreach (var usage in Usages)
		{
			// Try to find a register that contains the current variable
			var register = available_registers
				.Find(r => Equals(r.Handle, usage.Reference) && (!NonVolatileMode || !r.IsVolatile));

			if (register == null) continue;
			
			// Remove this variable from the load list since it's in a correct location
			load_list.Remove(usage);

			// The current variable occupies the register so it's not available
			available_registers.Remove(register);

			removed_variables.Add((usage, register));
		}

		// Sort the variables based on their number of usages (the least used variables first)
		removed_variables.Sort((a, b) => a.Info.Usages.CompareTo(b.Info.Usages));

		var remaining_usages = new List<VariableUsageInfo>();

		foreach (var usage in load_list)
		{
			var is_media_register_needed = usage.Reference!.Format.IsDecimal();
			var register_list = is_media_register_needed ? available_media_registers : available_registers;
			
			// Try to find totally available register
			var register = register_list.Find(r => r.Handle == null);

			if (register == null)
			{
				// Try to find a register which holds a value but it's not important anymore
				register = register_list.Find(r => r.IsAvailable(Position));

				if (register == null)
				{
					// Try to get the next register
					register = register_list.Pop();

					if (register != null)
					{
						// Clear the register safely if it holds something
						Unit.Release(register);
					}
				}
			}

			if (register == null)
			{
				var index = removed_variables
					.FindIndex(0, p => p.Register.IsMediaRegister == is_media_register_needed);
				
				if (index < 0)
				{
					// There are no available registers
					remaining_usages.Add(usage);
					break;
				}

				var (info, reserved_register) = removed_variables[index];

				// The current variable is only allowed to take over the used register if it will be more used
				if (info.Usages >= usage.Usages)
				{
					remaining_usages.Add(usage);
					continue;
				}

				removed_variables.RemoveAt(0);

				// Release the removed variable since its register will used with the current variable
				Release(info);

				register = reserved_register;
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
		return new [] { Result };
	}
}