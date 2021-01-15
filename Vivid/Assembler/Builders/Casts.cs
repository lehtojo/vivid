using System;

public static class Casts
{
	public static Result Cast(Unit unit, Result result, Type from, Type to)
	{
		if (from == to)
		{
			return result;
		}

		if (from is Number x && to is Number y)
		{
			if ((x == Types.DECIMAL && y != Types.DECIMAL) || (x != Types.DECIMAL && y == Types.DECIMAL))
			{
				return new ConvertInstruction(unit, result, to != Types.DECIMAL).Execute();
			}

			return result;
		}

		if (from.IsTypeInherited(to)) // Determine whether the cast is a down cast
		{
			var base_offset = from.GetSupertypeBaseOffset(to) ?? throw new ApplicationException("Could not calculate base offset of a super type while building down cast");

			if (base_offset == 0)
			{
				return result;
			}

			var calculation = ExpressionHandle.CreateMemoryAddress(result, base_offset);

			return new Result(calculation, result.Format);
		}

		if (to.IsTypeInherited(from)) // Determine whether the cast is a up cast
		{
			var base_offset = to.GetSupertypeBaseOffset(from) ?? throw new ApplicationException("Could not calculate base offset of a super type while building up cast");

			if (base_offset == 0)
			{
				return result;
			}

			var calculation = ExpressionHandle.CreateMemoryAddress(result, -base_offset);

			return new Result(calculation, result.Format);
		}

		// This means that the cast is unsafe since the types have nothing in common
		return result;
	}

	public static Result Build(Unit unit, CastNode node, AccessMode mode)
	{
		var from = node.Object.GetType();
		var to = node.GetType();

		var result = References.Get(unit, node.Object, mode);
		result.Format = to.Format;

		return Cast(unit, result, from, to);
	}
}