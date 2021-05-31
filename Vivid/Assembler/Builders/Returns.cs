using System;

public static class Returns
{
	public static Result Build(Unit unit, ReturnNode node)
	{
		unit.TryAppendPosition(node);

		var scope = (ScopeNode?)node.FindParent(i => i.Is(NodeType.SCOPE)) ?? throw new ApplicationException("Missing parent scope");

		if (node.Value != null)
		{
			var from = node.Value.GetType();
			var to = unit.Function.ReturnType ?? throw new ApplicationException("Function return type was not resolved");
			var value = Casts.Cast(unit, References.Get(unit, node.Value), from, to);

			unit.TryAppendPosition(scope.End);

			return new ReturnInstruction(unit, value, unit.Function.ReturnType).Execute();
		}

		unit.TryAppendPosition(scope.End);

		return new ReturnInstruction(unit, null, unit.Function.ReturnType).Execute();
	}
}