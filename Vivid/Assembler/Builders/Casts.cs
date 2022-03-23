using System;

public static class Casts
{
	public static Result Cast(Unit unit, Result result, Type from, Type to)
	{
		if (from == to) return result;

		if (from is Number x && to is Number y)
		{
			var a = x.Format.IsDecimal();
			var b = y.Format.IsDecimal();

			// Execute only if exactly one of the booleans is true
			if (a ^ b) return new ConvertInstruction(unit, result, y.Format).Execute();

			return result;
		}

		if (from.IsTypeInherited(to)) // Determine whether the cast is a down cast
		{
			var base_offset = from.GetSupertypeBaseOffset(to) ?? throw new ApplicationException("Could not compute base offset of a super type while building down cast");
			if (base_offset == 0) return result;

			var offset = new Result(new ConstantHandle((long)base_offset), Assembler.Signed);
			return new AdditionInstruction(unit, result, offset, result.Format, false).Execute();
		}

		if (to.IsTypeInherited(from)) // Determine whether the cast is a up cast
		{
			var base_offset = to.GetSupertypeBaseOffset(from) ?? throw new ApplicationException("Could not compute base offset of a super type while building up cast");
			if (base_offset == 0) return result;

			var offset = new Result(new ConstantHandle((long)-base_offset), Assembler.Signed);
			return new AdditionInstruction(unit, result, offset, result.Format, false).Execute();
		}

		// This means that the cast is unsafe since the types have nothing in common
		return result;
	}

	public static Result Build(Unit unit, CastNode node, AccessMode mode)
	{
		var from = node.Object.GetType();
		var to = node.GetType();

		var result = References.Get(unit, node.Object, mode);

		return Cast(unit, result, from, to);
	}
}