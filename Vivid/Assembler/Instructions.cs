using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StatusFlag
{
	public static class X64
	{
		public const int AF = 1;
		public const int CF = 2;
		public const int OF = 4;
		public const int PF = 8;
		public const int SF = 16;
		public const int ZF = 32;
	}

	public static class Arm64
	{
		public const int N = 1;
		public const int Z = 2;
		public const int C = 4;
		public const int V = 8;
	}
}

[SuppressMessage("Microsoft.Maintainability", "CA1034", Justification = "Nesting should be avoided but here it categorizes instructions well")]
public static class Instructions
{
	public static Dictionary<string, InstructionDescriptor> Descriptors { get; } = new Dictionary<string, InstructionDescriptor>();

	public static bool ModifiesFlags(Instruction instruction)
	{
		return Descriptors.TryGetValue(instruction.Operation, out InstructionDescriptor? descriptor) && descriptor.Flags != 0;
	}

	public static bool IsConditional(Instruction instruction)
	{
		return Descriptors.TryGetValue(instruction.Operation, out InstructionDescriptor? descriptor) && descriptor.IsConditional;
	}

	public class InstructionDescriptor
	{
		public short Flags { get; }
		public bool IsConditional { get; set; } = false;

		public InstructionDescriptor(params int[] flags)
		{
			Flags = (short)Flag.Combine(flags);
		}
	}

	public static class Shared
	{
		public const string COMPARE = "cmp";
		public const string ADD = "add";
		public const string AND = "and";
		public const string MOVE = "mov";
		public const string NEGATE = "neg";
		public const string SUBTRACT = "sub";
		public const string RETURN = "ret";
		public const string NOP = "nop";
	}

	public static class X64
	{
		public const int EVALUATE_MAX_MULTIPLIER = 8;

		public const string DOUBLE_PRECISION_ADD = "addsd";
		public const string SINGLE_PRECISION_ADD = "addss";

		public const string ATOMIC_EXCHANGE_ADD = "lock xadd";

		public const string CALL = "call";
		public const string EXTEND_DWORD = "cdq";
		public const string EXTEND_QWORD = "cqo";

		public const string DOUBLE_PRECISION_COMPARE = "comisd";
		public const string SINGLE_PRECISION_COMPARE = "comiss";
		public const string TEST = "test";

		public const string CONDITIONAL_MOVE_ABOVE = "cmova";
		public const string CONDITIONAL_MOVE_ABOVE_OR_EQUALS = "cmovae";
		public const string CONDITIONAL_MOVE_BELOW = "cmovb";
		public const string CONDITIONAL_MOVE_BELOW_OR_EQUALS = "cmovbe";
		public const string CONDITIONAL_MOVE_EQUALS = "cmove";
		public const string CONDITIONAL_MOVE_GREATER_THAN = "cmovg";
		public const string CONDITIONAL_MOVE_GREATER_THAN_OR_EQUALS = "cmovge";
		public const string CONDITIONAL_MOVE_LESS_THAN = "cmovl";
		public const string CONDITIONAL_MOVE_LESS_THAN_OR_EQUALS = "cmovle";
		public const string CONDITIONAL_MOVE_NOT_EQUALS = "cmovne";
		public const string CONDITIONAL_MOVE_NOT_ZERO = "cmovnz";
		public const string CONDITIONAL_MOVE_ZERO = "cmovz";

		public const string CONDITIONAL_SET_ABOVE = "seta";
		public const string CONDITIONAL_SET_ABOVE_OR_EQUALS = "setae";
		public const string CONDITIONAL_SET_BELOW = "setb";
		public const string CONDITIONAL_SET_BELOW_OR_EQUALS = "setbe";
		public const string CONDITIONAL_SET_EQUALS = "sete";
		public const string CONDITIONAL_SET_GREATER_THAN = "setg";
		public const string CONDITIONAL_SET_GREATER_THAN_OR_EQUALS = "setge";
		public const string CONDITIONAL_SET_LESS_THAN = "setl";
		public const string CONDITIONAL_SET_LESS_THAN_OR_EQUALS = "setle";
		public const string CONDITIONAL_SET_NOT_EQUALS = "setne";
		public const string CONDITIONAL_SET_NOT_ZERO = "setnz";
		public const string CONDITIONAL_SET_ZERO = "setz";

		public const string CONVERT_INTEGER_TO_DOUBLE_PRECISION = "cvtsi2sd";
		public const string CONVERT_INTEGER_TO_SINGLE_PRECISION = "cvtsi2ss";
		public const string CONVERT_DOUBLE_PRECISION_TO_INTEGER = "cvttsd2si";
		public const string CONVERT_SINGLE_PRECISION_TO_INTEGER = "cvttss2si";
		public const string DOUBLE_PRECISION_DIVIDE = "divsd";
		public const string SINGLE_PRECISION_DIVIDE = "divss";
		public const string SIGNED_DIVIDE = "idiv";
		public const string UNSIGNED_DIVIDE = "div";

