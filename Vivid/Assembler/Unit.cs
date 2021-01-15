using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

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
		var handle = unit.GetCurrentVariableHandle(Variable);

		if (Register.Handle != null && !Register.Handle.Equals(handle))
		{
			unit.Scope!.Variables.Remove(Variable);
			return;
		}

		if (handle != null)
		{
			Register.Handle = handle;
			handle.Value = new RegisterHandle(Register);
		}
	}
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

	private Dictionary<string, int> Indices { get; set; } = new Dictionary<string, int>();

	public int Next(string category)
	{
		var value = Indices.GetValueOrDefault(category, 0);
		Indices[category] = value + 1;
		return value;
	}

	public int this[string category]
	{
		get => Next(category);
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
	public Dictionary<string, SymmetryStartInstruction> Loops { get; } = new Dictionary<string, SymmetryStartInstruction>();
	public Dictionary<object, string> Constants { get; } = new Dictionary<object, string>();

	private Indexer Indexer { get; set; } = new Indexer();
	public Variable? Self { get; set; }

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
		Self = Function.GetSelfPointer();

		Registers = new List<Register>();

		if (Assembler.IsX64)
		{
			LoadArhitectureX64();
		}
		else
		{
			LoadArhitectureArm64();
		}

		NonVolatileRegisters = Registers.FindAll(r => !r.IsVolatile && !r.IsReserved);
		VolatileRegisters = Registers.FindAll(r => r.IsVolatile);

		NonReservedRegisters = VolatileRegisters.FindAll(r => !r.IsReserved);
		NonReservedRegisters.AddRange(NonVolatileRegisters.FindAll(r => !r.IsReserved));

		MediaRegisters = Registers.FindAll(r => r.IsMediaRegister);
	}

	private void LoadArhitectureX64()
	{
		var is_non_volatile = Assembler.IsTargetWindows && Assembler.Size.Bits == 64;
		var base_pointer_flags = Assembler.IsDebuggingEnabled ? RegisterFlag.RESERVED | RegisterFlag.BASE_POINTER : RegisterFlag.NONE;

		Registers.AddRange(new List<Register>()
		{
			new Register(Size.QWORD, new [] { "rax", "eax", "ax", "al" }, RegisterFlag.VOLATILE | RegisterFlag.RETURN | RegisterFlag.NUMERATOR),
			new Register(Size.QWORD, new [] { "rbx", "ebx", "bx", "bl" }),
			new Register(Size.QWORD, new [] { "rcx", "ecx", "cx", "cl" }, RegisterFlag.VOLATILE | RegisterFlag.SHIFT),
			new Register(Size.QWORD, new [] { "rdx", "edx", "dx", "dl" }, RegisterFlag.VOLATILE | RegisterFlag.REMAINDER),
			new Register(Size.QWORD, new [] { "rsi", "esi", "si", "sil" }, is_non_volatile ? RegisterFlag.NONE : RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new [] { "rdi", "edi", "di", "dil" }, is_non_volatile ? RegisterFlag.NONE : RegisterFlag.VOLATILE),
			new Register(Size.QWORD, new [] { "rbp", "ebp", "bp", "bpl" }, base_pointer_flags),
			new Register(Size.QWORD, new [] { "rsp", "esp", "sp", "spl" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER),

			new Register(Size.YMMWORD, new string[] { "ymm0", "xmm0", "xmm0", "xmm0", "xmm0", "xmm0" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED | RegisterFlag.DECIMAL_RETURN),
			new Register(Size.YMMWORD, new string[] { "ymm1", "xmm1", "xmm1", "xmm1", "xmm1" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm2", "xmm2", "xmm2", "xmm2", "xmm2" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm3", "xmm3", "xmm3", "xmm3", "xmm3" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm4", "xmm4", "xmm4", "xmm4", "xmm4" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm5", "xmm5", "xmm5", "xmm5", "xmm5" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm6", "xmm6", "xmm6", "xmm6", "xmm6" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm7", "xmm7", "xmm7", "xmm7", "xmm7" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm8", "xmm8", "xmm8", "xmm8", "xmm8" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm9", "xmm9", "xmm9", "xmm9", "xmm9" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm10", "xmm10", "xmm10", "xmm10", "xmm10" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm11", "xmm11", "xmm11", "xmm11", "xmm11" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm12", "xmm12", "xmm12", "xmm12", "xmm12" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm13", "xmm13", "xmm13", "xmm13", "xmm13" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm14", "xmm14", "xmm14", "xmm14", "xmm14" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED),
			new Register(Size.YMMWORD, new string[] { "ymm15", "xmm15", "xmm15", "xmm15", "xmm15" }, RegisterFlag.MEDIA | RegisterFlag.VOLATILE | RegisterFlag.RESERVED)
		});

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
	}

	private void LoadArhitectureArm64()
	{
		for (var i = 0; i < 29; i++)
		{
			var register = new Register(Size.QWORD, new [] { $"x{i}", $"w{i}", $"w{i}", $"w{i}" });

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
			var register = new Register(Size.FromFormat(Types.DECIMAL.Format), new [] { $"d{i}", $"d{i}", $"d{i}", $"d{i}" }, RegisterFlag.MEDIA | RegisterFlag.RESERVED);
			
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

		Registers.Add(new Register(Size.QWORD, new [] { "x30", "w30", "w30", "w30" }, RegisterFlag.RESERVED | RegisterFlag.RETURN_ADDRESS));
		Registers.Add(new Register(Size.QWORD, new [] { "xzr", "wzr", "szr", "bzr" }, RegisterFlag.ZERO | RegisterFlag.VOLATILE));
		Registers.Add(new Register(Size.QWORD, new [] { "sp", "sp", "sp", "sp" }, RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER));
	}

	public void ExpectMode(UnitPhase expected)
	{
		if (Phase != expected)
		{
			throw new InvalidOperationException("Unit mode did not match the expected");
		}
	}

	/// <summary>
	/// Returns all variables currently inside registers that are needed at the given instruction position or later
	/// </summary>
	public List<VariableState> GetState(int at)
	{
		return Scope!.Variables
			.Where(v => v.Value.IsAnyRegister && v.Value.IsValid(at))
			.Select(v => new VariableState(v.Key, v.Value.Value.To<RegisterHandle>().Register)).ToList();
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
			Anchor = instruction;
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

		if (Phase != UnitPhase.BUILD_MODE)
		{
			return;
		}

		if (after)
		{
			Position = Anchor!.Position;
			return;
		}

		var destination = Anchor;

		Anchor = instruction;
		Position = instruction.Position;

		instruction.Build();
		instruction.OnSimulate();

		// Return to the previous instruction by iterating forward since it must be at the current position or further
		while (Anchor != destination)
		{
			if (!Anchor.IsBuilt)
			{
				var temporary = Anchor;
				temporary.Build();
				temporary.OnSimulate();
			}

			Anchor = Instructions[++Position];
		}
	}

	public void Append(IEnumerable<Instruction> instructions, bool after = false)
	{
		if (!instructions.Any())
		{
			return;
		}

		if (Phase != UnitPhase.BUILD_MODE)
		{
			throw new ApplicationException("Appending a range of instructions only works in build mode since it uses reindexing");
		}

		if (Position < 0)
		{
			Instructions.AddRange(instructions);
			Anchor = Instructions.Last();
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

		if (Phase != UnitPhase.BUILD_MODE)
		{
			return;
		}

		if (after)
		{
			Position = Anchor!.Position;
			return;
		}

		var destination = Anchor;

		Anchor = instructions.First();
		Position = Anchor.Position;

		// Return to the previous instruction by iterating forward since it must be at the current position or further
		while (Anchor != destination)
		{
			if (!Anchor.IsBuilt)
			{
				var temporary = Anchor;
				temporary.Build();
				temporary.OnSimulate();
			}

			Anchor = Instructions[++Position];
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

	/// <summary>
	/// Moves the value of the specified register to memory
	/// </summary>
	public void Release(Register register)
	{
		var value = register.Handle;

		if (value == null)
		{
			return;
		}

		if (value.IsReleasable(this))
		{
			foreach (var iterator in Scope!.Variables)
			{
				if (!iterator.Value.Equals(value))
				{
					continue;
				}

				var destination = new Result(References.CreateVariableHandle(this, iterator.Key), iterator.Key.Type!.Format);

				var move = new MoveInstruction(this, destination, value)
				{
					Description = $"Releases the value representing '{iterator.Key}' to memory",
					Type = MoveType.RELOCATE
				};

				Append(move);
				break;
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
		return new Label(Function.GetFullname() + $"_L{Indexer[Indexer.LABEL]}");
	}

	public string GetNextString()
	{
		return Function.GetFullname() + $"_S{Indexer[Indexer.STRING]}";
	}

	public string GetNextConstantIdentifier(object constant)
	{
		if (Constants.TryGetValue(constant, out string? identifier))
		{
			return identifier;
		}

		identifier = Function.GetFullname() + $"_C{Indexer[Indexer.CONSTANT]}";

		Constants.Add(constant, identifier);
		return identifier;
	}

	public Register? GetNextNonVolatileRegister(int start, int end)
	{
		var register = NonVolatileRegisters.Find(r => (r.Handle == null || !r.Handle.Lifetime.IsIntersecting(start, end)) && !r.IsReserved);

		return register;
	}

	public Register? GetNextNonVolatileRegister(bool release = true)
	{
		var register = NonVolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null || !release)
		{
			return register;
		}

		register = NonVolatileRegisters.Find(r => r.IsReleasable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

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
		register = VolatileRegisters.Find(r => r.IsReleasable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		if (register != null)
		{
			Release(register);
			return register;
		}

		// Try to find the next volatile register which contains a value that has a corresponding memory location
		register = NonVolatileRegisters.Find(r => r.IsReleasable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

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
			// NOTE: This usually happens when there's a flaw in the algorithm and the compiler doesn't know how to handle a value for example
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
		var register = MediaRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister));

		if (register != null)
		{
			return register;
		}

		register = MediaRegisters.Find(r => r.IsReleasable(this) && !(Function.Returns && r.IsReturnRegister));

		if (register != null)
		{
			Release(register);
			return register;
		}

		throw new NotImplementedException("Could not find an available media register");
	}

	/// <summary>
	/// Tries to get the next available register without releasing a register to memory
	/// </summary>
	public Register? GetNextRegisterWithoutReleasing()
	{
		var register = VolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		return register ?? NonVolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);
	}

	/// <summary>
	/// Tries to get the next available register without releasing a register to memory (excludes the specified registers from the search)
	/// </summary>
	public Register? GetNextRegisterWithoutReleasing(Register[] exclude)
	{
		var register = VolatileRegisters.Where(r => !exclude.Contains(r)).ToList()
			.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

		return register ?? NonVolatileRegisters.Where(r => !exclude.Contains(r)).ToList()
			.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);
	}

	/// <summary>
	/// Tries to get the next available media register without releasing a register to memory
	/// </summary>
	public Register? GetNextMediaRegisterWithoutReleasing()
	{
		return MediaRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister));
	}

	/// <summary>
	/// Tries to get the next available media register without releasing a register to memory (excludes the specified registers from the search)
	/// </summary>
	public Register? GetNextMediaRegisterWithoutReleasing(Register[] exclude)
	{
		return MediaRegisters.Where(r => !exclude.Contains(r)).ToList()
			.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister));
	}

	public Register GetStackPointer()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.STACK_POINTER)) ?? throw new Exception("Architecture did not have stack pointer register");
	}

	public Register GetBasePointer()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.BASE_POINTER)) ?? throw new Exception("Architecture did not have base pointer register");
	}

	public Register GetStandardReturnRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.RETURN)) ?? throw new Exception("Architecture did not have a standard return register");
	}

	public Register GetDecimalReturnRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.DECIMAL_RETURN)) ?? throw new Exception("Architecture did not have a decimal return register");
	}

	public Register GetNumeratorRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.NUMERATOR)) ?? throw new ApplicationException("Architecture did not have a numerator register");
	}

	public Register GetRemainderRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER)) ?? throw new ApplicationException("Architecture did not have a remainder register");
	}

	public Register GetShiftRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.SHIFT)) ?? throw new ApplicationException("Architecture did not have a shift register");
	}

	public Register GetZeroRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.ZERO)) ?? throw new ApplicationException("Architecture did not have a zero register");
	}

	public Register GetReturnAddressRegister()
	{
		return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.RETURN_ADDRESS)) ?? throw new ApplicationException("Architecture did not have a return address register");
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

			Position = instruction.Position;

			if (Position + 1 >= Instructions.Count)
			{
				break;
			}

			Position++;
		}

		// Reset the state after this simulation
		Phase = UnitPhase.READ_ONLY_MODE;
	}

	public Result? GetCurrentVariableHandle(Variable variable)
	{
		return Scope?.GetCurrentVariableHandle(variable);
	}

	public string Export()
	{
		if (Assembler.IsDebuggingEnabled)
		{
			Builder.AppendLine(".cfi_endproc");
		}
		
		return Builder.ToString();
	}

	public void Reindex()
	{
		Position = 0;

		// Reindex all instructions
		foreach (var instruction in Instructions)
		{
			instruction.Position = Position++;
		}

		var dependencies = Instructions.Select(i => i.GetAllUsedResults()).ToArray();

		// Reset all lifetimes
		foreach (var iterator in dependencies)
		{
			foreach (var dependency in iterator)
			{
				dependency.Lifetime.Reset();
			}
		}

		// Calculate lifetimes
		for (var i = 0; i < dependencies.Length; i++)
		{
			var instruction = Instructions[i];
			var iterator = dependencies[i];

			foreach (var dependency in iterator)
			{
				dependency.Use(i);
			}
		}
	}

	public void Reindex(Instruction instruction)
	{
		var dependencies = instruction.GetAllUsedResults();

		foreach (var dependency in dependencies)
		{
			dependency.Use(Position);
		}
	}

	public bool TryAppendPosition(Node node)
	{
		if (!Assembler.IsDebuggingEnabled)
		{
			return true;
		}

		if (node.Position == null)
		{
			return false;
		}

		Append(new AppendPositionInstruction(this, node.Position));
		
		return true;
	}

	public string GetNextIdentity()
	{
		return Indexer.UNIT.ToLowerInvariant() + '.' + Function.Identity + '.' + Indexer[Indexer.UNIT];
	}

	public override string ToString()
	{
		var occupied_register = Registers.Where(r => r.Handle != null);
		var description = string.Join(", ", occupied_register.Select(r => r.ToString() + ": " + r.GetDescription().ToLowerInvariant()));

		return Function.ToString() + " | " + (string.IsNullOrEmpty(description) ? "ready" : description);
	}
}