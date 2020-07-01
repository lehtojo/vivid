using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Globalization;

public class Lifetime
{
	public int Start { get; set; } = -1;
	public int End { get; set; } = -1;

	public void Reset()
	{
		Start = -1;
		End = -1;
	}
	
	/// <summary>
	/// Returns whether this lifetime is active, that is, whether the lifetime has started but not ended from the specified instruction position's perspective
	/// </summary>
	public bool IsActive(int position)
	{
		return position >= Start && (End == -1 || position <= End);
	}

	/// <summary>
	/// Returns true, if the lifetime is active and is not starting or ending at the specified instruction position, otherwise false
	/// </summary>
	public bool IsOnlyActive(int position)
	{
		return IsActive(position) && Start != position && End != position;
	}

	public bool IsIntersecting(int start, int end)
	{
		var s1 = Start == -1 ? int.MinValue : Start;
		var e1 = End == -1 ? int.MaxValue : End;

		var s2 = start == -1 ? int.MinValue : start;
		var e2 = end == -1 ? int.MaxValue : end;

		return s1 < e2 && e1 > s2;
	}

	public Lifetime Clone()
	{
		return new Lifetime()
		{
			Start = Start,
			End = End
		};
	}

	public override string ToString()
	{
		if (Start == -1 && End == -1)
		{
			return "static";
		}

		return (Start == -1 ? string.Empty : Start.ToString(CultureInfo.InvariantCulture)) + ".." + (End == -1 ? string.Empty : End.ToString(CultureInfo.InvariantCulture));
	}
}

public enum UnitPhase
{
	READ_ONLY_MODE,
	APPEND_MODE,
	BUILD_MODE
}

public class VariableState
{
	public Variable Variable { get; private set; }
	public Register Register { get; private set; }

	public VariableState(Variable variable, Register register)
	{
		Variable = variable;
		Register = register;
	}

	public void Restore(Unit unit)
	{
		if (Register.Handle != null)
		{
			throw new ApplicationException("During state restoration one of the registers was conflicted");
		}

		var current_handle = unit.GetCurrentVariableHandle(Variable);

		if (current_handle != null)
		{
			Register.Handle = current_handle;
			current_handle.Value = new RegisterHandle(Register);
		}
	}
}

public class Unit
{
	public FunctionImplementation Function { get; private set; }

	public List<Register> Registers { get; }
	public List<Register> NonVolatileRegisters { get; }
	public List<Register> VolatileRegisters { get; }
	public List<Register> NonReservedRegisters { get; }
	public List<Register> MediaRegisters { get; }

	public List<Instruction> Instructions { get; } = new List<Instruction>();

	private StringBuilder Builder { get; } = new StringBuilder();
	public Dictionary<LoopNode, SymmetryStartInstruction> Loops { get; } = new Dictionary<LoopNode, SymmetryStartInstruction>();
	public Dictionary<object, string> Constants { get; } = new Dictionary<object, string>();

	private int LabelIndex { get; set; }
	private int StringIndex { get; set; }
	private int ConstantIndex { get; set; }

	public Variable? Self { get; }

	private Instruction? Anchor { get; set; }
	public int Position { get; private set; } = -1;

	public int StackOffset 
	{
		get => Scope?.StackOffset ?? 0;
		set => Scope!.StackOffset = value;
	}

	public Scope? Scope { get; set; }
	private UnitPhase Phase { get; set; } = UnitPhase.READ_ONLY_MODE;

