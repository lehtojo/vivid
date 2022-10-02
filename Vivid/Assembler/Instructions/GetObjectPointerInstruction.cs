using System.Collections.Generic;

/// <summary>
/// Returns a handle for accessing member variables
/// This instruction works on all architectures
/// </summary>
public class GetObjectPointerInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; private set; }
	public AccessMode Mode { get; private set; }
	public DisposablePackHandle? Pack { get; private set; } = null;

	public GetObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset, AccessMode mode) : base(unit, InstructionType.GET_OBJECT_POINTER)
	{
		Variable = variable;
		Start = start;
		Offset = offset;
		Mode = mode;
		IsAbstract = true;
		Dependencies = new List<Result> { Result, Start };

		Result.Format = Variable.Type!.Format;

		if (Variable.Type!.IsPack)
		{
			Pack = new DisposablePackHandle(unit, Variable.Type!);

			OutputPack(Pack, 0);

			Result.Value = Pack;
			Result.Format = Assembler.Format;
			return;
		}
	}

	private int OutputPack(DisposablePackHandle pack, int position)
	{
		foreach (var iterator in pack.Members)
		{
			var member = iterator.Key;
			var value = iterator.Value;

			if (member.Type!.IsPack)
			{
				// Output the members of the nested pack using this function recursively
				position = OutputPack(value.Value.To<DisposablePackHandle>(), position);
				continue;
			}

			// Update the format of the pack member
			value.Format = member.Type!.Format;

			if (Mode == AccessMode.WRITE)
			{
				// Since we are in write mode, we need to output a memory address for the pack member
				value.Value = new MemoryHandle(Unit, Start, Offset + position);
			}
			else
			{
				// 1. Ensure we are in build mode, so we can use registers
				// 2. Ensure the pack member is used, so we do not move it to a register unnecessarily
				if (Unit.Mode == UnitMode.BUILD && !value.IsDeactivating())
				{
					// Since we are in build mode and the member is required, we need to output a register value
					value.Value = new MemoryHandle(Unit, Start, Offset + position);
					Memory.MoveToRegister(Unit, value, Size.FromFormat(value.Format), value.Format.IsDecimal(), Trace.For(Unit, value));
				}
				else
				{
					value.Value = new Handle();
				}
			}

			position += member.Type!.AllocationSize;
		}

		return position;
	}

	private void ValidateHandle()
	{
		// Ensure the start value is a constant or in a register
		if (Start.IsConstant || Start.IsStackAllocation || Start.IsStandardRegister) return;
		Memory.MoveToRegister(Unit, Start, Assembler.Size, false, Trace.For(Unit, Start));
	}

	public override void OnBuild()
	{
		ValidateHandle();

		if (Pack != null)
		{
			OutputPack(Pack, 0);
			Result.Value = Pack;
			Result.Format = Assembler.Format;
			return;
		}

		if (Variable.IsInlined())
		{
			Result.Value = ExpressionHandle.CreateMemoryAddress(Start, Offset);
			Result.Format = Variable.Type!.Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;

			Memory.MoveToRegister(Unit, Result, Assembler.Size, Variable.GetRegisterFormat().IsDecimal(), Trace.For(Unit, Result));
		}
		else
		{
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;
		}
	}
}