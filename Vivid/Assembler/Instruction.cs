using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ParameterFlag
{
	public const int NONE = 0;
	public const int DESTINATION = WRITES | 1;
	public const int SOURCE = 2;
	public const int WRITE_ACCESS = 4;
	public const int ATTACH_TO_DESTINATION = 8;
	public const int ATTACH_TO_SOURCE = 16;
	public const int RELOCATE_TO_DESTINATION = 32;
	public const int RELOCATE_TO_SOURCE = 64;
	public const int HIDDEN = 128;
	public const int BIT_LIMIT = 256;
	public const int BIT_LIMIT_64 = BIT_LIMIT | (64 << 24);
	public const int NO_ATTACH = 512;
	public const int WRITES = 1024;
	public const int READS = 2048;
	public const int ALLOW_ADDRESS = 4096;
	public const int LOCKED = 8192;

	public static int CreateBitLimit(int bits)
	{
		return BIT_LIMIT | (bits << 24);
	}

	public static int GetBitLimit(int flag)
	{
		return flag >> 24;
	}
}

public class InstructionParameter
{
	public Result Result { get; set; }
	public Handle? Value { get; set; } = null;
	public Size Size { get; set; } = Size.NONE;
	public HandleType[] Types { get; private set; }

	public int Flags { get; private set; }

	public bool IsHidden => Flag.Has(Flags, ParameterFlag.HIDDEN);
	public bool IsDestination => Flag.Has(Flags, ParameterFlag.DESTINATION);
	public bool IsSource => Flag.Has(Flags, ParameterFlag.SOURCE);
	public bool IsProtected => !Flag.Has(Flags, ParameterFlag.WRITE_ACCESS);
	public bool IsAttachable => !Flag.Has(Flags, ParameterFlag.NO_ATTACH);
	public bool Writes => Flag.Has(Flags, ParameterFlag.WRITES);

	public bool IsAnyRegister => Value?.Type == HandleType.REGISTER || Value?.Type == HandleType.MEDIA_REGISTER;
	public bool IsStandardRegister => Value?.Type == HandleType.REGISTER;
	public bool IsMediaRegister => Value?.Type == HandleType.MEDIA_REGISTER;
	public bool IsMemoryAddress => Value?.Type == HandleType.MEMORY;
	public bool IsConstant => Value?.Type == HandleType.CONSTANT;

	public bool IsValueValid => Value != null && Types.Contains(Value.Type);

	public InstructionParameter(Result handle, int flags, params HandleType[] types)
	{
		if (types == null || types.Length == 0)
		{
			throw new ArgumentException("Instruction parameter types must contain at least one option");
		}

		Flags = flags;
		Result = handle;
		Types = types;
	}

	public InstructionParameter(Handle handle, int flags)
	{
		Flags = flags;
		Result = new Result(handle, Settings.Format);
		Value = handle;
		Types = new[] { handle.Type };
	}

	/// <summary>
	/// Returns all valid handle options that are lower in cost than the current one
	/// </summary>
	public HandleType[] GetLowerCostHandleOptions(HandleType current)
	{
		var index = Types.ToList().IndexOf(current);

		if (index == -1) throw new ArgumentException("Could not retrieve lower cost handle options since the current handle type was not valid");

		// Return all handle types before the index of the current handle (the handles at the top are lower in cost)
		return Types.Take(index).ToArray();
	}

	public bool IsValid()
	{
		if (!Types.Contains(Result.Value.Type))
		{
			return false;
		}

		// Watch out for bit limit
		if (Result.IsConstant)
		{
			var bits = Result.Value.To<ConstantHandle>().Bits;

			if (!Flag.Has(Flags, ParameterFlag.BIT_LIMIT))
			{
				return bits <= 32;
			}

			return bits <= ParameterFlag.GetBitLimit(Flags);
		}

		// Data section handles should be moved into a register
		if (Result.Value.Is(HandleInstanceType.DATA_SECTION) || Result.Value.Is(HandleInstanceType.CONSTANT_DATA_SECTION))
		{
			var handle = Result.Value.To<DataSectionHandle>();

			if (Settings.IsArm64) return Flag.Has(Flags, ParameterFlag.ALLOW_ADDRESS) && handle.Address;

			// Using the address value can be allowed, if the bit limit has been raised to 64-bit
			return Flag.Has(Flags, ParameterFlag.BIT_LIMIT_64) || !handle.Address;
		}

		return true;
	}