	public Unit(FunctionImplementation function)
	{
		Function = function;

		if (function.Metadata?.IsMember ?? false)
		{
			Self = function.GetVariable(global::Function.THIS_POINTER_IDENTIFIER) ?? throw new ApplicationException("Member function didn't have this pointer");
		}

		var is_non_volatile = Assembler.IsTargetWindows && Assembler.Size.Bits == 64;

		/* Registers = new List<Register>()
		{
			new Register(Size.QWORD, new string[] { "rax", "eax", "ax", "al" }, RegisterFlag.VOLATILE | RegisterFlag.RETURN | RegisterFlag.DENOMINATOR),
			new Register(Size.QWORD, new string[] { "rbx", "ebx", "bx", "bl" }),
			new Register(Size.QWORD, new string[] { "rcx", "ecx", "cx", "cl" }, RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new string[] { "rdx", "edx", "dx", "dl" }, RegisterFlag.VOLATILE | RegisterFlag.REMAINDER),
			new Register(Size.QWORD, new string[] { "rsp", "esp", "sp", "spl" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER),
			new Register(Size.QWORD, new string[] { "r8", "r8d", "r8w", "r8b" }, RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new string[] { "r9", "r9d", "r9w", "r9b" }, RegisterFlag.VOLATILE),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm0" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED | RegisterFlag.DECIMAL_RETURN),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm1" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm2" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED)
		}; */

		Registers = new List<Register>()
		{
			new Register(Size.QWORD, new [] { "rax", "eax", "ax", "al" }, RegisterFlag.VOLATILE | RegisterFlag.RETURN | RegisterFlag.DENOMINATOR),
			new Register(Size.QWORD, new [] { "rbx", "ebx", "bx", "bl" }),
			new Register(Size.QWORD, new [] { "rcx", "ecx", "cx", "cl" }, RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new [] { "rdx", "edx", "dx", "dl" }, RegisterFlag.VOLATILE | RegisterFlag.REMAINDER),
			new Register(Size.QWORD, new [] { "rsi", "esi", "si", "sil" }, is_non_volatile ? RegisterFlag.NONE : RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new [] { "rdi", "edi", "di", "dil" }, is_non_volatile ? RegisterFlag.NONE : RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new [] { "rbp", "ebp", "bp", "bpl" }),
			new Register(Size.QWORD, new [] { "rsp", "esp", "sp", "spl" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER),

			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm0" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED | RegisterFlag.DECIMAL_RETURN),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm1" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm2" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm3" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm4" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm5" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm6" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm7" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm8" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm9" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm10" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm11" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm12" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm13" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm14" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.FromFormat(Types.DECIMAL.Format), new string[] { "xmm15" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED)
		};

		if (Assembler.Size == Size.QWORD)
		{
			Registers.AddRange(new List<Register>()
			{
				new Register(Size.QWORD, new [] { "r8", "r8d", "r8w", "r8b" }, RegisterFlag.VOLATILE),
				new Register(Size.QWORD, new [] { "r9", "r9d", "r9w", "r9b" }, RegisterFlag.VOLATILE),
				new Register(Size.QWORD, new [] { "r10", "r10d", "r10w", "r10b" }, RegisterFlag.VOLATILE),
				new Register(Size.QWORD, new [] { "r11", "r11d", "r11w", "r11b" }, RegisterFlag.VOLATILE),
				new Register(Size.QWORD, new [] { "r12", "r12d", "r12w", "r12b" }),
				new Register(Size.QWORD, new [] { "r13", "r13d", "r13w", "r13b" }),
				new Register(Size.QWORD, new [] { "r14", "r14d", "r14w", "r14b" }),
				new Register(Size.QWORD, new [] { "r15", "r15d", "r15w", "r15b" })
			});
		}

		NonVolatileRegisters = Registers.FindAll(r => !r.IsVolatile && !r.IsReserved);
		VolatileRegisters = Registers.FindAll(r => r.IsVolatile);

		NonReservedRegisters = VolatileRegisters.FindAll(r => !r.IsReserved);
		NonReservedRegisters.AddRange(NonVolatileRegisters.FindAll(r => !r.IsReserved));

		MediaRegisters = Registers.FindAll(r => r.IsMediaRegister);
	}

	public void ExpectMode(UnitPhase expected)
	{
		if (Phase != expected)
		{
			throw new InvalidOperationException("Unit mode didn't match the expected");
		}
	}

	/// <summary>
	/// Returns all variables currently inside registers that are needed at the given instruction position or later
	/// </summary>
	public List<VariableState> GetState(int at)
	{
		return Registers
			.FindAll(register => !register.IsAvailable(at) && (register.Handle?.Metadata.IsVariable ?? false))
			.Select(register => new VariableState(register.Handle!.Metadata.Variables.First().Variable, register)).ToList();
	}

	public void Set(List<VariableState> state)
	{
		// Reset all registers
		Registers.ForEach(r => r.Reset());

		// Restore all the variables to their own registers
		state.ForEach(s => s.Restore(this));
	}

	public void Append(Instruction instruction, bool after = false)
	{
		if (Position < 0)
		{
			Instructions.Add(instruction);
			instruction.Position = Instructions.Count - 1;
		}
		else
		{
			// Find the last instruction with the current position since there can be many
			var position = Instructions.IndexOf(Anchor!);

			if (position == -1)
			{
				position = Position;
			}

			if (after)
			{
				position++;
			}

			Instructions.Insert(position, instruction);
			instruction.Position = Position;

			Reindex();
		}

		instruction.Scope = Scope;
		instruction.Result.Lifetime.Start = instruction.Position;

		if (Phase != UnitPhase.BUILD_MODE) return;
		
		var previous = Anchor;

		Anchor = instruction;
		Position = Anchor.Position;
		instruction.Build();
		instruction.OnSimulate();
		Anchor = previous;
		Position = Anchor!.Position;
	}

	public void Append(IEnumerable<Instruction> instructions, bool after = false)
	{
		if (Phase != UnitPhase.BUILD_MODE)
		{
			throw new ApplicationException("Appending a range of instructions only works in build mode since it uses reindexing");
		}

		if (Position < 0)
		{
			Instructions.AddRange(instructions);
		}
		else
		{
			// Find the last instruction with the current position since there can be many
			var position = Instructions.IndexOf(Anchor!);

			if (position == -1)
			{
				position = Position;
			}

			if (after)
			{
				position++;
			}

			Instructions.InsertRange(position, instructions);
		}

		foreach (var instruction in instructions)
		{
			instruction.Scope = Scope;
		}

		Reindex();	

		if (Phase == UnitPhase.BUILD_MODE)
		{
			var previous = Anchor;

			foreach (var instruction in instructions)
			{
				Anchor = instruction;
				Position = Anchor.Position;
				instruction.Build();
				instruction.OnSimulate();
			}

			Anchor = previous;
			Position = Anchor!.Position;
		}
	}

	public void Write(string instruction)
	{
		ExpectMode(UnitPhase.BUILD_MODE);

		Builder.Append(instruction);
		Builder.AppendLine();
	}

	public RegisterHandle? TryGetCached(Result handle)
	{
		var register = Registers
			.Find(r => !r.IsMediaRegister && (r.Handle != null) && r.Handle.Value == handle.Value);

		if (register != null)
		{
			return new RegisterHandle(register);
		}

		return null;
	}

	public RegisterHandle? TryGetCachedMediaRegister(Result handle)
	{
		var register = MediaRegisters.Find(r => (r.Handle != null) && r.Handle.Value == handle.Value);

		return register != null ? new RegisterHandle(register) : null;
	}

	public void Release(Register register)
	{
		var value = register.Handle;

		if (value == null)
		{
			return;
		}

		if (value.IsReleasable())
		{
			// Get all the variables that this value represents
			foreach (var attribute in value.Metadata.Variables)
			{
				var destination = new Result(References.CreateVariableHandle(this, null, attribute.Variable), attribute.Variable.Type!.Format);

				var move = new MoveInstruction(this, destination, value)
				{
					Description = "Releases the source value to memory",
					Type = MoveType.RELOCATE
				};

				Append(move);
			}
		}
		else
		{
			var destination = new Result(new TemporaryMemoryHandle(this), value.Format);
			var move = new MoveInstruction(this, destination, value)
			{
				Description = "Releases an unregistered value to a temporary memory location",
				Type = MoveType.RELOCATE
			};

         Append(move);
		}

		// Now the register is ready for use
		register.Reset();
	}

	public Label GetNextLabel()
	{
		return new Label(Function.Metadata!.GetFullname() + $"_L{LabelIndex++}");
	}

	public string GetNextString()
	{
		return Function.Metadata!.GetFullname() + $"_S{StringIndex++}";
	}

	public string GetNextConstantIdentifier(object constant)
	{
		if (Constants.TryGetValue(constant, out string? identifier))
		{
			return identifier;
		}

		identifier = Function.Metadata!.GetFullname() + $"_C{ConstantIndex++}";

		Constants.Add(constant, identifier);
		return identifier;
	}

	public Register? GetNextNonVolatileRegister(int start, int end)
	{
		var register = NonVolatileRegisters
			.Find(r => (r.Handle == null || !r.Handle.Lifetime.IsIntersecting(start, end)) && !r.IsReserved);

		return register;
	}

	public Register? GetNextNonVolatileRegister(bool release = true)
	{
		var register = NonVolatileRegisters
			.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null || !release)
		{
			return register;
		}

		register = NonVolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			Release(register);
			return register;
		}