		public const string JUMP_ABOVE = "ja";
		public const string JUMP_ABOVE_OR_EQUALS = "jae";
		public const string JUMP_BELOW = "jb";
		public const string JUMP_BELOW_OR_EQUALS = "jbe";
		public const string JUMP_EQUALS = "je";
		public const string JUMP_GREATER_THAN = "jg";
		public const string JUMP_GREATER_THAN_OR_EQUALS = "jge";
		public const string JUMP_LESS_THAN = "jl";
		public const string JUMP_LESS_THAN_OR_EQUALS = "jle";
		public const string JUMP = "jmp";
		public const string JUMP_NOT_EQUALS = "jne";
		public const string JUMP_NOT_ZERO = "jnz";
		public const string JUMP_ZERO = "jz";

		public const string EVALUATE = "lea";
		public const string DOUBLE_PRECISION_MOVE = "movsd";
		public const string SINGLE_PRECISION_MOVE = "movss";
		public const string SIGNED_CONVERSION_MOVE = "movsx";
		public const string SIGNED_DWORD_CONVERSION_MOVE = "movsxd";
		public const string UNALIGNED_XMMWORD_MOVE = "movups";
		public const string RAW_MEDIA_REGISTER_MOVE = "movq";
		public const string UNSIGNED_CONVERSION_MOVE = "movzx";
		public const string UNALIGNED_YMMWORD_MOVE = "vmovups";

		public const string UNSIGNED_MULTIPLY = "mul";
		public const string SIGNED_MULTIPLY = "imul";
		public const string DOUBLE_PRECISION_MULTIPLY = "mulsd";
		public const string SINGLE_PRECISION_MULTIPLY = "mulss";

		public const string NOT = "not";

		public const string OR = "or";

		public const string SHIFT_LEFT = "sal";
		public const string SHIFT_RIGHT = "sar";

		public const string DOUBLE_PRECISION_SUBTRACT = "subsd";
		public const string SINGLE_PRECISION_SUBTRACT = "subss";

		public const string XOR = "xor";
		public const string MEDIA_REGISTER_BITWISE_XOR = "pxor";
		public const string SINGLE_PRECISION_XOR = "xorps";
		public const string DOUBLE_PRECISION_XOR = "xorpd";

		public const string PUSH = "push";
		public const string POP = "pop";
		public const string EXCHANGE = "xchg";

		public static void Initialize()
		{
			if (Descriptors.Count > 0)
			{
				return;
			}

			Descriptors.Add(Shared.ADD, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));
			Descriptors.Add(Shared.AND, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // AF undefined

			Descriptors.Add(ATOMIC_EXCHANGE_ADD, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));

			Descriptors.Add(CALL, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));

