using System.Linq;
using System.Text;
using System.Collections.Generic;
using System;

public static class ParameterFlag
{
    public const int NONE = 0;
    public const int DESTINATION = 1;
    public const int WRITE_ACCESS = 2;
    public const int ATTACH_TO_DESTINATION = 4;
    public const int RELOCATE_TO_DESTINATION = 8;
    public const int HIDDEN = 16;
}

public class InstructionParameter
{
    public Result Result { get; set; }
    public Handle? Value { get; set; } = null;
    public HandleType[] Types { get; private set; }
    public HandleType OptimalType => Types[0];

    public int Flags { get; private set; }
    public bool IsHidden { get; set; } = false;
    public bool IsDestination => Flag.Has(Flags, ParameterFlag.DESTINATION);
    public bool IsProtected => !Flag.Has(Flags, ParameterFlag.WRITE_ACCESS);
    public bool IsValid => Types.Contains(Result.Value.Type);
    public bool IsValueValid => Value != null && Types.Contains(Value.Type);
    public bool IsSizeVisible
    {
        set
        {
            if (Value != null)
            {
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

    private Result Convert(InstructionParameter parameter)
    {
        var protect = parameter.IsDestination && parameter.IsProtected;

        if (parameter.IsValid)
        {
            // Get the more preffered options for this parameter
            var options = parameter.GetLowerCostHandleOptions(parameter.Result.Value.Type);

            if (options.Contains(HandleType.REGISTER))
            {
                var cached = Unit.TryGetCached(parameter.Result, !protect);

                if (cached != null)
                {
                    return new Result(this, cached);
                }
            }

            // If the current parameter is the destination and it is needed later, then it must me copied to another register
            if (protect && !parameter.Result.IsExpiring(Position))
            {
                return Memory.CopyToRegister(Unit, parameter.Result);
            }

            return parameter.Result;
        }

        return Memory.Convert(Unit, parameter.Result, parameter.Types, false, protect);
    }

    public string Format(string format, params InstructionParameter[] parameters)
    {
        var handles = new List<Handle>();
        var locks = new List<RegisterLock>();

        foreach (var parameter in parameters)
        {
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

    public void Build(string operation)
    {
        Operation = operation;
    }

    public void Build(string operation, Size mode, params InstructionParameter[] parameters)
    {
        Parameters.Clear();

        var locks = new List<RegisterLock>();

        for (var i = 0; i < parameters.Length; i++)
        {
            // Convert the parameter into a valid format
            var parameter = parameters[i];

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

        var destination = (Handle?)null;

        for (var i = 0; i < Parameters.Count; i++)
        {
            var parameter = Parameters[i];

            if (parameter.IsDestination)
            {
                destination = parameter.Value;
                break;
            }
        }

        if (destination != null)
        {
            if (destination is RegisterHandle handle)
            {
                var register = handle.Register;
                var attached = false;

                foreach (var parameter in Parameters)
                {
                    if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_DESTINATION))
                    {
                        register.Handle = parameter.Result;
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    register.Handle = Result;
                }
            }

            foreach (var parameter in Parameters)
            {
                if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
                {
                    parameter.Result.Value = destination;
                }
            }
        }

        Operation = operation;
        OnPostBuild();

        locks.ForEach(l => l.Dispose());
    }

    public void Translate()
    {
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
        if (Parameters.Select(p => p.Value!.Size).Distinct().Count() == 1)
        {
            // Only one parameter is required to show its size
            Parameters[0].IsSizeVisible = true;

            // Hide other parameter's sizes for cleaner output
            foreach (var parameter in Parameters.Skip(1))
            {
                parameter.IsSizeVisible = false;
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
            if (!Flag.Has(parameter.Flags, ParameterFlag.HIDDEN))
            {
                result.Append($" {parameter.Value},");
            }
        }

        // -----------------------------------------------------

        var destination = (Handle?)null;

        for (var i = 0; i < Parameters.Count; i++)
        {
            var parameter = Parameters[i];

            if (parameter.IsDestination)
            {
                destination = parameter.Value;
                break;
            }
        }

        if (destination != null)
        {
            if (destination is RegisterHandle handle)
            {
                var register = handle.Register;
                var attached = false;

                foreach (var parameter in Parameters)
                {
                    if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_DESTINATION))
                    {
                        register.Handle = parameter.Result;
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    register.Handle = Result;
                }
            }

            foreach (var parameter in Parameters)
            {
                if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
                {
                    parameter.Result.Value = destination;
                }
            }
        }

        // -------------------------------------------------------

        if (Parameters.Count > 0)
        {
            result.Remove(result.Length - 1, 1);
        }

        Unit.Write(result.ToString());
    }

    public abstract Result? GetDestinationDependency();
    public abstract void OnBuild();
    public virtual void OnPostBuild() {}

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

            var destination = (Handle?)null;

            for (var i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];

                if (parameter.IsDestination)
                {
                    destination = parameter.Value;
                    break;
                }
            }

            if (destination != null)
            {
                if (destination is RegisterHandle handle)
                {
                    var register = handle.Register;
                    var attached = false;

                    foreach (var parameter in Parameters)
                    {
                        if (Flag.Has(parameter.Flags, ParameterFlag.ATTACH_TO_DESTINATION))
                        {
                            register.Handle = parameter.Result;
                            attached = true;
                            break;
                        }
                    }

                    if (!attached)
                    {
                        register.Handle = Result;
                    }
                }

                foreach (var parameter in Parameters)
                {
                    if (Flag.Has(parameter.Flags, ParameterFlag.RELOCATE_TO_DESTINATION))
                    {
                        parameter.Result.Value = destination;
                    }
                }
            }
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
}