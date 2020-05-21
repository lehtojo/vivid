using System;

public class AssignInstruction : DualParameterInstruction
{
	public AssignInstruction(Unit unit, Result first, Result second) : base(unit, first, second) 
	{
		Result.Join(Second);
	}

	public override void OnSimulate()
	{
		if (First.Metadata.IsPrimarilyVariable)
		{
      	Unit.Scope!.Variables[First.Metadata.Variable] = Second;
			Second.Metadata.Attach(new VariableAttribute(First.Metadata.Variable));
		}
	}

	public override void OnBuild() 
	{
		if (First.Metadata.IsComplexMemoryAddress || (First.Metadata.IsVariable && First.Metadata.Variable.IsPredictable))
		{
			Unit.Append(new MoveInstruction(Unit, First, Second));
		}
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Assign-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.ASSIGN;
	}
}