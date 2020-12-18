using System.Collections.Generic;
using System;

public class MergeScopeInstruction : Instruction
{
	public MergeScopeInstruction(Unit unit) : base(unit) { }

	private Result GetVariableStackHandle(Variable variable)
	{
		return new Result(References.CreateVariableHandle(Unit, variable), variable.Type!.Format);
	}

	private Result GetDestinationHandle(Variable variable)
	{
		return Unit.Scope!.Outer?.GetCurrentVariableHandle(variable) ?? GetVariableStackHandle(variable);
	}

	private bool IsUsedLater(Variable variable)
	{
		return Unit.Scope!.Outer?.IsUsedLater(variable) ?? false;
	}

	public override void OnBuild()
	{
		var moves = new List<MoveInstruction>();

		foreach (var variable in Scope!.Actives)
		{
			var source = Unit.GetCurrentVariableHandle(variable) ?? GetVariableStackHandle(variable);

			// Copy the destination value to prevent any relocation leaks
			var destination = new Result(GetDestinationHandle(variable).Value, variable.GetRegisterFormat());

			// When the destination is a memory handle, it most likely means it won't be used later
			if (destination.IsMemoryAddress && !IsUsedLater(variable))
			{
				continue;
			}

			if (destination.IsConstant)
			{
				continue;
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
		return new[] { Result };
	}
}