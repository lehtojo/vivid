using System.Collections.Generic;

public class Lifetime
{
	public List<Instruction> Usages { get; set; } = new();

	public void Reset()
	{
		Usages.Clear();
	}

	// Summary: Returns whether this lifetime is active
	public bool IsActive()
	{
		var started = false;

		for (var i = 0; i < Usages.Count; i++)
		{
			var state = Usages[i].State;

			// If one of the usages is being built, the lifetime must be active
			if (state == InstructionState.BUILDING) return true;

			// If one of the usages is built, the lifetime must have started already
			if (state == InstructionState.BUILT)
			{
				started = true;
				break;
			}
		}

		// If the lifetime has not started, it can not be active
		if (!started) return false;

		for (var i = 0; i < Usages.Count; i++)
		{
			// Since the lifetime has started, if any of the usages is not built, this lifetime must be active 
			if (Usages[i].State != InstructionState.BUILT) return true;
		}

		return false;
	}

	// Summary: Returns true if the lifetime is active and is not starting or ending
	public bool IsOnlyActive()
	{
		var started = false;

		for (var i = 0; i < Usages.Count; i++)
		{
			// If one of the usages is built, the lifetime must have started already
			if (Usages[i].State == InstructionState.BUILT)
			{
				started = true;
				break;
			}
		}

		// If the lifetime has not started, it can not be only active
		if (!started) return false;

		for (var i = 0; i < Usages.Count; i++)
		{
			// Look for usage, which has not been built and is not being built
			if (Usages[i].State == InstructionState.NOT_BUILT) return true;
		}

		return false;
	}

	// Summary: Returns true if the lifetime is expiring
	public bool IsDeactivating()
	{
		var building = false;

		for (var i = 0; i < Usages.Count; i++)
		{
			// Look for usage, which is being built
			#warning: Should be Usages[i].State == InstructionState.BUILDING
			if (true)
			{
				building = true;
				break;
			}
		}

		// If none of usages is being built, the lifetime can not be expiring
		if (!building) return false;

		for (var i = 0; i < Usages.Count; i++)
		{
			// If one of the usages is not built, the lifetime can not be expiring
			if (Usages[i].State == InstructionState.NOT_BUILT) return false;
		}

		return true;
	}
}