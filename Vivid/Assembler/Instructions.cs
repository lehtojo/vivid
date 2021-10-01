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
		public const byte RAX = 0;
		public const byte RCX = 1;
		public const byte RDX = 2;
		public const byte RBX = 3;
		public const byte RSP = 4;
		public const byte RBP = 5;
		public const byte RSI = 6;
		public const byte RDI = 7;
		public const byte R8 = 8;
		public const byte R9 = 9;
		public const byte R10 = 10;
		public const byte R11 = 11;
		public const byte R12 = 12;
		public const byte R13 = 13;
		public const byte R14 = 14;
		public const byte R15 = 15;

		public static List<List<InstructionEncoding>> ParameterlessEncodings { get; set; } = new List<List<InstructionEncoding>>();
		public static List<List<InstructionEncoding>> SingleParameterEncodings { get; set; } = new List<List<InstructionEncoding>>();
		public static List<List<InstructionEncoding>> DualParameterEncodings { get; set; } = new List<List<InstructionEncoding>>();
		public static List<List<InstructionEncoding>> TripleParameterEncodings { get; set; } = new List<List<InstructionEncoding>>();

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
		public const string SHIFT_RIGHT_UNSIGNED = "shr";

		public const string DOUBLE_PRECISION_SUBTRACT = "subsd";
		public const string SINGLE_PRECISION_SUBTRACT = "subss";

		public const string XOR = "xor";
		public const string MEDIA_REGISTER_BITWISE_XOR = "pxor";
		public const string SINGLE_PRECISION_XOR = "xorps";
		public const string DOUBLE_PRECISION_XOR = "xorpd";

		public const string PUSH = "push";
		public const string POP = "pop";
		public const string EXCHANGE = "xchg";


		// Parameterless instructions
		public const int _RETURN = 0;
		public const int _LABEL = 1;
		public const int _CQO = 2;
		public const int _MAX_PARAMETERLESS_INSTRUCTIONS = 3;

		// Single parameter instructions
		public const int _PUSH = 0;
		public const int _POP = 1;
		public const int _JA = 2;
		public const int _JAE = 3;
		public const int _JB = 4;
		public const int _JBE = 5;
		public const int _JE = 6;
		public const int _JG = 7;
		public const int _JGE = 8;
		public const int _JL = 9;
		public const int _JLE = 10;
		public const int _JMP = 11;
		public const int _JNE = 12;
		public const int _JNZ = 13;
		public const int _JZ = 14;
		public const int _CALL = 15;
		public const int _MAX_SINGLE_PARAMETER_INSTRUCTIONS = 16;

		// Dual parameter instructions
		public const int _MOVE = 0;
		public const int _ADD = 1;
		public const int _SUBTRACT = 2;
		public const int _SIGNED_MULTIPLY = 3;
		public const int _UNSIGNED_MULTIPLY = 4;
		public const int _SIGNED_DIVIDE = 5;
		public const int _UNSIGNED_DIVIDE = 6;
		public const int _SHIFT_LEFT = 7;
		public const int _SHIFT_RIGHT = 8;
		public const int _MOVZX = 9;
		public const int _MOVSX = 10;
		public const int _MOVSXD = 11;
		public const int _LEA = 12;
		public const int _CMP = 13;
		public const int _ADDSD = 14;
		public const int _SUBSD = 15;
		public const int _MULSD = 16;
		public const int _DIVSD = 17;
		public const int _MOVSD = 18;
		public const int _MOVQ = 19;
		public const int _CVTSI2SD = 20;
		public const int _CVTTSD2SI = 21;
		public const int _AND = 22;
		public const int _XOR = 23;
		public const int _OR = 24;
		public const int _COMISD = 25;
		public const int _TEST = 26;
		public const int _MAX_DUAL_PARAMETER_INSTRUCTIONS = 27;

		// Triple parameter instructions
		public const int _MAX_TRIPLE_PARAMETER_INSTRUCTIONS = 0;

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

			ParameterlessEncodings = new List<List<InstructionEncoding>>();
			SingleParameterEncodings = new List<List<InstructionEncoding>>();
			DualParameterEncodings = new List<List<InstructionEncoding>>();
			TripleParameterEncodings = new List<List<InstructionEncoding>>();

			for (var i = 0; i < _MAX_PARAMETERLESS_INSTRUCTIONS; i++) { ParameterlessEncodings.Add(new List<InstructionEncoding>()); }
			for (var i = 0; i < _MAX_SINGLE_PARAMETER_INSTRUCTIONS; i++) { SingleParameterEncodings.Add(new List<InstructionEncoding>()); }
			for (var i = 0; i < _MAX_DUAL_PARAMETER_INSTRUCTIONS; i++) { DualParameterEncodings.Add(new List<InstructionEncoding>()); }
			for (var i = 0; i < _MAX_TRIPLE_PARAMETER_INSTRUCTIONS; i++) { TripleParameterEncodings.Add(new List<InstructionEncoding>()); }

			ParameterlessEncodings[_RETURN] = new List<InstructionEncoding>()
			{
				// ret
				new InstructionEncoding(0xC3),
			};

			ParameterlessEncodings[_LABEL] = new List<InstructionEncoding>()
			{
				new InstructionEncoding(0x00, EncodingRoute.L),
			};

			ParameterlessEncodings[_CQO] = new List<InstructionEncoding>()
			{
				new InstructionEncoding(0x99, EncodingRoute.NONE, true),
			};

			SingleParameterEncodings[_PUSH] = new List<InstructionEncoding>()
			{
				// push r64, push r16
				new InstructionEncoding(0x50, 0, EncodingRoute.O, false, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x50, 0, EncodingRoute.O, false, EncodingFilterType.REGISTER, 0, 8),
			};

			SingleParameterEncodings[_POP] = new List<InstructionEncoding>()
			{
				// pop r64, pop r16
				new InstructionEncoding(0x58, 0, EncodingRoute.O, false, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x58, 0, EncodingRoute.O, false, EncodingFilterType.REGISTER, 0, 8),
			};

			SingleParameterEncodings[_JA] = new List<InstructionEncoding>()  { new InstructionEncoding(0x770F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JAE] = new List<InstructionEncoding>() { new InstructionEncoding(0x730F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JB] = new List<InstructionEncoding>()  { new InstructionEncoding(0x720F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JBE] = new List<InstructionEncoding>() { new InstructionEncoding(0x760F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JE] = new List<InstructionEncoding>()  { new InstructionEncoding(0x740F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JG] = new List<InstructionEncoding>()  { new InstructionEncoding(0x7F0F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JGE] = new List<InstructionEncoding>() { new InstructionEncoding(0x7D0F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JL] = new List<InstructionEncoding>()  { new InstructionEncoding(0x7C0F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JLE] = new List<InstructionEncoding>() { new InstructionEncoding(0x7E0F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JNE] = new List<InstructionEncoding>() { new InstructionEncoding(0x750F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JNZ] = new List<InstructionEncoding>() { new InstructionEncoding(0x750F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_JZ] = new List<InstructionEncoding>()  { new InstructionEncoding(0x740F, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };

			SingleParameterEncodings[_JMP] = new List<InstructionEncoding>() { new InstructionEncoding(0xE9, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };
			SingleParameterEncodings[_CALL] = new List<InstructionEncoding>()  { new InstructionEncoding(0xE8, 0, EncodingRoute.D, false, EncodingFilterType.LABEL, 0, 8) };

			DualParameterEncodings[_MOVE] = new List<InstructionEncoding>()
			{
				// mov r64, r64 | mov r32, r32 | mov r16, r16 | mov r8, r8
				new InstructionEncoding(0x88, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x89, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x89, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x89, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// mov m64, r64 | mov m32, r32 | mov m16, r16 | mov m8, r8
				new InstructionEncoding(0x88, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x89, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x89, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x89, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// mov r64, m64 | mov r32, m32 | mov r16, m16 | mov r8, m8
				new InstructionEncoding(0x8A, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0x8B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x8B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0x8B, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8),

				// mov r64, c32
				new InstructionEncoding(0xC7, 0, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// mov r64, c64 | mov r32, c32 | mov r16, c16 | mov r8, c8
				new InstructionEncoding(0xB0, 0, EncodingRoute.OC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xB8, 0, EncodingRoute.OC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xB8, 0, EncodingRoute.OC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xB8, 0, EncodingRoute.OC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 8),

				// mov m64, c32 | mov m32, c32 | mov m16, c16 | mov m8, c8
				new InstructionEncoding(0xC6, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xC7, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xC7, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xC7, 0, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),
			};

			DualParameterEncodings[_ADD] = new List<InstructionEncoding>()
			{
				// add r64, c8 | add r32, c8 | add r16, c8
				new InstructionEncoding(0x83, 0, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 0, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 0, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// add rax, c32 | add eax, c32 | add ax, c16 | add al, c8
				new InstructionEncoding(0x04, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x05, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x05, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x05, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// add r64, c32 | add r32, c32 | add r16, c16 | add r8, c8
				new InstructionEncoding(0x81, 0, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 0, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 0, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 0, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// add m64, c32 | add m32, c32 | add m16, c16 | add m8, c8
				new InstructionEncoding(0x81, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 0, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 0, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// add r64, r64 | add r32, r32 | add r16, r16 | add r8, r8
				new InstructionEncoding(0x00, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x01, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x01, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x01, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// add m64, r64 | add m32, r32 | add m16, r16 | add m8, r8
				new InstructionEncoding(0x00, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x01, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x01, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x01, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),
			};

			DualParameterEncodings[_SUBTRACT] = new List<InstructionEncoding>()
			{
				// add r64, c8 | add r32, c8 | add r16, c8
				new InstructionEncoding(0x83, 5, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 5, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 5, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// sub rax, c32 | sub eax, c32 | sub ax, c16 | sub al, c8
				new InstructionEncoding(0x2C, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x2D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x2D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x2D, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// sub r64, c32 | sub r32, c32 | sub r16, c16 | sub r8, c8
				new InstructionEncoding(0x81, 5, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 5, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 5, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 5, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// sub m64, c32 | sub m32, c32 | sub m16, c16 | sub m8, c8
				new InstructionEncoding(0x81, 5, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 5, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 5, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 5, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// sub r64, r64 | sub r32, r32 | sub r16, r16 | sub r8, r8
				new InstructionEncoding(0x28, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x29, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x29, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x29, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// sub m64, r64 | sub m32, r32 | sub m16, r16 | sub m8, r8
				new InstructionEncoding(0x28, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x29, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x29, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x29, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),
			};

			DualParameterEncodings[_SIGNED_MULTIPLY] = new List<InstructionEncoding>()
			{
				// imul r64 | imul r32 | imul r16 | imul r8
				new InstructionEncoding(0xF6, 5, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0xF7, 5, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 5, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0xF7, 5, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8),

				// imul m64 | imul m32 | imul m16 | imul m8
				new InstructionEncoding(0xF6, 5, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0xF7, 5, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 5, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0xF7, 5, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8),

				// imul r64, m64 | imul r32, m32 | imul r16, m16
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// imul r64, m64 | imul r32, m32 | imul r16, m16
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xAF0F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8),
			};

			DualParameterEncodings[_UNSIGNED_MULTIPLY] = new List<InstructionEncoding>()
			{
				// mul r64 | mul r32 | mul r16 | mul r8
				new InstructionEncoding(0xF6, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0xF7, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0xF7, 4, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8),

				// mul m64 | mul m32 | mul m16 | mul m8
				new InstructionEncoding(0xF6, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0xF7, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0xF7, 4, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8),
			};

			DualParameterEncodings[_SIGNED_DIVIDE] = new List<InstructionEncoding>()
			{
				// idiv r64 | idiv r32 | idiv r16 | idiv r8
				new InstructionEncoding(0xF6, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0xF7, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0xF7, 7, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8),

				// idiv m64 | idiv m32 | idiv m16 | idiv m8
				new InstructionEncoding(0xF6, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0xF7, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xF7, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0xF7, 7, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8),
			};

			DualParameterEncodings[_SHIFT_LEFT] = new List<InstructionEncoding>()
			{
				// sal r64, 1 | sal r32, 1 | sal r16, 1 | sal r8, 1
				new InstructionEncoding(0xD0, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.SPECIFIC_CONSTANT, 1, 1),
				new InstructionEncoding(0xD1, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.SPECIFIC_CONSTANT, 1, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD1, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.SPECIFIC_CONSTANT, 1, 4),
				new InstructionEncoding(0xD1, 4, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.SPECIFIC_CONSTANT, 1, 8),

				// sal m64, 1 | sal m32, 1 | sal m16, 1 | sal m8, 1
				new InstructionEncoding(0xD0, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.SPECIFIC_CONSTANT, 1, 1),
				new InstructionEncoding(0xD1, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.SPECIFIC_CONSTANT, 1, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD1, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.SPECIFIC_CONSTANT, 1, 4),
				new InstructionEncoding(0xD1, 4, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.SPECIFIC_CONSTANT, 1, 8),

				// sal r64, c8 | sal r32, c8 | sal r16, c8 | sal r8, c8
				new InstructionEncoding(0xC0, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xC1, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xC1, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xC1, 4, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 8),

				// sal m64, c8 | sal m32, c8 | sal m16, c8 | sal m8, c8
				new InstructionEncoding(0xC0, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xC1, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xC1, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xC1, 4, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 8),

				// sal r64, cl | sal r32, cl | sal r16, cl | sal r8, cl
				new InstructionEncoding(0xD2, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 1),
				new InstructionEncoding(0xD3, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD3, 4, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 4),
				new InstructionEncoding(0xD3, 4, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 8),

				// sal m64, cl | sal m32, cl | sal m16, cl | sal m8, cl
				new InstructionEncoding(0xD2, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 1),
				new InstructionEncoding(0xD3, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD3, 4, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 4),
				new InstructionEncoding(0xD3, 4, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 8),
			};

			DualParameterEncodings[_SHIFT_RIGHT] = new List<InstructionEncoding>()
			{
				// sar r64, 1 | sar r32, 1 | sar r16, 1 | sar r8, 1
				new InstructionEncoding(0xD0, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.SPECIFIC_CONSTANT, 1, 1),
				new InstructionEncoding(0xD1, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.SPECIFIC_CONSTANT, 1, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD1, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.SPECIFIC_CONSTANT, 1, 4),
				new InstructionEncoding(0xD1, 7, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.SPECIFIC_CONSTANT, 1, 8),

				// sar m64, 1 | sar m32, 1 | sar m16, 1 | sar m8, 1
				new InstructionEncoding(0xD0, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.SPECIFIC_CONSTANT, 1, 1),
				new InstructionEncoding(0xD1, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.SPECIFIC_CONSTANT, 1, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD1, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.SPECIFIC_CONSTANT, 1, 4),
				new InstructionEncoding(0xD1, 7, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.SPECIFIC_CONSTANT, 1, 8),

				// sar r64, c8 | sar r32, c8 | sar r16, c8 | sar r8, c8
				new InstructionEncoding(0xC0, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xC1, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xC1, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xC1, 7, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 8),

				// sar m64, c8 | sar m32, c8 | sar m16, c8 | sar m8, c8
				new InstructionEncoding(0xC0, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0xC1, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xC1, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0xC1, 7, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 8),

				// sar r64, cl | sar r32, cl | sar r16, cl | sar r8, cl
				new InstructionEncoding(0xD2, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 1),
				new InstructionEncoding(0xD3, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD3, 7, EncodingRoute.R, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 4),
				new InstructionEncoding(0xD3, 7, EncodingRoute.R, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 8),

				// sar m64, cl | sar m32, cl | sar m16, cl | sar m8, cl
				new InstructionEncoding(0xD2, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 1),
				new InstructionEncoding(0xD3, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0xD3, 7, EncodingRoute.M, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 4),
				new InstructionEncoding(0xD3, 7, EncodingRoute.M, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RCX, 8),
			};

			DualParameterEncodings[_MOVZX] = new List<InstructionEncoding>()
			{
				// movzx r16, r8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 1, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// movzx r16, m8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// movzx r32, r8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 1),

				// movzx r32, m8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 1),

				// movzx r64, r8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 1),

				// movzx r64, m8
				new InstructionEncoding(0xB60F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 1),

				// movzx r32, r16
				new InstructionEncoding(0xB70F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 2),

				// movzx r32, m16
				new InstructionEncoding(0xB70F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 2),

				// movzx r64, r16
				new InstructionEncoding(0xB70F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 2),

				// movzx r64, m16
				new InstructionEncoding(0xB70F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
			};

			DualParameterEncodings[_MOVSX] = new List<InstructionEncoding>()
			{
				// movsx r16, r8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 1, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// movsx r16, m8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// movsx r32, r8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 1),

				// movsx r32, m8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 1),

				// movsx r64, r8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 1),

				// movsx r64, m8
				new InstructionEncoding(0xBE0F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 1),

				// movsx r32, r16
				new InstructionEncoding(0xBF0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 2),

				// movsx r32, m16
				new InstructionEncoding(0xBF0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 2),

				// movsx r64, r16
				new InstructionEncoding(0xBF0F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 2),

				// movsx r64, m16
				new InstructionEncoding(0xBF0F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
			};

			DualParameterEncodings[_MOVSXD] = new List<InstructionEncoding>()
			{
				// movsxd r64, r32
				new InstructionEncoding(0x63, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 4),

				// movsxd r64, m32
				new InstructionEncoding(0x63, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
			};

			DualParameterEncodings[_LEA] = new List<InstructionEncoding>()
			{
				// lea r16, e
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.EXPRESSION, 0, 8, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// lea r32, e
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.EXPRESSION, 0, 8),

				// lea r64, e
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.EXPRESSION, 0, 8),

				// lea r16, m16
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncoderX64.OPERAND_SIZE_OVERRIDE),

				// lea r32, m32
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 8),

				// lea r64, m64
				new InstructionEncoding(0x8D, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8),
			};

			DualParameterEncodings[_CMP] = new List<InstructionEncoding>()
			{
				// cmp rax, c32, cmp eax, c32, cmp ax, c16, cmp al, c8
				new InstructionEncoding(0x3C, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x3D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x3D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x3D, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// cmp r64, c32, cmp r32, c32, cmp r16, c16, cmp r8, c8
				new InstructionEncoding(0x80, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 7, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 7, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// cmp m64, c32, cmp m32, c32, cmp m16, c16, cmp m8, c8
				new InstructionEncoding(0x80, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x81, 7, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 7, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// cmp r64, r64, cmp r32, r32, cmp r16, r16, cmp r8, r8
				new InstructionEncoding(0x38, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x39, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x39, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x39, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// cmp m64, r64, cmp m32, r32, cmp m16, r16, cmp m8, r8
				new InstructionEncoding(0x38, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x39, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x39, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x39, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// cmp r64, m64, cmp r32, m32, cmp r16, m16, cmp r8, m8
				new InstructionEncoding(0x3A, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0x3B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncoderX64.OPERAND_SIZE_OVERRIDE),
				new InstructionEncoding(0x3B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0x3B, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8),
			};

			DualParameterEncodings[_ADDSD] = new List<InstructionEncoding>()
			{
				// addsd x, x
				new InstructionEncoding(0x580F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// addsd x, m64
				new InstructionEncoding(0x580F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_SUBSD] = new List<InstructionEncoding>()
			{
				// subsd x, x
				new InstructionEncoding(0x5C0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// subsd x, m64
				new InstructionEncoding(0x5C0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_MULSD] = new List<InstructionEncoding>()
			{
				// mulsd x, x
				new InstructionEncoding(0x590F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// mulsd x, m64
				new InstructionEncoding(0x590F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_DIVSD] = new List<InstructionEncoding>()
			{
				// divsd x, x
				new InstructionEncoding(0x5E0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// divsd x, m64
				new InstructionEncoding(0x5E0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_MOVSD] = new List<InstructionEncoding>()
			{
				// movsd x, x
				new InstructionEncoding(0x100F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// movsd x, m64
				new InstructionEncoding(0x100F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),

				// movsd m64, x
				new InstructionEncoding(0x110F, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),
			};

			DualParameterEncodings[_MOVQ] = new List<InstructionEncoding>()
			{
				// movq x, r64
				new InstructionEncoding(0x7E0F, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF3),

				// movq x, m64
				new InstructionEncoding(0x7E0F, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF3),
			};

			DualParameterEncodings[_CVTSI2SD] = new List<InstructionEncoding>()
			{
				// cvtsi2sd x, r64
				new InstructionEncoding(0x2A0F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// cvtsi2sd x, m64
				new InstructionEncoding(0x2A0F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_CVTTSD2SI] = new List<InstructionEncoding>()
			{
				// cvttsd2si r, x
				new InstructionEncoding(0x2C0F, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8, 0xF2),

				// cvttsd2si r, m64
				new InstructionEncoding(0x2C0F, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8, 0xF2),
			};

			DualParameterEncodings[_AND] = new List<InstructionEncoding>()
			{
				// and rax, c32 | and eax, c32 | and ax, c16 | and al, c8
				new InstructionEncoding(0x24, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x25, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x25, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x25, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// and r64, c8 | and r32, c8 | and r16, c8
				new InstructionEncoding(0x83, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 4, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// and m64, c8 | and m32, c8 | and m16, c8
				new InstructionEncoding(0x83, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 4, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// and r64, c32 | and r32, c32 | and r16, c16 | and r8, c8
				new InstructionEncoding(0x80, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 4, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 4, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// and m64, c32 | and m32, c32 | and m16, c16 | and m8, c8
				new InstructionEncoding(0x80, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 4, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 4, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// and r64, r64 | and r32, r32 | and r16, r16 | and r8, r8
				new InstructionEncoding(0x20, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x21, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x21, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x21, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// and m64, r64 | and m32, r32 | and m16, r16 | and m8, r8
				new InstructionEncoding(0x20, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x21, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x21, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x21, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// and r64, m64 | and r32, m32 | and r16, m16 | and r8, m8
				new InstructionEncoding(0x22, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0x23, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
				new InstructionEncoding(0x23, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0x23, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8)
			};

			DualParameterEncodings[_XOR] = new List<InstructionEncoding>()
			{
				// xor rax, c32 | xor eax, c32 | xor ax, c16 | xor al, c8
				new InstructionEncoding(0x34, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x35, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x35, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x35, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// xor r64, c8 | xor r32, c8 | xor r16, c8
				new InstructionEncoding(0x83, 6, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 6, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 6, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// xor m64, c8 | xor m32, c8 | xor m16, c8
				new InstructionEncoding(0x83, 6, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 6, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 6, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// xor r64, c32 | xor r32, c32 | xor r16, c16 | xor r8, c8
				new InstructionEncoding(0x80, 6, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 6, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 6, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 6, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// xor m64, c32 | xor m32, c32 | xor m16, c16 | xor m8, c8
				new InstructionEncoding(0x80, 6, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 6, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 6, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 6, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// xor r64, r64 | xor r32, r32 | xor r16, r16 | xor r8, r8
				new InstructionEncoding(0x30, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x31, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x31, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x31, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// xor m64, r64 | xor m32, r32 | xor m16, r16 | xor m8, r8
				new InstructionEncoding(0x30, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x31, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x31, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x31, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// xor r64, m64 | xor r32, m32 | xor r16, m16 | xor r8, m8
				new InstructionEncoding(0x32, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0x33, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
				new InstructionEncoding(0x33, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0x33, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8)
			};

			DualParameterEncodings[_OR] = new List<InstructionEncoding>()
			{
				// or rax, c32 | or eax, c32 | or ax, c16 | or al, c8
				new InstructionEncoding(0x0C, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x0D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x0D, 0, EncodingRoute.C, false, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x0D, 0, EncodingRoute.C, true, EncodingFilterType.SPECIFIC_REGISTER, Instructions.X64.RAX, 8, EncodingFilterType.CONSTANT, 0, 4),

				// or r64, c8 | or r32, c8 | or r16, c8
				new InstructionEncoding(0x83, 1, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 1, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 1, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// or m64, c8 | or m32, c8 | or m16, c8
				new InstructionEncoding(0x83, 1, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 1, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x83, 1, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 1),

				// or r64, c32 | or r32, c32 | or r16, c16 | or r8, c8
				new InstructionEncoding(0x80, 1, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 1, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 1, EncodingRoute.RC, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 1, EncodingRoute.RC, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// or m64, c32 | or m32, c32 | or m16, c16 | or m8, c8
				new InstructionEncoding(0x80, 1, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.CONSTANT, 0, 1),
				new InstructionEncoding(0x81, 1, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.CONSTANT, 0, 2),
				new InstructionEncoding(0x81, 1, EncodingRoute.MC, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.CONSTANT, 0, 4),
				new InstructionEncoding(0x81, 1, EncodingRoute.MC, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.CONSTANT, 0, 4),

				// or r64, r64 | or r32, r32 | or r16, r16 | or r8, r8
				new InstructionEncoding(0x08, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x09, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x09, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x09, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// or m64, r64 | or m32, r32 | or m16, r16 | or m8, r8
				new InstructionEncoding(0x08, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x09, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x09, 0, EncodingRoute.MR, false, EncodingFilterType.MEMORY_ADDRESS, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x09, 0, EncodingRoute.MR, true, EncodingFilterType.MEMORY_ADDRESS, 0, 8, EncodingFilterType.REGISTER, 0, 8),

				// or r64, m64 | or r32, m32 | or r16, m16 | or r8, m8
				new InstructionEncoding(0x0A, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.MEMORY_ADDRESS, 0, 1),
				new InstructionEncoding(0x0B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.MEMORY_ADDRESS, 0, 2),
				new InstructionEncoding(0x0B, 0, EncodingRoute.RM, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.MEMORY_ADDRESS, 0, 4),
				new InstructionEncoding(0x0B, 0, EncodingRoute.RM, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.MEMORY_ADDRESS, 0, 8)
			};

			DualParameterEncodings[_TEST] = new List<InstructionEncoding>()
			{
				// test r64, r64 | test r32, r32 | test r16, r16 | test r8, r8
				new InstructionEncoding(0x84, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 1, EncodingFilterType.REGISTER, 0, 1),
				new InstructionEncoding(0x85, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 2, EncodingFilterType.REGISTER, 0, 2),
				new InstructionEncoding(0x85, 0, EncodingRoute.RR, false, EncodingFilterType.REGISTER, 0, 4, EncodingFilterType.REGISTER, 0, 4),
				new InstructionEncoding(0x85, 0, EncodingRoute.RR, true, EncodingFilterType.REGISTER, 0, 8, EncodingFilterType.REGISTER, 0, 8),
			};
		}
	}

	public static class Arm64
	{
		public const string DECIMAL_ADD = "fadd";

		public const string XOR = "eor";
		public const string OR = "orr";

		public const string SHIFT_LEFT = "lsl";
		public const string SHIFT_RIGHT = "asr";
		public const string SHIFT_RIGHT_UNSIGNED = "lsr";

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