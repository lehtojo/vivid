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
    private Function Function { get; set; }
    private Dictionary<Variable, List<Result>> Variables = new Dictionary<Variable, List<Result>>();
    private Dictionary<object, List<Result>> Constants = new Dictionary<object, List<Result>>();
    private List<Register> Registers { get; set; } = new List<Register>();
    private List<Register> NonVolatileRegisters { get; set; } = new List<Register>();
    private List<Register> VolatileRegisters { get; set; } = new List<Register>();
    public List<Instruction> Instructions { get; private set; } = new List<Instruction>();
    private StringBuilder Builder { get; set; } = new StringBuilder();
    private int LabelIndex { get; set; } = 0;
    public Result? Self { get; set; }
    public int Position { get; private set; } = 0;

    public Unit(Function function)
    {
        Function = function;
        Registers = new List<Register>()
        {
            new Register("rax", RegisterFlag.VOLATILE | RegisterFlag.RETURN),
            new Register("rbx"),
            new Register("rcx", RegisterFlag.VOLATILE),
            new Register("rdx", RegisterFlag.VOLATILE),
            new Register("rsi"),
            new Register("rdi"),
            new Register("rbp", RegisterFlag.SPECIALIZED | RegisterFlag.BASE_POINTER),
            new Register("rsp", RegisterFlag.SPECIALIZED | RegisterFlag.STACK_POINTER),
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
        instruction.Position = Instructions.Count - 1;
    }

    public void Append(string instruction)
    {
        Builder.Append(instruction);
        Builder.AppendLine();
    }

    public void Build(Instruction instruction)
    {
        instruction.Build();
    }

    public RegisterHandle? TryGetCached(Result handle)
    {
        var register = Registers.Find(r => (r.Value != null) ? r.Value.Value == handle.Value : false);
        
        if (register != null)
        {
            return new RegisterHandle(register);
        }

        return null;
    }

    public Label GetNextLabel()
    {
        return new Label(Function.GetFullname() + $"_L{LabelIndex++}");
    }

    private void Release(Register register)
    {
        var value = register.Value;

        if (value == null)
        {
            throw new ArgumentException("Release called with an empty register");
        }

        if (value.Metadata is Variable variable)
        {
            var destination = new Result(References.CreateVariableHandle(this, null, variable));
            Build(new MoveInstruction(this, destination, value));

            value.Set(destination.Value);
            register.Value = null;
        }
    }

    public Register GetNextRegister()
    {
        var register = VolatileRegisters.Find(r => r.IsAvailable(this) && !Flag.Has(r.Flags, RegisterFlag.RETURN) && !Flag.Has(r.Flags, RegisterFlag.SPECIALIZED));

        if (register != null)
        {
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsAvailable(this) && !Flag.Has(r.Flags, RegisterFlag.RETURN) && !Flag.Has(r.Flags, RegisterFlag.SPECIALIZED));

        if (register != null)
        {
            return register;
        }

        register = Registers.Find(r => r.Releasable && !Flag.Has(r.Flags, RegisterFlag.RETURN) && !Flag.Has(r.Flags, RegisterFlag.SPECIALIZED));

        if (register != null)
        {
            Release(register);
            return register;
        }
        
        throw new NotImplementedException("Couldn't find available register");
    }

    public Register GetBasePointer()
    {
        return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.BASE_POINTER)) ?? throw new Exception("Architecture didn't have base pointer register?");
    }

    public Register GetStackPointer()
    {
        return Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.STACK_POINTER)) ?? throw new Exception("Architecture didn't have stack pointer register?");
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

    public void Cache(Variable variable, Result result, bool invalidate)
    {
        var handles = GetVariableHandles(variable);
        var position = handles.FindIndex(0, handles.Count, h => h.Lifetime.Start >= Position);

        result.Lifetime.Start = Position;

        if (position == -1)
        {
            // Crop the lifetime of the previous handle
            var index = handles.Count - 1;

            if (index != -1)
            {
                handles[index].Lifetime.End = Position;
            }

            handles.Add(result);
        }
        else
        {
            // Crop the lifetime of the previous handle
            var index = position - 1;

            if (index != -1)
            {
                handles[index].Lifetime.End = Position;
            }

            result.Lifetime.End = handles[position].Lifetime.Start;
            handles.Insert(position, result);
        }
    }

    public void Cache(object constant, Result value)
    {
        var handles = GetConstantHandles(constant);
        value.Lifetime.Start = Position;

        handles.Add(value);
    }

    public List<Result> GetVariableHandles(Variable variable)
    {
        if (Variables.TryGetValue(variable, out List<Result>? elements))
        {
            if (elements == null)
            {
                throw new Exception("Variable reference list was null");
            }

            return elements;
        }
        else
        {
            var handles = new List<Result>();
            Variables.Add(variable, handles);

            return handles;
        }
    }

    public List<Result> GetConstantHandles(object constant)
    {
        if (Constants.TryGetValue(constant, out List<Result>? elements))
        {
            if (elements == null)
            {
                throw new Exception("Constant reference list was null");
            }

            return elements;
        }
        else
        {
            var handles = new List<Result>();
            Constants.Add(constant, handles);

            return handles;
        }
    }

    public List<Result> GetValidVariableHandles(Variable variable)
    {
        var handles = GetVariableHandles(variable).FindAll(h => h.IsAlive(Position));
        handles.Reverse();
        
        return handles;
    }

    public List<Result> GetValidConstantHandles(object constant)
    {
        var handles = GetConstantHandles(constant).FindAll(h => h.IsAlive(Position));
        handles.Reverse();
        
        return handles;
    }

    public string Export()
    {
        return Builder.ToString().Replace("\n\n", "\n");
    }
}