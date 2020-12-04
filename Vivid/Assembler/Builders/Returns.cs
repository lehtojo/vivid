using System;

public static class Returns
{
	public static Result Build(Unit unit, ReturnNode node)
	{
		unit.TryAppendPosition(node);
		
		if (node.Value != null)
		{
			var from = node.Value.GetType();
			var to = unit.Function.ReturnType ?? throw new ApplicationException("Function return type was not resolved");
			var value = Casts.Cast(unit, References.Get(unit, node.Value), from, to);

			return new ReturnInstruction(unit, value, unit.Function.ReturnType).Execute();
		}

		return new ReturnInstruction(unit, null, unit.Function.ReturnType).Execute();
	}
}