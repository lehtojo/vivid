using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Returns a handle for accessing member variables from packs
/// This instruction works on all architectures
/// </summary>
public class GetPackObjectPointerInstruction : Instruction
{
	public Dictionary<Variable, Result> Values { get; private set; } = new Dictionary<Variable, Result>();
	public Variable Variable { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; private set; }
	public AccessMode Mode { get; private set; }

	public GetPackObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset, AccessMode mode) : base(unit, InstructionType.GET_PACK_OBJECT_POINTER)
	{
		Variable = variable;
		Start = start;
		Offset = offset;
		Mode = mode;
		IsAbstract = true;
		
		// Create the results for the pack members
		foreach (var member in Variable.Type!.Variables.Values)
		{
			Values.Add(member, new Result());
		}

		Dependencies = new[] { Result, Start }.Concat(Values.Values).ToArray();
		
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
		foreach (var member in Variable.Type!.Variables.Values)
		{
			var result = Values[member];
			var alignment = member.GetAlignment(Variable.Type!) ?? throw new ApplicationException("Missing member alignment");

			result.Value = new ComplexMemoryHandle(Start, new Result(new ConstantHandle((long)(Offset + alignment)), Assembler.Format), 1);
			result.Format = member.Type!.Format;
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