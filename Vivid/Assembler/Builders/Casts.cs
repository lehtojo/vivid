using System;

public static class Casts
{
	public static Result Cast(Unit unit, Result result, Type from, Type to)
	{
		if (from == to) return result;

		// Number casts:
		if (from.IsNumber && to.IsNumber)
		{
			var a = from.To<Number>().Format;
			var b = to.To<Number>().Format;

			if (a != b) return new ConvertInstruction(unit, result, b).Add();

			return result;
		}

		// Determine whether the cast is a down cast
		if (from.IsTypeInherited(to))
		{
			var offset = from.GetSupertypeBaseOffset(to) ?? throw new ApplicationException("Could not compute base offset of a super type while building down cast");
			if (offset == 0) return result;

			return new AdditionInstruction(unit, result, new Result(new ConstantHandle((long)offset), Assembler.Signed), result.Format, false).Add();
		}

		// Determine whether the cast is a up cast
		if (to.IsTypeInherited(from))
		{
			var offset = to.GetSupertypeBaseOffset(from) ?? throw new ApplicationException("Could not compute base offset of a super type while building up cast");
			if (offset == 0) return result;

			return new AdditionInstruction(unit, result, new Result(new ConstantHandle((long)-offset), Assembler.Signed), result.Format, false).Add();
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