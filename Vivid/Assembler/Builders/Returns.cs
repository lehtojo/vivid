using System;
using System.Linq;
using System.Collections.Generic;

public static class Returns
{
	/// <summary>
	/// Returns the specified pack by using the registers used when passing packs in parameters
	/// </summary>
	private static void ReturnPack(Unit unit, Result value, Type type)
	{
		var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(i => i[Size.QWORD] == name)!).ToList();
		var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();

		var destinations = new List<Handle>();
		var sources = new List<Result>();

		// Pass the first value using the stack just above the return address
		var position = new StackMemoryHandle(unit, Assembler.IsX64 ? Assembler.Size.Bytes : 0);
		Calls.PassArgument(unit, destinations, sources, standard_parameter_registers, decimal_parameter_registers, position, value, type, Assembler.Format);

		unit.Append(new ReorderInstruction(unit, destinations, sources, unit.Function.ReturnType!));
	}

	public static Result Build(Unit unit, ReturnNode node)
	{
		unit.TryAppendPosition(node);

		var scope = (ScopeNode?)node.FindParent(NodeType.SCOPE) ?? throw new ApplicationException("Missing parent scope");

		if (node.Value != null)
		{
			var from = node.Value.GetType();
			var to = unit.Function.ReturnType ?? throw new ApplicationException("Function return type was not resolved");
			var value = Casts.Cast(unit, References.Get(unit, node.Value), from, to);

			unit.TryAppendPosition(scope.End);

			if (to.IsPack) ReturnPack(unit, value, to);
			return new ReturnInstruction(unit, value, unit.Function.ReturnType).Execute();
		}

		unit.TryAppendPosition(scope.End);

		return new ReturnInstruction(unit, null, unit.Function.ReturnType).Execute();
	}
}