using System.Linq;
using System.Text;
using System.Collections.Generic;
using System;

public static class ParameterFlag
{
	public const int NONE = 0;
	public const int DESTINATION = 1;
	public const int SOURCE = 2;
	public const int WRITE_ACCESS = 4;
	public const int ATTACH_TO_DESTINATION = 8;
	public const int ATTACH_TO_SOURCE = 16;
	public const int RELOCATE_TO_DESTINATION = 32;
	public const int RELOCATE_TO_SOURCE = 64;
	public const int HIDDEN = 128;
}

public class InstructionParameter
{
	public Result Result { get; set; }
	public Handle? Value { get; set; } = null;
	public Size RequiredSize { get; set; } = Size.NONE;
	public HandleType[] Types { get; private set; }
	public HandleType OptimalType => Types[0];

	public int Flags { get; private set; }
	
	public bool IsHidden => Flag.Has(Flags, ParameterFlag.HIDDEN);
	public bool IsDestination => Flag.Has(Flags, ParameterFlag.DESTINATION);
	public bool IsSource => Flag.Has(Flags, ParameterFlag.SOURCE);
	public bool IsProtected => !Flag.Has(Flags, ParameterFlag.WRITE_ACCESS);

	public bool IsRegister => Value?.Type == HandleType.REGISTER;
	public bool IsMediaRegister => Value is RegisterHandle handle && handle.Register.IsMediaRegister;
	public bool IsMemoryAddress => Result.Value.Type == HandleType.MEMORY;

	public bool IsValid => Types.Contains(Result.Value.Type);
	public bool IsValueValid => Value != null && Types.Contains(Value.Type);
	
	public bool IsSizeVisible {
		set {
			if (Value != null) {
				Value.IsSizeVisible = value;
			}
		}
	}

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

	/// <summary>
	/// Returns all valid handle options that are lower in cost than the current one
	/// </summary>
	public HandleType[] GetLowerCostHandleOptions(HandleType current)
	{
		var index = Types.ToList().IndexOf(current);

		if (index == -1)
		{
			throw new ArgumentException("The current handle type isn't not an accepted handle type");
		}

		// Return all handle types before the current handle's index (the handles at the top are lower in cost)
		return Types.Take(index).ToArray();
	}

	public override string ToString()
	{
		return Result.Value.ToString();
	}
}

public abstract class Instruction
{
	public Unit Unit { get; private set; }
	public Scope? Scope { get; set; }
	public Result Result { get; private set; }
	public int Position { get; set; } = -1;
	public InstructionType Type => GetInstructionType();

	public string Description { get; set; } = string.Empty;
	public string Operation { get; set; } = string.Empty;

	public List<InstructionParameter> Parameters { get; private set; } = new List<InstructionParameter>();
	public InstructionParameter? Destination => Parameters.Find(p => p.IsDestination);

	public bool IsBuilt { get; private set; } = false;

	public Instruction(Unit unit)
	{
		Unit = unit;
		Result = new Result(this);
	}

	/// <summary>
	/// Depending on the state of the unit, this instruction is executed or added to the execution chain
	/// </summary>
	public Result Execute()
	{
		Unit.Append(this);
		return Result;
	}

	public T To<T>() where T : Instruction
	{
		return (T)this ?? throw new ApplicationException($"Couldn't convert 'Instruction' to '{typeof(T).Name}'");
	}

	private Result Convert(InstructionParameter parameter)
	{
		var protect = parameter.IsDestination && parameter.IsProtected;

		if (parameter.IsValid)
		{
			// Get the more preffered options for this parameter
			var options = parameter.GetLowerCostHandleOptions(parameter.Result.Value.Type);

			if (options.Contains(HandleType.REGISTER))
			{
				// No need to worry about required size since registers are always in the right format
				var cached = Unit.TryGetCached(parameter.Result);

				if (cached != null)
				{
					return new Result(this, cached);
				}
			}

			// If the parameter size doesn't match the required size, it can be converted by moving it to register
			if (parameter.IsMemoryAddress && parameter.RequiredSize != Size.NONE && parameter.Result.Value.Size != parameter.RequiredSize)
			{
				Memory.MoveToRegister(Unit, parameter.Result, parameter.Types.Contains(HandleType.MEDIA_REGISTER));
			}

			// If the current parameter is the destination and it is needed later, then it must me copied to another register
			if (protect && !parameter.Result.IsExpiring(Position))
			{
				/// TODO: All parameter that include media register type are not floating point numbers
				return Memory.CopyToRegister(Unit, parameter.Result, parameter.Types.Contains(HandleType.MEDIA_REGISTER));
			}

			return parameter.Result;
		}

		return Memory.Convert(Unit, parameter.Result, parameter.Types, false, protect);
	}

