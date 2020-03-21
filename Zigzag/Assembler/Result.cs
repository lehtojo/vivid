using System.Collections.Generic;
using System.Linq;
using System;

public class Result
{
    public Instruction? Instruction { get; set; }

    private object _Metadata = new object();
    public object Metadata { 
        get => _Metadata; 
        set {
            _Metadata = value;
            References.ForEach(r => r._Metadata = value);
        }
    }

    private Handle _Value;
    public Handle Value {
        get => _Value;
        set {
            _Value = value;
            References.ForEach(r => r._Value = value);
        }
    }

    public Lifetime Lifetime { get; private set; } = new Lifetime();

    private List<Result> References = new List<Result>();
    private IEnumerable<Result> System => References.Concat(new List<Result>{ this });

    public bool Empty => _Value.Type == HandleType.NONE;
    public bool Relesable => Metadata is Variable;

    public Result(Instruction instruction)
    {
        _Value = new Handle();
        Instruction = instruction;
    }

    public Result(Instruction instruction, Handle value)
    {
        _Value = value;
        Instruction = instruction;
    }

    public Result(Handle value)
    {
        _Value = value;
    }
    
    public void EntangleTo(Result parent)
    {
        // Update current references
        foreach (var reference in References)
        {
            reference.Value = parent.Value;
            reference.Lifetime = parent.Lifetime;
            reference.References.AddRange(parent.System);
        }

        // Update parent's references
        foreach (var reference in parent.References)
        {
            reference.References.AddRange(System);
        }

        // Update this reference:
        // Clone the current system that doesn't contain the parent itself
        var system = new List<Result>(System);

        Value = parent.Value;
        Lifetime = parent.Lifetime;
        Metadata = parent.Metadata;
        Instruction = parent.Instruction;
        References.AddRange(parent.System);

        // Update the parent
        parent.References.AddRange(system);
    }

    public bool IsExpiring(int position)
    {
        return position == -1 || !Lifetime.IsActive(position + 1);
    }

    public bool IsAlive(int position)
    {
        return Lifetime.IsActive(position);
    }

    public void Use(int position)
    {
        if (position > Lifetime.End)
        {
            Lifetime.End = position;
        }

        if (position < Lifetime.Start)
        {
            Lifetime.Start = position;
        }

        Value.AddUsage(position);
        References.ForEach(r => r.Lifetime = Lifetime);
    }

    public override string ToString() 
    {
        return Value?.ToString() ?? throw new InvalidOperationException("Missing value");
    }
}