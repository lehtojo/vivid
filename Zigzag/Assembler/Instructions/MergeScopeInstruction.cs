using System.Collections.Generic;
using System.Linq;
using System;

public class MergeScopeInstruction : Instruction
{
	public MergeScopeInstruction(Unit unit) : base(unit) {}

	private Result GetDestinationHandle(Variable variable)
	{
		return Unit.Scope!.Outer?.GetCurrentVariableHandle(variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
	}

	private bool IsUsedLater(Variable variable)
	{
		return Unit.Scope!.Outer?.IsUsedLater(variable) ?? false;
	}

	public override void OnBuild() 
	{
		var moves = new List<MoveInstruction>();

		foreach (var variable in Scope!.ActiveVariables)
		{
			var source = Unit.GetCurrentVariableHandle(variable) ?? throw new ApplicationException("Could not get the current handle for an active variable");

			// Copy the destination value to prevent any relocation leaks
			var destination = new Result(GetDestinationHandle(variable).Value, variable.Type!.Format);
			
			// When the destination is a memory handle, it most likely means it won't be used later
			if (destination.IsMemoryAddress && !IsUsedLater(variable))
			{
				continue;
			}

			if (destination.IsConstant)
			{
				throw new ApplicationException("Constant value was not moved to register or released before entering scope");
			}

			moves.Add(new MoveInstruction(Unit, destination, source));
		}

		Unit.Append(Memory.Relocate(Unit, moves), true);
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Merge-Scope-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.MERGE_SCOPE;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}
}