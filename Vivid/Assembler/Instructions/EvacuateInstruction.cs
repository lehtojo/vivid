/// <summary>
/// Ensures that variables and values which are required later are moved to locations which are not affected by call instructions for example
/// This instruction is works on all architectures
/// </summary>
public class EvacuateInstruction : Instruction
{
	public EvacuateInstruction(Unit unit) : base(unit, InstructionType.EVACUATE)
	{
		IsAbstract = true;
	}

	public override void OnBuild()
	{
		while (true)
		{
			var evacuated = false;

			// Save all important values in the standard volatile registers
			foreach (var register in Unit.VolatileRegisters)
			{
				// Skip values which are not needed after the call instruction
				/// NOTE: The availability of the register is not checked the standard way since they are usually locked at this stage
				if (register.Value == null || !register.Value.IsActive() || register.Value.IsDeactivating() || register.IsHandleCopy()) continue;

				evacuated = true;

				// Try to get an available non-volatile register
				var destination = (Handle?)null;
				var non_volatile_register = Unit.GetNextNonVolatileRegister(register.IsMediaRegister, false);

				// Use the non-volatile register, if one was found
				if (non_volatile_register != null)
				{
					destination = new RegisterHandle(non_volatile_register);
				}
				else
				{
					// Since there are no non-volatile registers available, the value must be relocated to stack memory
					Unit.Release(register);
					continue;
				}

				Unit.Add(new MoveInstruction(Unit, new Result(destination, register.Value!.Format), register.Value!)
				{
					Description = "Evacuates a value",
					Type = MoveType.RELOCATE
				});
			}

			if (!evacuated) break;
		}
	}
}