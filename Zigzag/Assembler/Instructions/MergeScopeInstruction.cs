using System.Collections.Generic;
using System;

public class MergeScopeInstruction : Instruction
{
	public MergeScopeInstruction(Unit unit, IEnumerable<Variable> variables) : base(unit) {}

	private Result GetDestinationHandle(Variable variable)
	{
		return Unit.Scope!.Outer?.GetCurrentVariableHandle(Unit, variable) ?? References.GetVariable(Unit, variable, AccessMode.WRITE);
	}

	private bool IsUsedLater(Variable variable)
	{
		return Unit.Scope!.Outer?.IsUsedLater(variable) ?? false;
	}

	public override void OnBuild() 
	{
		var moves = new List<DualParameterInstruction>();

		foreach (var variable in Scope!.ActiveVariables)
		{
			var source = Unit.GetCurrentVariableHandle(variable) ?? throw new ApplicationException("Couldn't get the current handle for an active variable");

			// Copy the destination value to prevent any relocation leaks
			var destination = new Result(GetDestinationHandle(variable).Value);
			
			// When the destination is a memory handle, it most likely means it won't be used later
			if (destination.Value.Type == HandleType.MEMORY && !IsUsedLater(variable))
			{
				continue;
			}

			if (destination.Value.Type == HandleType.CONSTANT)
			{
				throw new ApplicationException("Constant value was not moved to register or released before entering scope");
			}

			if (!destination.Value.Equals(source.Value))
			{
				moves.Add(new MoveInstruction(Unit, destination, source));
			}
		}

		var remove_list = new List<DualParameterInstruction>();
		var exchanges = new List<ExchangeInstruction>();

		foreach (var a in moves)
		{
			foreach (var b in moves)
			{
				if (a == b || remove_list.Contains(a) || remove_list.Contains(b)) continue;
				
				if (a.First.Value.Equals(b.Second.Value) &&
					a.Second.Value.Equals(b.First.Value))
				{
					exchanges.Add(new ExchangeInstruction(Unit, a.First, a.Second, false));
					
					remove_list.Add(a);
					remove_list.Add(b);
					break;
				}
			}
		}

		moves.AddRange(exchanges);
		moves.RemoveAll(m => remove_list.Contains(m));

		moves.Sort((a, b) => a.First.Value.Equals(b.Second.Value) ? -1 : 0);
		moves.ForEach(move => Unit.Append(move));
	}

	public override Result? GetDestinationDependency()
	{
		return null;
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