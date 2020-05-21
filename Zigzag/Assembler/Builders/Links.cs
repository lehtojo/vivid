using System;

public static class Links
{
	public static Result Build(Unit unit, LinkNode node)
	{
		var start = References.Get(unit, node.Object);

		if (node.Member is VariableNode member)
		{
			if (member.Variable.Category == VariableCategory.GLOBAL)
			{
				return References.GetVariable(unit, member.Variable, AccessMode.READ);
			}

			return new GetObjectPointerInstruction(unit, member.Variable, start, member.Variable.Alignment ?? throw new ApplicationException("Member variable wasn't aligned")).Execute();
		}
		else if (node.Member is FunctionNode function)
		{
			return Calls.Build(unit, start, function);
		}
		else
		{
			throw new NotImplementedException("Unsupported member node");
		}
	}
}