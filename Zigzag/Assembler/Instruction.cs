using System.Linq;
using System.Text;
using System.Collections.Generic;
using System;

public struct InstructionParameter
{
    public Quantum<Handle> Handle { get; private set; }
    public HandleType[] Types { get; private set; }

    public HandleType PrefferedType => Types[0];
    public bool IsDestination { get; private set; }
    public bool IsValid => Types.Contains(Handle.Value.Type);

    public InstructionParameter(Quantum<Handle> handle, bool destination, params HandleType[] types)
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
    public Quantum<Handle> Result { get; set; } = new Quantum<Handle>();
    public InstructionType Type => GetInstructionType();

    public Quantum<Handle> Execute(Unit unit)
    {
        unit.Append(this);
        return Result;
    }

    private Quantum<Handle> Convert(Unit unit, InstructionParameter parameter)
    {
        if (!parameter.IsValid)
        {
            return Memory.Convert(unit, parameter.Handle, parameter.PrefferedType, parameter.IsDestination);
        }
        else
        {
            var prefferable = parameter.GetPrefferableOptions(parameter.Handle.Value.Type);

            if (prefferable.Contains(HandleType.REGISTER))
            {
                var cached = unit.TryGetCached(parameter.Handle);

                if (cached != null)
                {
                    return new Quantum<Handle>(cached);
                }
            }

            return parameter.Handle;
        }
    }

    public string Mold(Unit unit, StringBuilder builder, string format, params InstructionParameter[] parameters)
    {
        var handles = new List<Handle>();

        foreach (var parameter in parameters)
        {
            var handle = Convert(unit, parameter);

            if (parameter.IsDestination)
            {
                Result.Set(handle.Value);
            }

            handles.Add(handle.Value);
        }

        return string.Format(format, handles.ToArray());
    }

    public void Build(Unit unit, string operation, params InstructionParameter[] parameters)
    {
        var handles = new List<Quantum<Handle>>();

        foreach (var parameter in parameters)
        {
            var handle = Convert(unit, parameter);

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

        unit.Append(result.Remove(result.Length - 1, 1).ToString());
    }

    public abstract void Weld(Unit unit);
    public abstract void Build(Unit unit);

    public abstract InstructionType GetInstructionType();
    public abstract Handle[] GetHandles();
}