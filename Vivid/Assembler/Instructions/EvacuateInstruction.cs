/// <summary>
/// Ensures that variables and values which are required later are moved to locations which are not affected by call instructions for example
/// This instruction is works on all architectures
/// </summary>
public class EvacuateInstruction : Instruction
{
	public Instruction Perspective { get; private set; }

	public EvacuateInstruction(Unit unit, Instruction perspective) : base(unit, InstructionType.EVACUATE)
	{
		Perspective = perspective;
		IsAbstract = true;
	}

	public override void OnBuild()
	{
		// Save all important values in the standard volatile registers
		Unit.VolatileRegisters.ForEach(i =>
		{
			i.IsLocked = true;

			// Skip values which are not needed after the call instruction
			/// NOTE: The availability of the register is not checked the standard way since they are usually locked at this stage
			if (i.Handle == null || !i.Handle.IsValid(Perspective.Position + 1) || i.IsHandleCopy())
			{
				return;
			}

			// Try to get an available non-volatile register
			var destination = (Handle?)null;
			var register = Unit.GetNextNonVolatileRegister(i.IsMediaRegister, false);

			// Use the non-volatile register, if one was found
			if (register != null)
			{
				destination = new RegisterHandle(register);
			}
			else
			{
				// Since there are no non-volatile registers available, the value must be relocated to stack memory
				Unit.Release(i);
				return;
			}

			Unit.Append(new MoveInstruction(Unit, new Result(destination, i.Format), i.Handle!)
			{
				Description = $"Evacuate an important value into '{destination}'",
				Type = MoveType.RELOCATE
			});
		});

		// Unlock all the volatile registers
		Unit.VolatileRegisters.ForEach(i => i.IsLocked = false);
	}
}