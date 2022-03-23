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
		Dependencies = new[] { Result, Start };

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
				if (Unit.Mode == UnitMode.BUILD && !value.IsExpiring(Unit.Position))
				{
					// Since we are in build mode and the member is required, we need to output a register value
					value.Value = new MemoryHandle(Unit, Start, Offset + position);
					Memory.MoveToRegister(Unit, value, Size.FromFormat(value.Format), value.Format.IsDecimal(), Trace.GetDirectives(Unit, value));
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
		if (!Start.IsConstant && !Start.IsInline && !Start.IsStandardRegister)
		{
			Memory.MoveToRegister(Unit, Start, Assembler.Size, false, Trace.GetDirectives(Unit, Start));
		}
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

		/// NOTE: Addresses of inlined member variables can not be modified, therefore the text below does not affect inlined member variables
		if (Variable.IsInlined())
		{
			Result.Value = ExpressionHandle.CreateMemoryAddress(Start, Offset);
			Result.Format = Variable.Type!.Format;
			return;
		}

		// Fixes situations where an object memory address is requested by not immediately loaded into a register, so another instruction might affect the value before loading
		/// Example: object.member + object.modify()
		/// NOTE: In the example above the first operand requests the memory address but does not necessarily load it so the function call might modify the contents of the address
		if (Mode != AccessMode.READ && !Trace.IsLoadingRequired(Unit, Result))
		{
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{
			Result.Value = new MemoryHandle(Unit, Start, Offset);
			Result.Format = Variable.Type!.Format;

			Memory.MoveToRegister(Unit, Result, Assembler.Size, Variable.GetRegisterFormat().IsDecimal(), Trace.GetDirectives(Unit, Result));
		}
		else
		{
			var address = new Result(ExpressionHandle.CreateMemoryAddress(Start, Offset), Assembler.Format);
			Memory.MoveToRegister(Unit, address, Assembler.Size, false, Trace.GetDirectives(Unit, Result));

			Result.Value = new MemoryHandle(Unit, address, 0);
			Result.Format = Variable.Type!.Format;
		}
	}
}