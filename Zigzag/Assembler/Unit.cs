using System.Collections.Generic;
using System.Text;
using System;

public class Lifetime
{
    public int Start { get; set; } = 0;
    public int End { get; set; } = -1;

    public bool Determined => End != -1;

    public bool IsActive(int position)
    {
        return position >= Start && (End == -1 || position <= End);
    }
}

public class Unit
{
    private Dictionary<Variable, List<Quantum<Handle>>> Variables = new Dictionary<Variable, List<Quantum<Handle>>>();
    private Dictionary<object, List<Quantum<Handle>>> Constants = new Dictionary<object, List<Quantum<Handle>>>();
    private List<Register> Registers { get; set; } = new List<Register>();
    private List<Register> NonVolatileRegisters { get; set; } = new List<Register>();
    private List<Register> VolatileRegisters { get; set; } = new List<Register>();
    private List<Instruction> Instructions { get; set; } = new List<Instruction>();
    private StringBuilder Builder { get; set; } = new StringBuilder();
    private List<Handle> Handles { get; set; }  = new List<Handle>();

    public int Position { get; private set; } = 0;

    public Unit()
    {
        Registers = new List<Register>()
        {
            new Register("rax", RegisterFlag.VOLATILE | RegisterFlag.RETURN),
            new Register("rbx"),
            new Register("rcx", RegisterFlag.VOLATILE),
            new Register("rdx", RegisterFlag.VOLATILE),
            new Register("rsi"),
            new Register("rdi"),
            new Register("rbp", RegisterFlag.SPECIALIZED),
            new Register("rsp", RegisterFlag.SPECIALIZED),
            new Register("r8"),
            new Register("r9"),
            new Register("r10"),
            new Register("r11"),
            new Register("r12", RegisterFlag.VOLATILE),
            new Register("r13", RegisterFlag.VOLATILE),
            new Register("r14", RegisterFlag.VOLATILE),
            new Register("r15", RegisterFlag.VOLATILE)
        };

        NonVolatileRegisters = Registers.FindAll(r => !r.Volatile);
        VolatileRegisters = Registers.FindAll(r => r.Volatile);
    }

    public void Append(Instruction instruction)
    {
        Instructions.Add(instruction);
    }

    public void Append(string instruction)
    {
        Builder.Append(instruction);
        Builder.AppendLine();
    }

    public void Build(Instruction instruction)
    {
        instruction.Build(this);
    }
    
    public void AddHandle(Handle handle)
    {
        Handles.Add(handle);
    }

    public RegisterHandle? TryGetCached(Quantum<Handle> handle)
    {
        var register = Registers.Find(r => (r.Value != null) ? r.Value.Value == handle.Value : false);
        
        if (register != null)
        {
            return new RegisterHandle(register);
        }

        return null;
    }

    public Register GetNextRegister()
    {
        var register = VolatileRegisters.Find(r => r.IsAvailable(this) && !Flag.Has(r.Flags, RegisterFlag.RETURN));

        if (register != null)
        {
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsAvailable(this) && !Flag.Has(r.Flags, RegisterFlag.RETURN));

        if (register != null)
        {
            return register;
        }

        throw new NotImplementedException("Couldn't find available register");
    }

    public Register GetStandardReturnRegister()
    {
        return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.RETURN)) ?? throw new Exception("Architecture didn't have return register?");
    }

    public void Simulate(Action<Instruction> action)
    {
        Position = 0;
        
        foreach (var instruction in Instructions)
        {
            action(instruction);
            Position++;
        }

        Registers.ForEach(r => r.Value = null);
    }

    public void Cache(Variable variable, Quantum<Handle> handle, bool invalidate)
    {
        var handles = GetVariableHandles(variable);
        var position = handles.FindIndex(0, handles.Count, h => h.Value.Lifetime.Start >= Position);

        var value = handle.Value;
        value.Lifetime.Start = Position;

        if (position == -1)
        {
            // Crop the lifetime of the previous handle
            var index = handles.Count - 1;

            if (index != -1)
            {
                handles[index].Value.Lifetime.End = Position;
            }

            handles.Add(handle);
        }
        else
        {
            // Crop the lifetime of the previous handle
            var index = position - 1;

            if (index != -1)
            {
                handles[index].Value.Lifetime.End = Position;
            }

            value.Lifetime.End = handles[position].Value.Lifetime.Start;
            handles.Insert(position, handle);
        }
    }

    public void Cache(object constant, Quantum<Handle> value)
    {
        var handles = GetConstantHandles(constant);
        value.Value.Lifetime.Start = Position;

        handles.Add(value);
    }

    public List<Quantum<Handle>> GetVariableHandles(Variable variable)
    {
        if (Variables.TryGetValue(variable, out List<Quantum<Handle>>? elements))
        {
            if (elements == null)
            {
                throw new Exception("Variable reference list was null");
            }

            return elements;
        }
        else
        {
            var handles = new List<Quantum<Handle>>();
            Variables.Add(variable, handles);

            return handles;
        }
    }

    public List<Quantum<Handle>> GetConstantHandles(object constant)
    {
        if (Constants.TryGetValue(constant, out List<Quantum<Handle>>? elements))
        {
            if (elements == null)
            {
                throw new Exception("Constant reference list was null");
            }

            return elements;
        }
        else
        {
            var handles = new List<Quantum<Handle>>();
            Constants.Add(constant, handles);

            return handles;
        }
    }

    public List<Quantum<Handle>> GetValidVariableHandles(Variable variable)
    {
        var handles = GetVariableHandles(variable).FindAll(h => h.Value.IsAlive(Position));
        handles.Reverse();
        
        return handles;
    }

    public List<Quantum<Handle>> GetValidConstantHandles(object constant)
    {
        var handles = GetConstantHandles(constant).FindAll(h => h.Value.IsAlive(Position));
        handles.Reverse();
        
        return handles;
    }

    public string Export()
    {
        return Builder.ToString().Replace("\n\n", "\n");
    }
}