	public override string ToString()
	{
		return Value?.ToString() ?? Result.Value.ToString();
	}
}

public enum InstructionState
{
	NOT_BUILT = 1,
	BUILDING = 2,
	BUILT = 4
}

public class Instruction
{
	public Unit Unit { get; private set; }
	public Scope? Scope { get; set; }
	public Result Result { get; private set; }
	public InstructionType Type { get; }
	public string Description { get; set; } = string.Empty;
	public string Operation { get; set; } = string.Empty;
	public List<InstructionParameter> Parameters { get; private set; } = new List<InstructionParameter>();
	public List<Result>? Dependencies { get; set; }
	public InstructionState State { get; set; } = InstructionState.NOT_BUILT;

	public InstructionParameter? Destination => Parameters.Find(i => i.IsDestination);
	public InstructionParameter? Source => Parameters.Find(i => !i.IsDestination);

	public bool IsUsageAnalyzed { get; set; } = true; // Controls whether the unit is allowed to load operands into registers while respecting the constraints
	public bool IsBuilt { get; protected set; } = false; // Tells whether this instructions is built
	public bool IsAbstract { get; set; } = false; // Tells whether the instruction is abstract. Abstract instructions will not translate into real assembly instructions
	public bool IsManual { get; set; } = false; // Tells whether the instruction is built manually using textual assembly. This helps the assembler by telling it to use the assembly code parser.

	/// <summary>
	/// Returns the largest format with the specified sign
	/// </summary>
	public static Format GetSystemFormat(bool unsigned)
	{
		return unsigned ? Format.UINT64 : Format.INT64;
	}

	/// <summary>
	/// Returns the largest format with the specified sign
	/// </summary>
	public static Format GetSystemFormat(Format format)
	{
		return format.IsUnsigned() ? Format.UINT64 : Format.INT64;
	}

	/// <summary>
	/// Returns the largest format with the specified sign
	/// </summary>
	public static Format GetSystemFormat(Result result)
	{
		return result.Format.IsUnsigned() ? Format.UINT64 : Format.INT64;
	}

	/// <summary>
	/// Returns the largest format with the specified sign
	/// </summary>
	public static Format GetSystemFormat(Variable variable)
	{
		return variable.Type!.Format.IsUnsigned() ? Format.UINT64 : Format.INT64;
	}

	public Instruction(Unit unit, InstructionType type)
	{
		Unit = unit;
		Type = type;
		Result = new Result();
		Dependencies = new List<Result> { Result };
	}

	public bool Is(InstructionType type)
	{
		return Type == type;
	}

	public bool Is(params InstructionType[] types)
	{
		return types.Any(i => Type == i);
	}

	/// <summary>
	/// Adds this instruction to the unit and returns the result of this instruction
	/// </summary>
	public Result Add()
	{
		Unit.Add(this);
		return Result;
	}

	public T To<T>() where T : Instruction
	{
		return (T)this;
	}

	/// <summary>
	/// Prepares the handle for use by relocating its inner handles into registers, therefore its use does not require additional steps, except if it is in invalid format
	/// </summary>
	/// <returns>
	/// Returns a list of register locks which must be active while the handle is in use
	/// </returns>
	private void ValidateHandle(Handle handle, List<Register> locked)
	{
		var results = handle.GetRegisterDependentResults();

		foreach (var iterator in results)
		{
			if (!iterator.IsStandardRegister)
			{
				Memory.MoveToRegister(Unit, iterator, Settings.Size, false, Trace.For(Unit, iterator));
			}

			var register = iterator.Register;
			register.Lock();

			locked.Add(register);
		}
	}

	/// <summary>
	/// Simulates the interactions between the instruction parameters such as relocating the source to the destination
	/// </summary>
	private void ApplyParameterFlags()
	{
		var destination = (Handle?)null;
		var source = (Handle?)null;

		// Determine the destination and the source
		for (var i = 0; i < Parameters.Count; i++)
		{
			var parameter = Parameters[i];

			if (parameter.IsDestination && parameter.IsAttachable)
			{
				// There should not be multiple destinations
				if (destination != null) throw new ApplicationException("Instruction had multiple destinations");
				destination = parameter.Value!.Finalize();
			}

			if (parameter.IsSource && parameter.IsAttachable)
			{
				if (source != null) throw new InvalidOperationException("Instruction had multiple sources");
				source = parameter.Value!.Finalize();
			}

			var is_relocated = Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION) || Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_SOURCE);
			var is_register = parameter.Result.IsAnyRegister;

