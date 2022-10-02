using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum UnitMode
{
	NONE,
	ADD,
	BUILD
}

/// <summary>
/// Keeps track of labels, strings and constants by generating an index for every requester
/// </summary>
public class Indexer
{
	public const string LAMBDA = "Lambda";
	public const string CONSTANT = "Constant";
	public const string CONTEXT = "Context";
	public const string HIDDEN = "Hidden";
	public const string LABEL = "Label";
	public const string SECTION = "Section";
	public const string STRING = "String";
	public const string UNIT = "Unit";
	public const string STACK = "Stack";
	public const string SCOPE = "Scope";

	private Dictionary<string, int> Indices { get; set; } = new Dictionary<string, int>();

	public int Next(string category)
	{
		var value = Indices.GetValueOrDefault(category, 0);
		Indices[category] = value + 1;
		return value;
	}

	public int this[string category] => Next(category);
}

public struct VariableState
{
	public Variable Variable { get; }
	public Handle Handle { get; }

	public VariableState(Variable variable, Result result)
	{
		Variable = variable;
		Handle = result.Value.Finalize();
		Handle.Format = result.Format;
	}
}

public class Unit
{
	public FunctionImplementation Function { get; private set; }
	public Scope? Scope { get; set; }
	private Indexer Indexer { get; set; } = new Indexer();
	public Variable? Self { get; set; }

	public List<Register> Registers { get; } = new();
	public List<Register> StandardRegisters { get; }
	public List<Register> MediaRegisters { get; }
	public List<Register> VolatileRegisters { get; }
	public List<Register> VolatileStandardRegisters { get; }
	public List<Register> VolatileMediaRegisters { get; }
	public List<Register> NonVolatileRegisters { get; }
	public List<Register> NonVolatileStandardRegisters { get; }
	public List<Register> NonVolatileMediaRegisters { get; }
	public List<Register> NonReservedRegisters { get; }

	public List<Instruction> Instructions { get; } = new();
	public Dictionary<string, List<VariableState>> States { get; } = new();
	public Instruction? Anchor { get; set; }
	public int Position { get; set; } = -1;
	public int StackOffset { get; set; } = 0;
	public StringBuilder Builder { get; } = new();
	public UnitMode Mode { get; set; } = UnitMode.NONE;

	// All scopes indexed by their id
	public Dictionary<string, Scope> Scopes { get; private set; } = new();

	// List of scopes (value) that enter the scope indicated by the id of a scope (key)
	public Dictionary<string, List<Scope>> Arrivals { get; private set; } = new();

	public Unit(FunctionImplementation function)
	{
		Function = function;
		Self = Function.GetSelfPointer();

		if (Assembler.IsX64)
		{
			LoadArchitectureX64();
		}
		else
		{
			LoadArchitectureArm64();
		}

		StandardRegisters = Registers.FindAll(i => !i.IsMediaRegister && !i.IsReserved);
		MediaRegisters = Registers.FindAll(i => i.IsMediaRegister && !i.IsReserved);

		VolatileRegisters = Registers.FindAll(i => i.IsVolatile && !i.IsReserved);
		VolatileStandardRegisters = VolatileRegisters.FindAll(i => !i.IsMediaRegister);
		VolatileMediaRegisters = VolatileRegisters.FindAll(i => i.IsMediaRegister);

		NonVolatileRegisters = Registers.FindAll(i => !i.IsVolatile && !i.IsReserved);
		NonVolatileStandardRegisters = NonVolatileRegisters.FindAll(i => !i.IsMediaRegister);
		NonVolatileMediaRegisters = NonVolatileRegisters.FindAll(i => i.IsMediaRegister);

		NonReservedRegisters = VolatileRegisters.FindAll(i => !i.IsReserved);
		NonReservedRegisters.AddRange(NonVolatileRegisters.FindAll(i => !i.IsReserved));
	}

