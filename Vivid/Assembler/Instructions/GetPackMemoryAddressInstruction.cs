using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Returns a handle for accessing raw memory from packs
/// This instruction works on all architectures
/// </summary>
public class GetPackMemoryAddressInstruction : Instruction
{
	public Dictionary<Variable, Result> Values { get; private set; } = new Dictionary<Variable, Result>();
	public Type Element { get; private set; }
	public AccessMode Mode { get; private set; }

	public Result Start { get; private set; }
	public Result Offset { get; private set; }
	public int Stride { get; private set; }

	public GetPackMemoryAddressInstruction(Unit unit, Type element, AccessMode mode, Result start, Result offset, int stride) : base(unit, InstructionType.GET_PACK_MEMORY_ADDRESS)
	{
		Element = element;
		Mode = mode;
		Start = start;
		Offset = offset;
		Stride = stride;
		IsAbstract = true;

		if (Assembler.IsArm64)
		{
			Start = new Result(ExpressionHandle.CreateMemoryAddress(start, offset, stride), Assembler.Format);
			Stride = 1;
		}
		else if (stride > Instructions.X64.EVALUATE_MAX_MULTIPLIER)
		{
			Offset = new MultiplicationInstruction(Unit, offset, new Result(new ConstantHandle((long)stride), Assembler.Format), Assembler.Format, false).Execute();
			Stride = 1;
		}
		else
		{
			Offset = offset;
			Stride = stride;
		}

		// Create the results for the pack members
		foreach (var member in element.Variables.Values)
		{
			Values.Add(member, new Result());
		}

		Dependencies = new[] { Result, Start, Offset }.Concat(Values.Values).ToArray();
		
		BuildPackHandle();
	}

	private void ValidateHandle()
	{
		// Ensure the start value is a constant or in a register
		if (Start.IsConstant || Start.IsInline || Start.IsStandardRegister) return;
		Memory.MoveToRegister(Unit, Start, Assembler.Size, false, Trace.GetDirectives(Unit, Start));
	}

	private void BuildPackHandle()
	{
		foreach (var member in Element.Variables.Values)
		{
			var result = Values[member];
			var alignment = member.GetAlignment(Element) ?? throw new ApplicationException("Missing member alignment");

			if (Assembler.IsArm64)
			{
				result.Value = new ComplexMemoryHandle(Start, new Result(new ConstantHandle((long)alignment), Assembler.Format), Stride);
				result.Format = member.Type!.Format;
			}
			else
			{
				result.Value = new ComplexMemoryHandle(Start, Offset, Stride, alignment);
				result.Format = member.Type!.Format;
			}
		}

		Result.Value = new DisposablePackHandle(Values);
		Result.Format = Assembler.Format;
	}

	public override void OnBuild()
	{
		ValidateHandle();
		BuildPackHandle();

		if (!Trace.IsLoadingRequired(Unit, Result)) return;

		if (Mode == AccessMode.READ)
		{
			// If loading is required, load the values in registers, if they are stored in memory
			foreach (var value in Values.Values)
			{
				if (value.IsConstant) continue;
				Memory.MoveToRegister(Unit, value, Assembler.Size, value.Format.IsDecimal(), Trace.GetDirectives(Unit, value));
			}
		}
	}
}