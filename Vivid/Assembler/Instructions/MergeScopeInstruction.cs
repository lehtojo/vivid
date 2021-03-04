using System.Collections.Generic;

/// <summary>
/// Relocates variables so that their locations match the state of the outer scope
/// This instruction works on all architectures
/// </summary>
public class MergeScopeInstruction : Instruction
{
	public MergeScopeInstruction(Unit unit) : base(unit, InstructionType.MERGE_SCOPE)
	{
		Description = "Relocates values so that their locations match the state of the outer scope";
		IsAbstract = true;
	}

	private Result GetVariableStackHandle(Variable variable)
	{
		return new Result(References.CreateVariableHandle(Unit, variable), variable.Type!.Format);
	}

	private Result GetDestinationHandle(Variable variable)
	{
		return Unit.Scope!.Outer?.GetCurrentVariableHandle(variable) ?? GetVariableStackHandle(variable);
	}

	public override void OnBuild()
	{
		var moves = new List<MoveInstruction>();

		foreach (var variable in Scope!.Actives)
		{
			var source = Unit.GetCurrentVariableHandle(variable) ?? GetVariableStackHandle(variable);

			// Copy the destination value to prevent any relocation leaks
			var handle = GetDestinationHandle(variable);
			var destination = new Result(handle.Value, handle.Format);

			if (destination.IsConstant)
			{
				continue;
			}

			moves.Add(new MoveInstruction(Unit, destination, source)
			{
				IsSafe = true,
				Description = "Relocates the source value to merge the current scope with the outer scope",
				Type = MoveType.RELOCATE
			});
		}

		Unit.Append(Memory.Align(Unit, moves), true);
	}
}