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
		Unit.VolatileRegisters.ForEach(source => 
		{
			if (!source.IsAvailable(Perspective.Position)) 
			{
				var destination = (Handle?)null;
				var register = Unit.GetNextNonVolatileRegister();
				
				if (register != null) 
				{
					destination = new RegisterHandle(register);
				}
				else
				{
					throw new NotImplementedException("Stack move required but not implemented");
				}

            var move = new MoveInstruction(Unit, new Result(destination), source.Handle!)
            {
               Type = MoveType.RELOCATE
            };

            Unit.Append(move);
			}
		});
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}

	public override Result? GetDestinationDependency()
	{
		return null;   
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.EVACUATE;
	}
}