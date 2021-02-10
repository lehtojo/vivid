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
		// Save all imporant values in the standard volatile registers
		Unit.VolatileRegisters.ForEach(source =>
		{
			// Skip values which are not needed after the call instruction
			/// NOTE: The availability of the register is not checked the standard way since they are usually locked at this stage
			if (source.Handle == null || !source.Handle.IsValid(Perspective.Position + 1) || source.IsHandleCopy())
			{
				return;
			}

			var destination = (Handle?)null;
			var register = Unit.GetNextNonVolatileRegister(source.IsMediaRegister, false);

			if (register != null)
			{
				destination = new RegisterHandle(register);
			}
			else
			{
				Unit.Release(source);
				return;
			}

			Unit.Append(new MoveInstruction(Unit, new Result(destination, source.Format), source.Handle!)
			{
				Description = $"Evacuate an important value in '{destination}'",
				Type = MoveType.RELOCATE
			});
		});
	}
}