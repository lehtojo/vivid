using System;
using System.Linq;
using System.Collections.Generic;

public static class Returns
{
	/// <summary>
	/// Relocates the pack values to the correct locations
	/// </summary>
	private static void ReturnPackValue(Unit unit, List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, StackMemoryHandle position, List<Handle> destinations, List<Result> sources, Result pack)
	{
		if (pack.Value.Is(HandleInstanceType.PACK))
		{
			var handle = pack.Value.To<PackHandle>();

			foreach (var iterator in handle.Variables)
			{
				var local = iterator.Value;
				var source = new GetVariableInstruction(unit, local, AccessMode.READ).Execute();

				if (local.Type!.IsPack)
				{
					ReturnPackValue(unit, standard_parameter_registers, decimal_parameter_registers, position, destinations, sources, source);
					continue;
				}

				var register = local.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

				if (register != null)
				{
					var destination = new RegisterHandle(register);
					destination.Format = local.GetRegisterFormat();
					destinations.Add(destination);
				}
				else
				{
					// Since there is no more room for parameters in registers, this parameter must be pushed to stack
					var destination = position.Finalize();
					destination.Format = local.GetRegisterFormat();
					destinations.Add(destination);

					position.Offset += Assembler.Size.Bytes;
				}

				sources.Add(source);
			}
		}
		else
		{
			var handle = pack.Value.To<DisposablePackHandle>();

			foreach (var iterator in handle.Variables)
			{
				var member = iterator.Key;
				var source = iterator.Value;

				if (member.Type!.IsPack)
				{
					ReturnPackValue(unit, standard_parameter_registers, decimal_parameter_registers, position, destinations, sources, source);
					continue;
				}

				var register = member.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();
				
				if (register != null)
				{
					var destination = new RegisterHandle(register);
					destination.Format = member.GetRegisterFormat();
					destinations.Add(destination);
				}
				else
				{
					// Since there is no more room for parameters in registers, this parameter must be pushed to stack
					var destination = position.Finalize();
					destination.Format = member.GetRegisterFormat();
					destinations.Add(destination);

					position.Offset += Assembler.Size.Bytes;
				}

				sources.Add(source);
			}
		}
	}

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

			// Handle pack types
			if (to.IsPack)
			{
				var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(i => i[Size.QWORD] == name)!).ToList();
				var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
				var position = new StackMemoryHandle(unit, Assembler.Size.Bytes + (Assembler.IsTargetWindows ? Calls.SHADOW_SPACE_SIZE : 0), true);

				var destinations = new List<Handle>();
				var sources = new List<Result>();
				
				ReturnPackValue(unit, standard_parameter_registers, decimal_parameter_registers, position, destinations, sources, value);
				
				unit.Append(new ReorderInstruction(unit, destinations, sources));
			}

			return new ReturnInstruction(unit, value, unit.Function.ReturnType).Execute();
		}

		unit.TryAppendPosition(scope.End);

		return new ReturnInstruction(unit, null, unit.Function.ReturnType).Execute();
	}
}