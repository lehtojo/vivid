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
		Dependencies = new[] { Result, Start, Offset };

		if (type.IsPack)
		{
			Pack = new DisposablePackHandle(unit, type);

			OutputPack(Pack, 0);

			Result.Value = Pack;
			Result.Format = Assembler.Format;
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
				position = OutputPack(value.Value.To<DisposablePackHandle>(), position);
			}
			else
			{
				value.Value = new ComplexMemoryHandle(Start, Offset, Stride, position);
				value.Format = member.Type!.Format;
				position += member.Type!.AllocationSize;
			}
		}

		return position;
	}

	private void ValidateHandle()
	{
		// Ensure the start value is a constant or in a register
		if (Start.IsConstant || Start.IsInline || Start.IsStandardRegister) return;
		Memory.MoveToRegister(Unit, Start, Assembler.Size, false, Trace.GetDirectives(Unit, Start));
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

		// Fixes situations where a memory address is requested by not immediately loaded into a register, so another instruction might affect the value before loading
		/// Example: address[0] + call(address)
		/// NOTE: In the example above the first operand requests the memory address but does not necessarily load it so the function call might modify the contents of the address
		if (!Trace.IsLoadingRequired(Unit, Result))
		{
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;

			Memory.MoveToRegister(Unit, Result, Assembler.Size, Format.IsDecimal(), Trace.GetDirectives(Unit, Result));
		}
		else
		{
			var address = new Result(ExpressionHandle.CreateMemoryAddress(Start, Offset, Stride), Assembler.Format);
			Memory.MoveToRegister(Unit, address, Assembler.Size, false, Trace.GetDirectives(Unit, Result));

			Result.Value = new MemoryHandle(Unit, address, 0);
			Result.Format = Format;
		}
	}
}