		return null;
	}

	public Register? GetNextRegisterWithoutReleasing()
	{
		var register = VolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		return register ?? NonVolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);
	}

	/// <summary>
	/// Retrieves the next available register, releasing a register to memory if necessary
	/// </summary>
	public Register GetNextRegister()
	{
		// Try to find the next fully available volatile register
		var register = VolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			return register;
		}

		// Try to find the next fully available non-volatile register
		register = NonVolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			return register;
		}

		// Try to find the next volatile register which contains a value that has a corresponding memory location
		register = VolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Try to find the next volatile register which contains a value that has a corresponding memory location
		register = NonVolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Since all registers contain intermediate values, one of them must be released a temporary memory location
		// NOTE: Some registers may be locked which prevents them from being used, but not all registers should be locked, otherwise something very strange has happened

		// Find the next register which is not locked
		register = NonReservedRegisters.Find(r => !r.IsReserved && !r.IsLocked && !(Function.Returns && r.IsReturnRegister));

		if (register == null)
		{
			throw new ApplicationException("All registers were locked or reserved, this should not happen");
		}

		Release(register);

		return register;
	}

	public Register GetNextMediaRegister()
	{
		var register = MediaRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister));

		if (register != null)
		{
			return register;
		}

		register = MediaRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister));

		if (register != null)
		{
			Release(register);
			return register;
		}

		throw new NotImplementedException("Couldn't find an available media register");
	}

	public Register? GetNextMediaRegisterWithoutReleasing()
	{
		return MediaRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister));
	}

	public Register GetStackPointer()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.STACK_POINTER)) ?? throw new Exception("Architecture didn't have stack pointer register?");
	}

	public Register GetStandardReturnRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.RETURN)) ?? throw new Exception("Architecture didn't have a standard return register?");
	}

	public Register GetDecimalReturnRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.DECIMAL_RETURN)) ?? throw new Exception("Architecture didn't have a decimal return register?");
	}

	public void Reset()
	{
		Registers.ForEach(r => r.Reset(true));
	}

	public void Execute(UnitPhase mode, Action action)
	{
		Phase = mode;
		Position = -1;

		try
		{
			action();
		}
		catch (Exception e)
		{
			throw new ApplicationException($"ERROR: Unit execution failed: {e}");
		}

		Phase = UnitPhase.READ_ONLY_MODE;
	}

	public void Simulate(UnitPhase mode, Action<Instruction> action)
	{
		Phase = mode;
		Position = 0;
		StackOffset = 0;
		Scope = null;

		Reset();

		for (Position = 0; Position < Instructions.Count;)
		{
			var instruction = Instructions[Position];
			var next = Position + 1 < Instructions.Count ? Instructions[Position + 1] : null;

			try
			{
				if (instruction.Scope == null)
				{
					throw new ApplicationException("Instruction was missing its scope");
				}

				Anchor = instruction;

				if (Scope != instruction.Scope)
				{
					var back = false;

					// Detect if the program is exiting the current scope
					if (Scope?.Outer == instruction.Scope)
					{
						Scope?.Exit();
						back = true;
					}

					// Scope enter function is designed for entering not for falling back
					if (back)
					{
						Scope = instruction.Scope;
					}
					else
					{
						instruction.Scope.Enter(this);
					}
				}

				action(instruction);

				instruction.OnSimulate();

				// Simulate the stack size change
				StackOffset += instruction.GetStackOffsetChange();
			}
			catch (Exception e)
			{
				var name = Enum.GetName(typeof(InstructionType), instruction.Type);

				throw new ApplicationException($"ERROR: Unit simulation failed while processing {name}-instruction at position {Position}: {e}");
			}

			if (next == null)
			{
				break;
			}

			Position = Instructions.IndexOf(next, instruction.Position);

			if (Position == -1)
			{
				throw new ApplicationException("Next instruction was removed from the instruction list");
			}
		}

		// Reset the state after this simulation
		Phase = UnitPhase.READ_ONLY_MODE;
	}

	public void Cache(object constant, Result value)
	{
		var handles = GetConstantHandles(constant);
		value.Lifetime.Start = Position;

		handles.Add(value);
	}

	public List<Result> GetConstantHandles(object constant)
	{
		return Scope?.GetConstantHandles(constant) ?? throw new ApplicationException("Couldn't get constant reference list");
	}

	public Result? GetCurrentVariableHandle(Variable variable)
	{
		return Scope?.GetCurrentVariableHandle(variable);
	}

	public Result? GetCurrentConstantHandle(object constant)
	{
		var handles = GetConstantHandles(constant).FindAll(h => h.IsValid(Position));
		return handles.Count == 0 ? null : handles.Last();
	}

	public string Export()
	{
		return Builder.ToString();
	}

	private void Reindex()
	{
		Position = 0;

		// Reindex all instructions
		foreach (var instruction in Instructions)
		{
			instruction.Position = Position++;
		}

		// Reset all lifetimes
		foreach (var result in Instructions.SelectMany(instruction => instruction.GetAllUsedResults()))
		{
			result.Lifetime.Reset();
		}

		// Calculate lifetimes
		for (var i = 0; i < Instructions.Count; i++)
		{
			var instruction = Instructions[i];

			foreach (var result in instruction.GetAllUsedResults())
			{
				result.Use(i);
			}
		}
	}
}