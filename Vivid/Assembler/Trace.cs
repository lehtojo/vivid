using System.Linq;

public static class Trace
{
	/// <summary>
	/// Returns whether the specified result lives through at least one call
	/// </summary>
	public static bool IsUsedAfterCall(Unit unit, Result result)
	{
		var start = result.Lifetime.Start;
		var end = result.Lifetime.End;

		end = end == -1 ? unit.Instructions.Count : end;
		start = start == -1 ? unit.Instructions.Count : start;

		for (var i = start + 1; i < end; i++)
		{
			if (unit.Instructions[i].Is(InstructionType.CALL))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Returns whether the specified result stays constant during the lifetime of the specified parent
	/// </summary>
	public static bool IsLoadingRequired(Unit unit, Result result)
	{
		if (IsUsedAfterCall(unit, result))
		{
			return true;
		}

		var start = result.Lifetime.Start;
		var end = result.Lifetime.End;

		end = end == -1 ? unit.Instructions.Count : end;
		start = start == -1 ? unit.Instructions.Count : start;

		return unit.Instructions.GetRange(start, end - start).Any(i => 
			i.Is(InstructionType.GET_VARIABLE) && i.To<GetVariableInstruction>().Mode == AccessMode.WRITE && !i.To<GetVariableInstruction>().Result.Equals(result) ||
			i.Is(InstructionType.GET_OBJECT_POINTER) && i.To<GetObjectPointerInstruction>().Mode == AccessMode.WRITE && !i.To<GetObjectPointerInstruction>().Result.Equals(result) ||
			i.Is(InstructionType.GET_MEMORY_ADDRESS) && i.To<GetMemoryAddressInstruction>().Mode == AccessMode.WRITE && !i.To<GetMemoryAddressInstruction>().Result.Equals(result)
		);
	}
}