	/// <summary>
	/// Formats the instruction with the given arguments and forces the parameters to match the given size
	/// </summary>
	public string Format(string format, Size size, params InstructionParameter[] parameters)
	{
		var handles = new List<Handle>();
		var locks = new List<RegisterLock>();

		foreach (var parameter in parameters)
		{
			// Force the parameter to match the given size
			parameter.RequiredSize = size;

			// Convert the parameter into a usable format
			var handle = Convert(parameter);

			if (parameter.IsDestination)
			{
				throw new NotImplementedException("Format called with a parameter that is a destination");
			}

			// Prevents other parameters from stealing the register of the current parameter in the middle of this instruction
			if (handle.Value is RegisterHandle register_handle)
			{
				// Register locks have a destructor which releases the register so they are safe
				locks.Add(new RegisterLock(register_handle.Register));
			}

			handles.Add(handle.Value);
		}

		locks.ForEach(l => l.Dispose());

		return string.Format(format, handles.ToArray());
	}

	/// <summary>
	/// Returns whether the result is actually a reference to another variable's value
	/// </summary> 
	private static bool IsReference(Variable variable, Result result)
	{
		return !(result.Metadata.Primary is VariableAttribute attribute && attribute.Variable == variable);
	}

	/// <summary>
	/// Simulates the interactions between the instruction parameters such as relocating the source to the destination
	/// </summary>
	private void SimulateParameterFlags()
	{
		var destination = (Handle?)null;
		var source = (Handle?)null;

		for (var i = 0; i < Parameters.Count; i++)
		{
			var parameter = Parameters[i];

			if (parameter.IsDestination)
			{
				if (destination != null)
				{
					throw new ApplicationException("Instruction parameters had multiple destinations");
				}

				destination = parameter.Value;
			}
			else if (parameter.IsSource)
			{
				if (source != null)
				{
					throw new ApplicationException("Instruction parameters had multiple sources");
				}

				source = parameter.Value;
			}
		}

		if (destination != null)
		{
			if (destination is RegisterHandle handle)
			{
				var register = handle.Register;
				var attached = false;

				// Search for values to attach to the destination register
				foreach (var parameter in Parameters)
				{
					if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_DESTINATION))
					{
						register.Handle = parameter.Result;
						attached = true;
						break;
					}
				}

				// If no result was attachted to the destination, the default action should be taken
				if (!attached)
				{
					register.Handle = Result;
				}
			}

