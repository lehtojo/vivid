using System.Collections.Generic;

/// <summary>
/// Relocates variables so that their locations match the state of the outer scope
/// This instruction works on all architectures
/// </summary>
public class MergeScopeInstruction : Instruction
{
	public new Scope Scope { get; private set; }

	public MergeScopeInstruction(Unit unit, Scope scope) : base(unit, InstructionType.MERGE_SCOPE)
	{
		Scope = scope;
		Description = "Relocates values so that their locations match the state of the outer scope";
		IsAbstract = true;
	}

	private Result GetVariableStackHandle(Variable variable)
	{
		return new Result(References.CreateVariableHandle(Unit, variable), variable.Type!.Format);
	}

	private Result GetDestinationHandle(Variable variable)
	{
		return Scope.Outer?.GetVariableValue(variable) ?? GetVariableStackHandle(variable);
	}

	public override void OnBuild()
	{
		var moves = new List<MoveInstruction>();

		foreach (var variable in Scope.Actives)
		{
			// Packs variables are not merged, their members are instead
			if (variable.Type!.IsPack) continue;

			var source = Unit.GetVariableValue(variable) ?? GetVariableStackHandle(variable);

			// Copy the destination value to prevent any relocation leaks
			var handle = GetDestinationHandle(variable);
			var destination = new Result(handle.Value, handle.Format);

			if (destination.IsConstant) continue;
			
			// If the only difference between the source and destination, is the size, and the source size is larger than the destination size, no conversion is needed
			/// NOTE: Move instruction should still be created, so that the destination is locked
			if (destination.Value.Equals(source.Value) && Size.FromFormat(destination.Format).Bytes <= Size.FromFormat(source.Format).Bytes)
			{
				source = destination;
			}

			moves.Add(new MoveInstruction(Unit, destination, source)
			{
				IsDestinationProtected = true,
				Description = "Relocates the source value to merge the current scope with the outer scope",
				Type = MoveType.RELOCATE
			});
		}

		Unit.Append(Memory.Align(Unit, moves), true);
	}
}