			Descriptors.Add(Shared.COMPARE, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));
			Descriptors.Add(DOUBLE_PRECISION_COMPARE, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));
			Descriptors.Add(SINGLE_PRECISION_COMPARE, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));
			Descriptors.Add(TEST, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));

			Descriptors.Add(SIGNED_DIVIDE, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // All are undefined

			Descriptors.Add(UNSIGNED_MULTIPLY, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // SF, ZF, AF and PF undefined
			Descriptors.Add(SIGNED_MULTIPLY, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // SF, ZF, AF and PF undefined

			Descriptors.Add(OR, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // AF undefined

			Descriptors.Add(Shared.SUBTRACT, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));

			Descriptors.Add(SHIFT_LEFT, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // AF undefined
			Descriptors.Add(SHIFT_RIGHT, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // AF undefined

			Descriptors.Add(Shared.NEGATE, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF));

			Descriptors.Add(XOR, new(StatusFlag.X64.OF, StatusFlag.X64.SF, StatusFlag.X64.ZF, StatusFlag.X64.AF, StatusFlag.X64.CF, StatusFlag.X64.PF)); // AF undefined

			Descriptors.Add(CONDITIONAL_MOVE_ABOVE, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_ABOVE_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_BELOW, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_BELOW_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_GREATER_THAN, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_GREATER_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_LESS_THAN, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_LESS_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_NOT_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_NOT_ZERO, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_MOVE_ZERO, new() { IsConditional = true });

			Descriptors.Add(CONDITIONAL_SET_ABOVE, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_ABOVE_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_BELOW, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_BELOW_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_GREATER_THAN, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_GREATER_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_LESS_THAN, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_LESS_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_NOT_EQUALS, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_NOT_ZERO, new() { IsConditional = true });
			Descriptors.Add(CONDITIONAL_SET_ZERO, new() { IsConditional = true });

			Descriptors.Add(JUMP_ABOVE, new() { IsConditional = true });
			Descriptors.Add(JUMP_ABOVE_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_BELOW, new() { IsConditional = true });
			Descriptors.Add(JUMP_BELOW_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_GREATER_THAN, new() { IsConditional = true });
			Descriptors.Add(JUMP_GREATER_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_LESS_THAN, new() { IsConditional = true });
			Descriptors.Add(JUMP_LESS_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_NOT_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_NOT_ZERO, new() { IsConditional = true });
			Descriptors.Add(JUMP_ZERO, new() { IsConditional = true });

			JumpInstruction.Initialize();
			MoveInstruction.Initialize();
		}
	}

	public static class Arm64
	{
		public const string DECIMAL_ADD = "fadd";

		public const string XOR = "eor";
		public const string OR = "orr";

		public const string SHIFT_LEFT = "lsl";
		public const string SHIFT_RIGHT = "asr";

		public const string CALL_LABEL = "bl";
		public const string CALL_REGISTER = "blr";

		public const string DECIMAL_COMPARE = "fcmp";

		public const string SIGNED_DIVIDE = "sdiv";
		public const string UNSIGNED_DIVIDE = "udiv";
		public const string DECIMAL_DIVIDE = "fdiv";

		public const string MULTIPLY_SUBTRACT = "msub";

		public const string LOAD_RELATIVE_ADDRESS = "adrp";

		public const string STORE_REGISTER_PAIR = "stp";
		public const string LOAD_REGISTER_PAIR = "ldp";

		public const string JUMP_LABEL = "b";
		public const string JUMP_REGISTER = "br";

		public const string JUMP_GREATER_THAN = "b.gt";
		public const string JUMP_GREATER_THAN_OR_EQUALS = "b.ge";
		public const string JUMP_LESS_THAN = "b.lt";
		public const string JUMP_LESS_THAN_OR_EQUALS = "b.le";
		public const string JUMP_EQUALS = "b.eq";
		public const string JUMP_NOT_EQUALS = "b.ne";

		public const string LOAD_SHIFTED_CONSTANT = "movk";

		public const string CONDITIONAL_MOVE = "csel";

		public const string DECIMAL_MOVE = "fmov";

		public const string CONVERT_DECIMAL_TO_INTEGER = "fcvtzs";
		public const string CONVERT_INTEGER_TO_DECIMAL = "scvtf";

		public const string STORE = "str";
		public const string LOAD = "ldr";

		public const string LOAD_UINT8 = "ldrb";
		public const string LOAD_UINT16 = "ldrh";
		public const string LOAD_INT8 = "ldrsb";
		public const string LOAD_INT16 = "ldrsh";
		public const string LOAD_INT32 = "ldrsw";

		public const string STORE_UINT8 = "strb";
		public const string STORE_UINT16 = "strh";

		public const string CONVERT_INT8_TO_INT64 = "sxtb";
		public const string CONVERT_INT16_TO_INT64 = "sxth";
		public const string CONVERT_INT32_TO_INT64 = "sxtw";

		public const string SIGNED_MULTIPLY = "mul";
		public const string DECIMAL_MULTIPLY = "fmul";

		public const string DECIMAL_NEGATE = "fneg";
		public const string NOT = "mvn";

		public const string DECIMAL_SUBTRACT = "fsub";

		public static void Initialize()
		{
			if (Descriptors.Count > 0) return;

			Descriptors.Add(SHIFT_LEFT, new(StatusFlag.Arm64.C));
			Descriptors.Add(SHIFT_RIGHT, new(StatusFlag.Arm64.C));
			Descriptors.Add(CALL_LABEL, new(StatusFlag.Arm64.C, StatusFlag.Arm64.N, StatusFlag.Arm64.V, StatusFlag.Arm64.Z));
			Descriptors.Add(CALL_REGISTER, new(StatusFlag.Arm64.C, StatusFlag.Arm64.N, StatusFlag.Arm64.V, StatusFlag.Arm64.Z));

			Descriptors.Add(Shared.COMPARE, new(StatusFlag.Arm64.C, StatusFlag.Arm64.N, StatusFlag.Arm64.V, StatusFlag.Arm64.Z));
			Descriptors.Add(DECIMAL_COMPARE, new(StatusFlag.Arm64.C, StatusFlag.Arm64.N, StatusFlag.Arm64.V, StatusFlag.Arm64.Z));

			Descriptors.Add(CONDITIONAL_MOVE, new() { IsConditional = true });

			Descriptors.Add(JUMP_GREATER_THAN, new() { IsConditional = true });
			Descriptors.Add(JUMP_GREATER_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_LESS_THAN, new() { IsConditional = true });
			Descriptors.Add(JUMP_LESS_THAN_OR_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_EQUALS, new() { IsConditional = true });
			Descriptors.Add(JUMP_NOT_EQUALS, new() { IsConditional = true });

			JumpInstruction.Initialize();
			MoveInstruction.Initialize();
		}
	}
}