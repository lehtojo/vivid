using System;

public class EvacuateInstruction : Instruction
{
	public Instruction Perspective { get; private set; }

	public EvacuateInstruction(Unit unit, Instruction perspective) : base(unit)  
	{
		Perspective = perspective;
	}

	public override void OnBuild() 
	{
		// Save all imporant values in normal volatile registers
		Unit.VolatileRegisters.ForEach(source => 
		{
			// Skip values which aren't needed after the call instruction
			if (source.IsAvailable(Perspective.Position)) 
			{
				return;
			}
			
			// Media registers must be released to memory
			if (source.IsMediaRegister)
			{
				Unit.Release(source);
				return;
			}

			var destination = (Handle?)null;
			var register = Unit.GetNextNonVolatileRegister(false);
				
			if (register != null) 
			{
				destination = new RegisterHandle(register);
			}
			else
			{
				if (!source.IsReleasable)
				{
					throw new ApplicationException("Register evacuation failed because releasing a non-variable value to memory is not implemented");
				}

				Unit.Release(source);
				return;
			}

			Unit.Append(new MoveInstruction(Unit, new Result(destination, source.Format), source.Handle!)
			{
				Description = $"Evacuate an important value in '{destination}'",
				Type = MoveType.RELOCATE
			});
		});

		// Save all important values inside media registers by releasing them to memory
		Unit.MediaRegisters.ForEach(source =>
		{
			// Skip values which aren't needed after the call instruction
			if (source.IsAvailable(Perspective.Position)) 
			{
				return;
			}
			
			Unit.Release(source);
		});
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Evacuate-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.EVACUATE;
	}
}