	public Unit()
	{
		Function = null!;
		Registers = new List<Register>();

		if (Assembler.IsX64)
		{
			LoadArchitectureX64();
		}
		else
		{
			LoadArchitectureArm64();
		}

		StandardRegisters = Registers.FindAll(i => !i.IsMediaRegister && !i.IsReserved);
		MediaRegisters = Registers.FindAll(i => i.IsMediaRegister && !i.IsReserved);

		VolatileRegisters = Registers.FindAll(i => i.IsVolatile && !i.IsReserved);
		VolatileStandardRegisters = VolatileRegisters.FindAll(i => !i.IsMediaRegister);
		VolatileMediaRegisters = VolatileRegisters.FindAll(i => i.IsMediaRegister);

		NonVolatileRegisters = Registers.FindAll(i => !i.IsVolatile && !i.IsReserved);
		NonVolatileStandardRegisters = NonVolatileRegisters.FindAll(i => !i.IsMediaRegister);
		NonVolatileMediaRegisters = NonVolatileRegisters.FindAll(i => i.IsMediaRegister);

		NonReservedRegisters = VolatileRegisters.FindAll(i => !i.IsReserved);
		NonReservedRegisters.AddRange(NonVolatileRegisters.FindAll(i => !i.IsReserved));
	}

	private void LoadArchitectureX64()
	{
		var volatility_flag = Assembler.IsTargetWindows ? RegisterFlag.NONE : RegisterFlag.VOLATILE;

		Registers.AddRange(new List<Register>()
		{
			new Register(global::Instructions.X64.RAX, Size.QWORD, new [] { "rax", "eax", "ax", "al" }, RegisterFlag.VOLATILE | RegisterFlag.RETURN | RegisterFlag.NUMERATOR),
			new Register(global::Instructions.X64.RBX, Size.QWORD, new [] { "rbx", "ebx", "bx", "bl" }),
			new Register(global::Instructions.X64.RCX, Size.QWORD, new [] { "rcx", "ecx", "cx", "cl" }, RegisterFlag.VOLATILE | RegisterFlag.SHIFT),
			new Register(global::Instructions.X64.RDX, Size.QWORD, new [] { "rdx", "edx", "dx", "dl" }, RegisterFlag.VOLATILE | RegisterFlag.REMAINDER),
			new Register(global::Instructions.X64.RSI, Size.QWORD, new [] { "rsi", "esi", "si", "sil" }, volatility_flag),
			new Register(global::Instructions.X64.RDI, Size.QWORD, new [] { "rdi", "edi", "di", "dil" }, volatility_flag),
			new Register(global::Instructions.X64.RBP, Size.QWORD, new [] { "rbp", "ebp", "bp", "bpl" }, RegisterFlag.NONE),
			new Register(global::Instructions.X64.RSP, Size.QWORD, new [] { "rsp", "esp", "sp", "spl" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER),

			new Register(global::Instructions.X64.YMM0, Size.YWORD, new [] { "ymm0", "xmm0", "xmm0", "xmm0", "xmm0" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.DECIMAL_RETURN),
			new Register(global::Instructions.X64.YMM1, Size.YWORD, new [] { "ymm1", "xmm1", "xmm1", "xmm1", "xmm1" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM2, Size.YWORD, new [] { "ymm2", "xmm2", "xmm2", "xmm2", "xmm2" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM3, Size.YWORD, new [] { "ymm3", "xmm3", "xmm3", "xmm3", "xmm3" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM4, Size.YWORD, new [] { "ymm4", "xmm4", "xmm4", "xmm4", "xmm4" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM5, Size.YWORD, new [] { "ymm5", "xmm5", "xmm5", "xmm5", "xmm5" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM6, Size.YWORD, new [] { "ymm6", "xmm6", "xmm6", "xmm6", "xmm6" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM7, Size.YWORD, new [] { "ymm7", "xmm7", "xmm7", "xmm7", "xmm7" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM8, Size.YWORD, new [] { "ymm8", "xmm8", "xmm8", "xmm8", "xmm8" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM9, Size.YWORD, new [] { "ymm9", "xmm9", "xmm9", "xmm9", "xmm9" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM10, Size.YWORD, new [] { "ymm10", "xmm10", "xmm10", "xmm10", "xmm10" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM11, Size.YWORD, new [] { "ymm11", "xmm11", "xmm11", "xmm11", "xmm11" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM12, Size.YWORD, new [] { "ymm12", "xmm12", "xmm12", "xmm12", "xmm12" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM13, Size.YWORD, new [] { "ymm13", "xmm13", "xmm13", "xmm13", "xmm13" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM14, Size.YWORD, new [] { "ymm14", "xmm14", "xmm14", "xmm14", "xmm14" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.YMM15, Size.YWORD, new [] { "ymm15", "xmm15", "xmm15", "xmm15", "xmm15" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE),

			new Register(global::Instructions.X64.R8, Size.QWORD, new [] { "r8", "r8d", "r8w", "r8b" }, RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.R9, Size.QWORD, new [] { "r9", "r9d", "r9w", "r9b" }, RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.R10, Size.QWORD, new [] { "r10", "r10d", "r10w", "r10b" }, RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.R11, Size.QWORD, new [] { "r11", "r11d", "r11w", "r11b" }, RegisterFlag.VOLATILE),
			new Register(global::Instructions.X64.R12, Size.QWORD, new [] { "r12", "r12d", "r12w", "r12b" }),
			new Register(global::Instructions.X64.R13, Size.QWORD, new [] { "r13", "r13d", "r13w", "r13b" }),
			new Register(global::Instructions.X64.R14, Size.QWORD, new [] { "r14", "r14d", "r14w", "r14b" }),
			new Register(global::Instructions.X64.R15, Size.QWORD, new [] { "r15", "r15d", "r15w", "r15b" })
		});
	}

	private void LoadArchitectureArm64()
	{
		for (var i = 0; i < 29; i++)
		{
			var register = new Register(Size.QWORD, new[] { $"x{i}", $"w{i}", $"w{i}", $"w{i}" });

			if (i < 19)
			{
				register.Flags |= RegisterFlag.VOLATILE;
			}

			if (i == 0)
			{
				register.Flags |= RegisterFlag.RETURN;
			}

			Registers.Add(register);
		}

		for (var i = 0; i < 29; i++)
		{
			var register = new Register(Size.XWORD, new[] { $"d{i}", $"d{i}", $"d{i}", $"d{i}", $"d{i}" }, RegisterFlag.MEDIA);

			if (i < 19)
			{
				register.Flags |= RegisterFlag.VOLATILE;
			}

			if (i == 0)
			{
				register.Flags |= RegisterFlag.DECIMAL_RETURN;
			}

			Registers.Add(register);
		}

		Registers.Add(new Register(Size.QWORD, new[] { "x30", "w30", "w30", "w30" }, RegisterFlag.RESERVED | RegisterFlag.RETURN_ADDRESS));
		Registers.Add(new Register(Size.QWORD, new[] { "xzr", "wzr", "wzr", "wzr" }, RegisterFlag.RESERVED | RegisterFlag.ZERO | RegisterFlag.VOLATILE));
		Registers.Add(new Register(Size.QWORD, new[] { "sp", "sp", "sp", "sp" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER));
	}

	/// <summary>
	/// Requests a value for the specified variable from all scopes that arrive to the specified scope.
	/// Returns the input value for the specified variable.
	/// </summary>
	public Result RequireVariableFromArrivals(Variable variable, Scope scope)
	{
		// If we end up here, it means the specified scope does not have a value for the specified variable.
		// We need to require the variable from all scopes that arrive to specified one:
		var input = scope.AddInput(variable);

		if (Arrivals.ContainsKey(scope.Id))
		{
			foreach (var arrival in Arrivals[scope.Id])
			{
				RequireVariable(variable, arrival);
			}
		}

		return input;
	}

	/// <summary>
	/// Requests a value for the specified variable from the specified scope.
	/// If the specified scope does not have a value for the specified variable, it will be required from all scopes that arrive to it.
	/// </summary>
	public void RequireVariable(Variable variable, Scope scope)
	{
		// If the variable is already outputted, no need to do anything
		if (scope.Outputs.ContainsKey(variable)) return;

		// If the scope assigns a value for the specified variable, we can output it from the scope
		// In other words, no need to require the variable from other scopes entering the specified scope, since it has its own value for the variable
		if (scope.Variables.ContainsKey(variable))
		{
			scope.AddOutput(variable, scope.Variables[variable]);
			return;
		}

		var input = RequireVariableFromArrivals(variable, scope);
		scope.AddOutput(variable, input);
	}

	// Summary: Tries to return the current value of the specified variable
	public Result GetVariableValue(Variable variable)
	{
		if (Scope == null) throw new ApplicationException("Missing scope");

		// If the current scope has a value for the specified variable, we can return it
		if (Scope.Variables.ContainsKey(variable))
		{
			return Scope.Variables[variable];
		}

		if (Mode == UnitMode.BUILD) throw new ApplicationException("Can not require variable from other scopes in build mode");

		return RequireVariableFromArrivals(variable, Scope);
	}

	public void AddArrival(string id, Scope scope)
	{
		if (Arrivals.ContainsKey(id))
		{
			Arrivals[id].Add(scope);
		}
		else
		{
			Arrivals[id] = new List<Scope> { scope };
		}
	}

	public void Add(JumpInstruction instruction)
	{
		var is_conditional = instruction.IsConditional;
		var destination_scope_id = instruction.Label.Name;
		var next_scope_id = GetNextScope();
		var current_scope = Scope ?? throw new ApplicationException("Missing scope");

		// Arrive to the destination scope from the current scope
		AddArrival(destination_scope_id, current_scope);

		// Merge with the next scope as well, if we can fall through
		if (is_conditional)
		{
			Add(new LabelMergeInstruction(this, destination_scope_id, next_scope_id));
		}
		else
		{
			Add(new LabelMergeInstruction(this, destination_scope_id));
		}

		Add(instruction, false);

		var next_scope = new Scope(this, next_scope_id);
		Add(new EnterScopeInstruction(this, next_scope_id));

		// If we can fall through the jump instruction, the current scope can arrive to the next scope
		if (is_conditional)
		{
			AddArrival(next_scope.Id, current_scope);
		}
	}

	public void Add(ReturnInstruction instruction)
	{
		Add(instruction, false);

		var next_scope_id = GetNextScope();
		var next_scope = new Scope(this, next_scope_id);
		Add(new EnterScopeInstruction(this, next_scope_id));
	}

	public void Add(LabelInstruction instruction)
	{
		var next_scope_id = instruction.Label.Name;
		var previous_scope = Scope ?? throw new ApplicationException("Missing scope");

		// Merge with the next scope, since we are falling through
		Add(new LabelMergeInstruction(this, next_scope_id));

		// Create the next scope
		var next_scope = new Scope(this, next_scope_id);
		Add(instruction, false);
		Add(new EnterScopeInstruction(this, next_scope_id));

		// Arrive to the next scope from the previous scope
		AddArrival(next_scope_id, previous_scope);
	}

	public void Add(Instruction instruction)
	{
		if (instruction.Type == InstructionType.JUMP) Add((JumpInstruction)instruction);
		else if (instruction.Type == InstructionType.LABEL) Add((LabelInstruction)instruction);
		else if (instruction.Type == InstructionType.RETURN) Add((ReturnInstruction)instruction);
		else Add(instruction, false);
	}

	public void Add(Instruction instruction, bool after)
	{
		if (after && (instruction.Type == InstructionType.JUMP || instruction.Type == InstructionType.LABEL || instruction.Type == InstructionType.RETURN))
		{
			throw new ApplicationException("Can not add the instruction after the current instruction");
		}

		if (Mode == UnitMode.ADD)
		{
			Instructions.Add(instruction);
			Anchor = instruction;
		}
		else if (after)
		{
			// TODO: Instructions could form a linked list?
			Instructions.Insert(Position + 1, instruction);
		}
		else
		{
			Instructions.Insert(Position, instruction);
		}

		instruction.Reindex();

		instruction.Scope = Scope;
		instruction.Result.Use(instruction);

		if (Mode != UnitMode.BUILD || after) return;

		var destination = Anchor;
		Anchor = instruction;

		instruction.Build();

		// Return to the previous instruction by iterating forward, since it must be ahead
		while (!ReferenceEquals(Anchor, destination))
		{
			if (Anchor.State != InstructionState.BUILT)
			{
				var iterator = Anchor;
				iterator.Build();
			}

			Anchor = Instructions[++Position];
		}
	}

	public void Write(string instruction)
	{
		Builder.Append(instruction);
		Builder.AppendLine();
	}

	/// <summary>
	/// Moves the value of the specified register to memory
	/// </summary>
	public Register Release(Register register)
	{
		var value = register.Value;
		if (value == null) { return register; }

		if (value.IsReleasable(this))
		{
			foreach (var iterator in Scope!.Variables)
			{
				if (!iterator.Value.Equals(value)) continue;

				// Get the default handle of the variable
				var handle = References.CreateVariableHandle(this, iterator.Key);

				// The handle must be a memory handle, otherwise anything can happen
				if (!handle.Is(HandleType.MEMORY)) { handle = new TemporaryMemoryHandle(this); }

				var destination = new Result(handle, iterator.Key.Type!.Format);

				var instruction = new MoveInstruction(this, destination, value)
				{
					Description = $"Releases the value representing '{iterator.Key}' to memory",
					Type = MoveType.RELOCATE
				};

				Add(instruction);
				break;
			}
		}
		else
		{
			var destination = new Result(new TemporaryMemoryHandle(this), value.Format);
			var instruction = new MoveInstruction(this, destination, value)
			{
				Description = "Releases an unregistered value to a temporary memory location",
				Type = MoveType.RELOCATE
			};

			Add(instruction);
		}

		// Now the register is ready for use
		register.Reset();
		return register;
	}

	public Register? GetNextNonVolatileRegister(bool media_register, bool release = true)
	{
		var register = NonVolatileRegisters.Find(i => i.IsAvailable() && i.IsMediaRegister == media_register);

		if (register != null || !release)
		{
			return register;
		}

		register = NonVolatileRegisters.Find(i => i.IsReleasable(this) && i.IsMediaRegister == media_register);

		if (register != null)
		{
			Release(register);
			return register;
		}

		return null;
	}

	/// <summary>
	/// Retrieves the next available register, releasing a register to memory if necessary
	/// </summary>
	public Register GetNextRegister()
	{
		// Try to find the next fully available volatile register
		var register = VolatileStandardRegisters.Find(i => i.IsAvailable());

		if (register != null)
		{
			return register;
		}

		// Try to find the next fully available non-volatile register
		register = NonVolatileStandardRegisters.Find(i => i.IsAvailable());

		if (register != null)
		{
			return register;
		}

		// Try to find the next volatile register which contains a value that has a corresponding memory location
		register = VolatileStandardRegisters.Find(i => i.IsReleasable(this));

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Try to find the next volatile register which contains a value that has a corresponding memory location
		register = NonVolatileStandardRegisters.Find(i => i.IsReleasable(this));

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Since all registers contain intermediate values, one of them must be released to a temporary memory location
		// NOTE: Some registers may be locked which prevents them from being used, but not all registers should be locked, otherwise something very strange has happened

		// Find the next register which is not locked
		register = StandardRegisters.Find(i => !i.IsLocked);

		if (register == null)
		{
			// NOTE: This usually happens when there is a flaw in the algorithm and the compiler does not know how to handle a value for example
			throw new ApplicationException("All registers were locked or reserved, this should not happen");
		}

		Release(register);

		return register;
	}

	/// <summary>
	/// Retrieves the next available media register, releasing a media register to memory if necessary
	/// </summary>
	public Register GetNextMediaRegister()
	{
		var register = MediaRegisters.Find(i => i.IsAvailable());

		if (register != null)
		{
			return register;
		}

		register = MediaRegisters.Find(i => i.IsReleasable(this));

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Find the next media register which is not locked
		register = MediaRegisters.Find(i => !i.IsLocked);

		if (register == null)
		{
			// NOTE: This usually happens when there is a flaw in the algorithm and the compiler does not know how to handle a value for example
			throw new ApplicationException("All media registers were locked or reserved, this should not happen");
		}

		Release(register);

		return register;
	}

	/// <summary>
	/// Tries to get the next available register without releasing a register to memory
	/// </summary>
	public Register? GetNextRegisterWithoutReleasing()
	{
		var register = VolatileStandardRegisters.Find(i => i.IsAvailable());

		return register ?? NonVolatileStandardRegisters.Find(i => i.IsAvailable());
	}

	/// <summary>
	/// Tries to get the next available register without releasing a register to memory (excludes the specified registers from the search)
	/// </summary>
	public Register? GetNextRegisterWithoutReleasing(List<Register> exclude)
	{
		var register = VolatileStandardRegisters.Where(i => !exclude.Contains(i)).ToList().Find(i => i.IsAvailable());

		return register ?? NonVolatileStandardRegisters.Where(i => !exclude.Contains(i)).ToList().Find(i => i.IsAvailable());
	}

	/// <summary>
	/// Tries to get the next available media register without releasing a register to memory
	/// </summary>
	public Register? GetNextMediaRegisterWithoutReleasing()
	{
		return MediaRegisters.Find(i => i.IsAvailable());
	}

	/// <summary>
	/// Tries to get the next available media register without releasing a register to memory (excludes the specified registers from the search)
	/// </summary>
	public Register? GetNextMediaRegisterWithoutReleasing(List<Register> exclude)
	{
		return MediaRegisters.Where(i => !exclude.Contains(i)).ToList().Find(i => i.IsAvailable());
	}

	public string GetNextString()
	{
		return Function.GetFullname() + $"_S{Indexer[Indexer.STRING]}";
	}

	public Label GetNextLabel()
	{
		return new Label(Function.GetFullname() + $"_L{Indexer[Indexer.LABEL]}");
	}

	public string GetNextConstant()
	{
		return Function.GetFullname() + $"_C{Indexer[Indexer.CONSTANT]}";;
	}

	public string GetNextIdentity()
	{
		return Indexer.UNIT.ToLowerInvariant() + '.' + Function.Identity + '.' + Indexer[Indexer.UNIT];
	}

	public string GetNextScope()
	{
		return Indexer[Indexer.SCOPE].ToString();
	}

	public Register GetStackPointer()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.STACK_POINTER)) ?? throw new Exception("Architecture did not have stack pointer register");
	}

	public Register GetBasePointer()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.BASE_POINTER)) ?? throw new Exception("Architecture did not have base pointer register");
	}

	public Register GetStandardReturnRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.RETURN)) ?? throw new Exception("Architecture did not have a standard return register");
	}

	public Register GetDecimalReturnRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.DECIMAL_RETURN)) ?? throw new Exception("Architecture did not have a decimal return register");
	}

	public Register GetNumeratorRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.NUMERATOR)) ?? throw new ApplicationException("Architecture did not have a numerator register");
	}

	public Register GetRemainderRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.REMAINDER)) ?? throw new ApplicationException("Architecture did not have a remainder register");
	}