			// Search for values to relocate to the destination
			foreach (var parameter in Parameters)
			{
				if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
				{
					parameter.Result.Value = destination;
				}
			}
		}

		if (source != null)
		{
			if (source is RegisterHandle handle)
			{
				var register = handle.Register;

				// Search for values to attach to the source register
				foreach (var parameter in Parameters)
				{
					if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_SOURCE))
					{
						register.Handle = parameter.Result;
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
				}
			}
		}
	}

	/// <summary>
	/// Builds the given operation without any processing
	/// </summary>
	public void Build(string operation)
	{
		Operation = operation;
	}

	/// <summary>
	/// Builds the instruction with the given arguments but doesn't force the parameters to be the same size
	/// </summary>
	public void Build(string operation, params InstructionParameter[] parameters)
	{
		Build(operation, Size.NONE, parameters);
	}

	/// <summary>
	/// Builds the instruction with the given arguments and forces the parameters to match the given size
	/// </summary>
	public void Build(string operation, Size size, params InstructionParameter[] parameters)
	{
		Parameters.Clear();

		var locks = new List<RegisterLock>();

		for (var i = 0; i < parameters.Length; i++)
		{
			// Convert the parameter into a valid format
			var parameter = parameters[i];
			parameter.RequiredSize = size;

			// Convert the parameter to a valid format for this instruction
			var result = Convert(parameter);

			if (parameter.IsDestination)
			{
				// Set the result to be equal to the destination
				Result.Value = result.Value;
			}

			// Prevents other parameters from stealing the register of the current parameter in the middle of this instruction
			if (result.Value is RegisterHandle register_handle)
			{
				// Register locks have a destructor which releases the register so they are safe
				locks.Add(new RegisterLock(register_handle.Register));
			}

			parameter.Result = result;
			parameter.Value = result.Value.Freeze();

			Parameters.Add(parameter);
		}

		SimulateParameterFlags();

		Operation = operation;
		OnPostBuild();

		locks.ForEach(l => l.Dispose());
	}

	public void Translate()
	{
		// Check if this instruction is meant to be translated
		if (string.IsNullOrEmpty(Operation))
		{
			return;
		}

		foreach (var parameter in Parameters)
		{
			if (!parameter.IsValueValid || parameter.Value == null)
			{
				throw new ApplicationException("During translation operation a parameter was in incorrect format");
			}

			if (parameter.IsDestination)
			{
				// Set the result to be equal to the destination
				Result.Value = parameter.Value;
			}
		}

		var result = new StringBuilder(Operation);

		// Detect whether all parameter are meant to be the same size
		if (Parameters.All(p => !p.IsMediaRegister) && Parameters.Select(p => p.Value!.Size).Distinct().Count() == 1)
		{
			// Try to find the first parameter which is actually visible
			var visible = Parameters.Find(p => !p.IsHidden);

			if (visible != null)
			{
				// Only one parameter is required to show its size
				visible.IsSizeVisible = true;
			}
			else
			{
				/// TODO: Investigate whether this is bad or not
				Console.WriteLine("Warning: All parameter of an instruction were hidden");
			}

			// Hide other parameter's sizes for cleaner output
			foreach (var parameter in Parameters.Skip(1))
			{
				if (parameter != visible)
				{
					parameter.IsSizeVisible = false;
				}
			}
		}
		else
		{
			// Each parameter must be configured to display their sizes
			foreach (var parameter in Parameters)
			{
				parameter.IsSizeVisible = true;
			}
		}

		foreach (var parameter in Parameters)
		{
			if (!parameter.IsHidden)
			{
				result.Append($" {parameter.Value},");
			}
		}
		
		SimulateParameterFlags();

		if (Parameters.Count > 0)
		{
			result.Remove(result.Length - 1, 1);
		}

		Unit.Write(result.ToString());
	}

	public abstract Result? GetDestinationDependency();
	
	public virtual void OnSimulate() { }
	public abstract void OnBuild();
	public virtual void OnPostBuild() { }

	public void Build()
	{
		if (IsBuilt)
		{
			foreach (var parameter in Parameters)
			{
				if (!parameter.IsValueValid || parameter.Value == null)
				{
					throw new ApplicationException("During translation operation a parameter was in incorrect format");
				}

				if (parameter.IsDestination)
				{
					// Set the result to be equal to the destination
					Result.Value = parameter.Value;
				}
			}
	
			SimulateParameterFlags();
		}
		else
		{
			IsBuilt = true;
			OnBuild();
		}
	}

	public virtual int GetStackOffsetChange()
	{
		return 0;
	}

	public Instruction GetRedirectionRoot()
	{
		var instruction = this;

		var previous = (Result?)null;
		var dependency = GetDestinationDependency();

		while (true)
		{
			if (dependency == null || dependency.Instruction == null || dependency == previous)
			{
				return instruction;
			}

			instruction = dependency.Instruction;

			previous = dependency;
			dependency = instruction.GetDestinationDependency();
		}
	}

	public int Redirect(Handle to)
	{
		Result.Value = to;

		var destination = GetDestinationDependency();
		var previous = (Result?)null;

		while (destination != null && destination != previous)
		{
			destination.Set(to, true);

			previous = destination;
			destination = destination.Instruction?.GetDestinationDependency();
		}

		return previous?.Instruction?.Position ?? -1;
	}

	public abstract InstructionType GetInstructionType();
	public abstract Result[] GetResultReferences();

	public Result[] GetAllUsedResults()
	{
		return Parameters.Select(p => p.Result).Concat(GetResultReferences()).ToArray();
	}

	public override string ToString()
	{
		return string.IsNullOrEmpty(Description) ? GetType().Name : Description;
	}
}