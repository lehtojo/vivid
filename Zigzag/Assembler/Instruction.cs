using System.Linq;
using System.Text;
using System.Collections.Generic;
using System;

public struct InstructionParameter
{
    public Result Handle { get; private set; }
    public HandleType[] Types { get; private set; }

    public HandleType PrefferedType => Types[0];
    public bool IsDestination { get; private set; }
    public bool IsValid => Types.Contains(Handle.Value.Type);

    public InstructionParameter(Result handle, bool destination, params HandleType[] types)
    {
        if (types == null || types.Length == 0)
        {
            throw new ArgumentException("Instruction parameter types must contain atleast one option");
        }

        IsDestination = destination;

        Handle = handle;
        Types = types;
    }

    public HandleType[] GetPrefferableOptions(HandleType current)
    {
        var index = Types.ToList().IndexOf(current);

        if (index == -1)
        {
            throw new ArgumentException("The current handle type isn't not an accepted handle type");
        }

        // Return all handle types before the index since they are more prefferable
        return Types.Take(index).ToArray();
    }

    public override string ToString()
    {
        return Handle.Value.ToString();
    }
}

public abstract class Instruction
{
    public Unit Unit { get; private set; }
    public Result Result { get; private set; }
    public int Position { get; set; } = -1;
    public InstructionType Type => GetInstructionType();

    public Instruction(Unit unit)
    {
        Unit = unit;
        Result = new Result(this);
    }

    public Result Execute()
    {
        Unit.Append(this);
        Position = Unit.Instructions.Count;
        return Result;
    }

    private Result Convert(InstructionParameter parameter)
    {
        if (!parameter.IsValid)
        {
            return Memory.Convert(Unit, parameter.Handle, parameter.Types, false, parameter.IsDestination);
        }
        else
        {
            var prefferable = parameter.GetPrefferableOptions(parameter.Handle.Value.Type);

            if (prefferable.Contains(HandleType.REGISTER))
            {
                var cached = Unit.TryGetCached(parameter.Handle);

                if (cached != null)
                {
                    return new Result(this, cached);
                }
            }

            return parameter.Handle;
        }
    }

    public string Format(StringBuilder builder, string format, params InstructionParameter[] parameters)
    {
        var handles = new List<Handle>();

        foreach (var parameter in parameters)
        {
            var handle = Convert(parameter);

            if (parameter.IsDestination)
            {
                Result.Set(handle.Value);

                // Attach result of this operation must be attached to the destination register
                if (handle.Value is RegisterHandle destination)
                {
                    destination.Register.Value = Result;
                }
            }

            handles.Add(handle.Value);
        }

        return string.Format(format, handles.ToArray());
    }

    public void Build(string operation, params InstructionParameter[] parameters)
    {
        var handles = new List<Result>();

        foreach (var parameter in parameters)
        {
            var handle = Convert(parameter);

            if (parameter.IsDestination)
            {
                Result.Set(handle.Value);
            }

            handles.Add(handle);
        }

        var result = new StringBuilder(operation);

        foreach (var handle in handles)
        {
            result.Append($" {handle.Value},");
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.IsDestination && handles[i].Value is RegisterHandle destination)
            {
                // Attach result of this operation must be attached to the destination register
                destination.Register.Value = Result;
            }
        }

        Unit.Append(result.Remove(result.Length - 1, 1).ToString());
    }

    public abstract Result? GetDestination();
    public abstract void Build();

    public void Redirect(Handle handle)
    {
        var destination = GetDestination();
        var previous = (Result?)null;

        while (destination != null && destination != previous)
        {
            destination.Set(handle);

            previous = destination;
            destination = destination.Instruction?.GetDestination();
        }
    }

    public abstract InstructionType GetInstructionType();
    public abstract Result[] GetHandles();
}