			if (is_relocated && is_register)
			{
				// Since the parameter is relocated, its current register can be reset
				parameter.Result.Value.To<RegisterHandle>().Register.Reset();
			}
		}

		if (destination != null)
		{
			if (destination.Instance == HandleInstanceType.REGISTER)
			{
				var register = destination.To<RegisterHandle>().Register;
				var attached = false;

				// Search for values to attach to the destination register
				foreach (var parameter in Parameters)
				{
					if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_DESTINATION) || Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
					{
						register.Value = parameter.Result;
						parameter.Result.Format = destination.Format;
						attached = true;
						break;
					}
				}

				// If no result was attached to the destination, the default action should be taken
				if (!attached)
				{
					register.Value = Result;
					Result.Format = destination.Format;
				}
			}

			// Search for values to relocate to the destination
			foreach (var parameter in Parameters)
			{
				if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
				{
					parameter.Result.Value = destination;
					parameter.Result.Format = destination.Format;
				}
			}
		}

		if (source != null)
		{
			if (source.Instance == HandleInstanceType.REGISTER)
			{
				var register = source.To<RegisterHandle>().Register;

				// Search for values to attach to the source register
				foreach (var parameter in Parameters)
				{
					if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_SOURCE) || Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_SOURCE))
					{
						register.Value = parameter.Result;
						parameter.Result.Format = source.Format;
						break;
					}
				}
			}

			// Search for values to relocate to the source
			foreach (var parameter in Parameters)
			{
				if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_SOURCE))
				{
					parameter.Result.Value = source;
					parameter.Result.Format = source.Format;
				}
			}
		}
	}

	public Result Convert(InstructionParameter parameter)
	{
		var protect = parameter.IsDestination && parameter.IsProtected;
		var directives = parameter.IsDestination ? Trace.For(Unit, Result) : Trace.For(Unit, parameter.Result);

		if (parameter.IsValid())
		{
			// Get the more preferred options for this parameter
			var options = parameter.GetLowerCostHandleOptions(parameter.Result.Value.Type);

			if (options.Contains(HandleType.REGISTER))
			{
				// If the value will be used later in the future and the register situation is good, the value can be moved to a register
				if (IsUsageAnalyzed && !parameter.IsDestination && !parameter.Result.IsDeactivating() && Unit.GetNextRegisterWithoutReleasing() != null)
				{
					return Memory.MoveToRegister(Unit, parameter.Result, parameter.Size, false, directives);
				}
			}
			else if (options.Contains(HandleType.MEDIA_REGISTER))
			{
				// If the value will be used later in the future and the register situation is good, the value can be moved to a register
				if (IsUsageAnalyzed && !parameter.IsDestination && !parameter.Result.IsDeactivating() && Unit.GetNextMediaRegisterWithoutReleasing() != null)
				{
					return Memory.MoveToRegister(Unit, parameter.Result, parameter.Size, true, directives);
				}
			}

			// If the parameter size does not match the required size, it can be converted by moving it to register
			// NOTE: The parameter shall not be converted if it represents a destination memory address which is being written to
			if (parameter.Size != Size.NONE && parameter.Result.Size != parameter.Size)
			{
				if (parameter.Result.IsMemoryAddress && parameter.IsDestination)
				{
					throw new ApplicationException("Could not convert memory address to the required size since it was specified as a destination");
				}

				Memory.Convert(Unit, parameter.Result, parameter.Size, directives);
			}

			// If the current parameter is the destination and it is needed later, then it must me copied to another register
			if (protect && parameter.Result.IsOnlyActive())
			{
				return Memory.CopyToRegister(Unit, parameter.Result, parameter.Size, parameter.Types.Contains(HandleType.MEDIA_REGISTER), directives);
			}

			return parameter.Result;
		}

		return Memory.Convert(Unit, parameter.Result, parameter.Size, parameter.Types, protect, directives);
	}

	/// <summary>
	/// Builds the given operation without any processing
	/// </summary>
	public void Build(string operation)
	{
		Operation = operation;
		IsManual = true;
	}

	/// <summary>
	/// Builds the instruction with the given arguments but doesn't force the parameters to be the same size
	/// </summary>
	public void Build(string operation, params InstructionParameter[] parameters)
	{
		Build(operation, Size.NONE, parameters);
	}

	/// <summary>
	/// Builds the instruction with the given arguments but doesn't force the parameters to be the same size
	/// </summary>
	public void Build(params InstructionParameter[] parameters)
	{
		Build(string.Empty, Size.NONE, parameters);
	}

	/// <summary>
	/// Builds the instruction with the specified arguments and forces the parameters to match the specified size
	/// </summary>
	public void Build(string operation, Size size, params InstructionParameter[] parameters)
	{
		Parameters.Clear();

		var locked = new List<Register>();

		for (var i = 0; i < parameters.Length; i++)
		{
			// Convert the parameter into a valid format
			var parameter = parameters[i];
			parameter.Size = size == Size.NONE ? parameter.Result.Size : size;

			// Convert the parameter to a valid format for this instruction
			var converted = Convert(parameter);

			// Set the result of this instruction to match the parameter, if it is the destination
			if (parameter.IsDestination)
			{
				Result.Value = converted.Value;
				Result.Format = converted.Format;
			}

			// Prepare the handle for use
			ValidateHandle(converted.Value, locked);

			// Prevents other parameters from stealing the register of the current parameter in the middle of this instruction
			if (converted.Value.Is(HandleInstanceType.REGISTER))
			{
				var register = converted.Register;
				register.Lock();
				locked.Add(register);
			}

			var format = converted.Format;

			parameter.Result = converted;
			parameter.Value = converted.Value.Finalize();
			parameter.Value.Format = format.IsDecimal() ? Format.DECIMAL : parameter.Size.ToFormat(format.IsUnsigned());

			Parameters.Add(parameter);
		}

		// Simulate the effects of the parameter flags
		ApplyParameterFlags();

		// Allow final touches to this instruction
		Operation = operation;
		OnPostBuild();

		// Unlock the registers since the instruction has been executed
		locked.ForEach(i => i.Unlock());
	}

	public void Reindex()
	{
		var dependencies = GetAllDependencies();
		foreach (var dependency in dependencies) { dependency.Use(this); }

		// Use the result at its usages
		Result.Use(Result.Lifetime.Usages);
	}

	public void Build()
	{
		if (State == InstructionState.BUILT)
		{
			foreach (var parameter in Parameters)
			{
				if (!parameter.IsValueValid || parameter.Value == null)
				{
					throw new ApplicationException("During translation one instruction parameter was in incorrect format");
				}

				if (parameter.IsDestination)
				{
					// Set the result to be equal to the destination
					Result.Value = parameter.Value;
				}
			}

			// Simulate the effects of the parameter flags
			ApplyParameterFlags();
		}
		else
		{
			State = InstructionState.BUILDING;
			Reindex();
			OnBuild();
			Reindex();
			State = InstructionState.BUILT;
		}

		// Extend all inner results to last at least as long as their parents
		/// NOTE: This fixes the issue where for example a memory address is created but the lifetime of the starting address is not extended so its register could be stolen
		foreach (var iterator in GetAllDependencies())
		{
			foreach (var inner in iterator.Value.GetInnerResults())
			{
				inner.Use(iterator.Lifetime.Usages);
			}
		}
	}

	public void Finish()
	{
		// Skip empty instructions
		if (string.IsNullOrEmpty(Operation)) return;

		var builder = new StringBuilder(Operation);
		var added = false;

		foreach (var parameter in Parameters)
		{
			if (parameter.IsHidden) continue;

			var value = parameter.Value?.ToString();
			if (string.IsNullOrEmpty(value)) throw new ApplicationException("Instruction parameter could not be converted into assembly");

			builder.Append($" {value},");
			added = true;
		}

		// If any parameters were added, remove the comma from the end
		if (added) builder.Remove(builder.Length - 1, 1);

		Unit.Write(builder.ToString());
	}

	public virtual void OnBuild() {}
	public virtual void OnPostBuild() {}
	public virtual bool Redirect(Handle handle, bool root) { return false; }

	public virtual Result[] GetDependencies()
	{
		return new[] { Result };
	}

	public IEnumerable<Result> GetAllDependencies()
	{
		if (Dependencies == null)
		{
			return Parameters.Select(i => i.Result).Concat(GetDependencies());
		}

		return Parameters.Select(i => i.Result).Concat(Dependencies);
	}
}