	public Register GetShiftRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.SHIFT)) ?? throw new ApplicationException("Architecture did not have a shift register");
	}

	public Register GetZeroRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.ZERO)) ?? throw new ApplicationException("Architecture did not have a zero register");
	}

	public Register GetReturnAddressRegister()
	{
		return Registers.Find(i => Flag.Has(i.Flags, RegisterFlag.RETURN_ADDRESS)) ?? throw new ApplicationException("Architecture did not have a return address register");
	}

	/// <summary>
	/// Returns whether a value has been assigned to the specified variable
	/// </summary>
	public bool IsInitialized(Variable variable)
	{
		return Scope != null && Scope.Variables.ContainsKey(variable);
	}

	/// <summary>
	/// Updates the value of the specified variable in the current scope
	/// </summary>
	public void SetVariableValue(Variable variable, Result value)
	{
		if (Scope == null) throw new ApplicationException("Unit did not have an active scope");
		Scope!.Variables[variable] = value;
	}

	/// <summary>
	/// Returns whether any variables owns the specified value
	/// </summary>
	public bool IsVariableValue(Result result)
	{
		if (Scope == null) return false;
		foreach (var iterator in Scope.Variables) { if (iterator.Value == result) return true; }
		return false;
	}

	/// <summary>
	/// Returns the variable which owns the specified value, if it is owned by any
	/// </summary>
	public Variable? GetValueOwner(Result value)
	{
		if (Scope == null) return null;

		foreach (var iterator in Scope.Variables)
		{
			if (iterator.Value == value) return iterator.Key;
		}

		return null;
	}

	public bool AddDebugPosition(Node node)
	{
		return AddDebugPosition(node.Position);
	}

	public bool AddDebugPosition(Position? position)
	{
		if (!Assembler.IsDebuggingEnabled) return true;
		if (position == null) return false;

		Add(new DebugBreakInstruction(this, position));

		return true;
	}

	public void Reset()
	{
		Registers.ForEach(i => i.Reset());
	}

	public override string ToString()
	{
		return Builder.ToString();
	}
}