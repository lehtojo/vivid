using System.Collections.Generic;

/// <summary>
/// Returns a handle for accessing raw memory
/// This instruction works on all architectures
/// </summary>
public class GetMemoryAddressInstruction : Instruction
{
	public AccessMode Mode { get; private set; }
	public Format Format { get; private set; }

	public Result Start { get; private set; }
	public Result Offset { get; private set; }
	public int Stride { get; private set; }
	public DisposablePackHandle? Pack { get; private set; } = null;

	public GetMemoryAddressInstruction(Unit unit, AccessMode mode, Type type, Format format, Result start, Result offset, int stride) : base(unit, InstructionType.GET_MEMORY_ADDRESS)
	{
		Mode = mode;
		Start = start;
		Offset = offset;
		Stride = stride;
		Format = format;
		IsAbstract = true;
		Dependencies = new List<Result> { Result, Start, Offset };

		if (type.IsPack)
		{
			Pack = new DisposablePackHandle(unit, type);

			OutputPack(Pack, 0);

			Result.Value = Pack;
			Result.Format = Settings.Format;
			return;
		}

		Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
		Result.Format = Format;
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
				value.Value = new ComplexMemoryHandle(Start, Offset, Stride, position);
			}
			else
			{
				// 1. Ensure we are in build mode, so we can use registers
				// 2. Ensure the pack member is used, so we do not move it to a register unnecessarily
				if (Unit.Mode == UnitMode.BUILD && !value.IsDeactivating())
				{
					if (member.IsInlined())
					{
						value.Value = new ExpressionHandle(Offset, Stride, Start, position);
					}
					else
					{
						// Since we are in build mode and the member is required, we need to output a register value
						value.Value = new ComplexMemoryHandle(Start, Offset, Stride, position);
						Memory.MoveToRegister(Unit, value, Size.FromFormat(value.Format), value.Format == Format.DECIMAL, Trace.For(Unit, value));
					}
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
		Memory.MoveToRegister(Unit, Start, Settings.Size, false, Trace.For(Unit, Start));
	}

	public override void OnBuild()
	{
		ValidateHandle();

		if (Pack != null)
		{
			OutputPack(Pack, 0);
			Result.Value = Pack;
			Result.Format = Settings.Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;

			Memory.MoveToRegister(Unit, Result, Settings.Size, Format.IsDecimal(), Trace.For(Unit, Result));
		}
		else
		{
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;
		}
	}
}