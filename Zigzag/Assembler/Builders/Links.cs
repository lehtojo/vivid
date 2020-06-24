using System;

public static class Links
{
	public static Result Build(Unit unit, LinkNode node)
	{
		var start = References.Get(unit, node.Object);

		return node.Member switch
		{
			VariableNode member when member.Variable.Category == VariableCategory.GLOBAL => References.GetVariable(unit,
				member.Variable, AccessMode.READ),
			
			VariableNode member => new GetObjectPointerInstruction(unit, member.Variable, start,
					member.Variable.Alignment ?? throw new ApplicationException("Member variable wasn't aligned"))
				.Execute(),
			
			FunctionNode function => Calls.Build(unit, start, function),
			
			_ => throw new NotImplementedException("Unsupported member node")
		};
	}
}