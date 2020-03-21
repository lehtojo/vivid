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

public enum UnitMode
{
    READ_ONLY_MODE,
    APPEND_MODE,
    BUILD_MODE
}

public class Unit
{
    public FunctionImplementation Function { get; private set; }
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
    public UnitMode Mode { get; private set; } = UnitMode.READ_ONLY_MODE;

    public Unit(FunctionImplementation function)
    {
        Function = function;
        /*Registers = new List<Register>()
        {
            new Register("rax", RegisterFlag.VOLATILE | RegisterFlag.RETURN),
            new Register("rbx"),
            new Register("rcx", RegisterFlag.VOLATILE),
            new Register("rdx", RegisterFlag.VOLATILE),
            new Register("rsi"),
            new Register("rdi"),
            new Register("rbp", RegisterFlag.RESERVED | RegisterFlag.BASE_POINTER),
            new Register("rsp", RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER),
            new Register("r8"),
            new Register("r9"),
            new Register("r10"),
            new Register("r11"),
            new Register("r12", RegisterFlag.VOLATILE),
            new Register("r13", RegisterFlag.VOLATILE),
            new Register("r14", RegisterFlag.VOLATILE),
            new Register("r15", RegisterFlag.VOLATILE)
        };*/

        Registers = new List<Register>()
        {
            new Register("eax", RegisterFlag.VOLATILE | RegisterFlag.RETURN),
            new Register("ebx"),
            new Register("ecx", RegisterFlag.VOLATILE),
            new Register("edx", RegisterFlag.VOLATILE),
            new Register("esi"),
            new Register("edi"),
            new Register("ebp", RegisterFlag.RESERVED | RegisterFlag.BASE_POINTER),
            new Register("esp", RegisterFlag.RESERVED | RegisterFlag.STACK_POINTER)
        };

        NonVolatileRegisters = Registers.FindAll(r => !r.IsVolatile);
        VolatileRegisters = Registers.FindAll(r => r.IsVolatile);
    }

    private void ExpectMode(UnitMode expected)
    {
        if (Mode != expected)
        {
            throw new InvalidOperationException("Unit mode didn't match the expected");
        }
    }

    public void Append(Instruction instruction)
    {
        ExpectMode(UnitMode.APPEND_MODE);
        
        Instructions.Add(instruction);
        instruction.Position = Instructions.Count - 1;
        instruction.Result.Lifetime.Start = instruction.Position;
    }

    public void Append(string instruction)
    {
        ExpectMode(UnitMode.BUILD_MODE);

        Builder.Append(instruction);
        Builder.AppendLine();
    }

    public void Build(Instruction instruction)
    {
        ExpectMode(UnitMode.BUILD_MODE);

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

            value.Value = destination.Value;
            register.Value = null;
        }
    }

    public Label GetNextLabel()
    {
        return new Label(Function.Metadata!.GetFullname() + $"_L{LabelIndex++}");
    }

    public Register? GetNextNonVolatileRegister()
    {
        var register = NonVolatileRegisters
            .Find(r => r.IsAvailable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        return register;
    }

    public Register GetNextRegister()
    {
        var register = VolatileRegisters.Find(r => r.IsAvailable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            return register;
        }

        register = VolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            Release(register);
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsAvailable(this) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

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

    public void Execute(UnitMode mode, Action action)
    {
        Mode = mode;

        try 
        {
            action();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: Unit simulation failed: {e}");
        }

        Mode = UnitMode.READ_ONLY_MODE;
    }

    public void Simulate(UnitMode mode, Action<Instruction> action)
    {
        Position = 0;
        Mode = mode;
        
        foreach (var instruction in Instructions)
        {
            try 
            {
                action(instruction);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"ERROR: Unit simulation failed: {e}");
                break;
            }
            
            Position++;
        }

        Mode = UnitMode.READ_ONLY_MODE;
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