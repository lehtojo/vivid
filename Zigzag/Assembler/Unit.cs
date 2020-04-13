using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class Lifetime
{
    public int Start { get; set; } = -1;
    public int End { get; set; } = -1;

    public void Reset()
    {
        Start = -1;
        End = -1;
    }

    public bool IsActive(int position)
    {
        return position >= Start && (End == -1 || position <= End);
    }

    public Lifetime Clone()
    {
        return new Lifetime() {
            Start = Start,
            End = End
        };
    }
}

public enum UnitMode
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
        if (Register.Handle != null) 
        {
            throw new ApplicationException("During state restoration one of the registers was conflicted");
        }

        var current_handle = unit.GetCurrentVariableHandle(Variable);

        if (current_handle != null)
        {
            Register.Handle = current_handle;
            current_handle.Value = new RegisterHandle(Register);
        }
    }
}

public class Unit
{
    public bool Optimize = true;

    public FunctionImplementation Function { get; private set; }
    
    public List<Register> Registers { get; private set; } = new List<Register>();
    public List<Register> NonVolatileRegisters { get; private set; } = new List<Register>();
    public List<Register> VolatileRegisters { get; private set; } = new List<Register>();
    public List<Register> NonReservedRegisters { get; private set; } = new List<Register>();
    
    public List<Instruction> Instructions { get; private set; } = new List<Instruction>();
    
    private StringBuilder Builder { get; set; } = new StringBuilder();

    private int LabelIndex { get; set; } = 0;
    private int StringIndex { get; set; } = 0;

    private bool IsReindexingNeeded { get; set; } = false;

    public Result? Self { get; set; }

    private Instruction? Anchor { get; set; }
    public int Position { get; private set; } = -1;

    public Scope? Scope { get; set; }
    public UnitMode Mode { get; private set; } = UnitMode.READ_ONLY_MODE;

    public Unit(FunctionImplementation function)
    {
        Function = function;

        // 64-bit:
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

        // 32-bit:
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

        NonReservedRegisters = VolatileRegisters.FindAll(r => !r.IsReserved);
        NonReservedRegisters.AddRange(NonVolatileRegisters.FindAll(r => !r.IsReserved));
    }

    public void ExpectMode(UnitMode expected)
    {
        if (Mode != expected)
        {
            throw new InvalidOperationException("Unit mode didn't match the expected");
        }
    }

    public int GetCurrentVariableVersion(Variable variable)
    {
        return Math.Max(GetVariableHandles(variable).FindLastIndex(h => h.IsValid(Position)), 0);
    }

    /// <summary>
    /// Returns all variables currently inside registers that are needed at the given instruction position or later
    /// </summary>
    public List<VariableState> GetState(int at)
    {
        return Registers
            .FindAll(register => !register.IsAvailable(at) && (register.Handle?.Metadata.IsVariable ?? false))
            .Select(register => new VariableState(register.Handle!.Metadata.Variables.First().Variable, register)).ToList();
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

            IsReindexingNeeded = true;
        }

        instruction.Scope = Scope;
        instruction.Result.Lifetime.Start = instruction.Position;

        if (Mode == UnitMode.BUILD_MODE)
        {
            var previous = Anchor;
            
            Anchor = instruction;
            instruction.TryBuild();
            Anchor = previous;
        }
    }

    public void Write(string instruction)
    {
        ExpectMode(UnitMode.BUILD_MODE);

        Builder.Append(instruction);
        Builder.AppendLine();
    }

    public RegisterHandle? TryGetCached(Result handle, bool write)
    {
        var register = Registers.Find(r => (r.Handle != null) ? r.Handle.Value == handle.Value : false);
        
        if (register != null)
        {
            return new RegisterHandle(register);
        }

        return null;
    }

    public void Release(Register register)
    {
        var value = register.Handle;

        if (value == null || !value.IsReleasable())
        {
            return;
        }

        // Get all the variables that this value represents
        var attributes = value.Metadata.Variables;

        foreach (var attribute in attributes)
        {
            var destination = new Result(References.CreateVariableHandle(this, null, attribute.Variable));
            
            var move = new MoveInstruction(this, destination, value);
            move.Mode = MoveMode.RELOCATE;
            
            Append(move);
        }

        // Now the register is ready for use
        register.Reset();
    }

    public Label GetNextLabel()
    {
        return new Label(Function.Metadata!.GetFullname() + $"_L{LabelIndex++}");
    }

    public string GetNextString()
    {
        return $"S{StringIndex++}";
    }

#region Registers

    public Register? GetNextNonVolatileRegister(bool release = true)
    {
        var register = NonVolatileRegisters
            .Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null || !release)
        {
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            Release(register);
            return register;
        }

        return null;
    }

    public Register GetNextRegister()
    {
        var register = VolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsAvailable(Position) && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            return register;
        }

        register = VolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            /// TODO: Handle member variables, for example their base handle is not passed
            Release(register);
            return register;
        }

        register = NonVolatileRegisters.Find(r => r.IsReleasable && !(Function.Returns && r.IsReturnRegister) && !r.IsReserved);

        if (register != null)
        {
            /// TODO: Handle member variables, for example their base handle is not passed
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

    public void Reset()
    {
        Registers.ForEach(r => r.Reset(true));
    }

#endregion

#region Interaction

    public void Execute(UnitMode mode, Action action)
    {
        Mode = mode;
        Position = -1;

        try 
        {
            action();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: Unit simulation failed: {e}");
        }

        if (IsReindexingNeeded)
        {
            Reindex();
        }

        Mode = UnitMode.READ_ONLY_MODE;
    }

    public void Simulate(UnitMode mode, Action<Instruction> action)
    {
        Mode = mode;
        Position = 0;
        Scope = null;
        
        Reset();

        var instructions = new List<Instruction>(Instructions);
        
        foreach (var instruction in instructions)
        {
            try 
            {
                if (instruction.Scope == null)
                {
                    throw new ApplicationException("Instruction was missing its scope");
                }

                Anchor = instruction;

                if (Scope != instruction.Scope)
                {
                    Scope?.Exit();
                    instruction.Scope.Enter(this);
                }

                action(instruction);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"ERROR: Unit simulation failed: {e}");
                break;
            }
            
            Position++;
        }

        if (IsReindexingNeeded)
        {
            Reindex();
        }

        // Reset the state after this simulation
        Mode = UnitMode.READ_ONLY_MODE;
    }

#endregion

    public void Cache(Variable variable, Result result, bool invalidate)
    {
        // Get all cached versions of this variable
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
        return Scope?.GetVariableHandles(this, variable) ?? throw new ApplicationException("Couldn't get variable reference list");
    }

    public List<Result> GetConstantHandles(object constant)
    {
        return Scope?.GetConstantHandles(constant) ?? throw new ApplicationException("Couldn't get constant reference list");
    }

    public Result? GetCurrentVariableHandle(Variable variable)
    {
        var handles = GetVariableHandles(variable).FindAll(h => h.IsValid(Position));
        return handles.Count == 0 ? null : handles.Last();
    }

    public Result? GetCurrentConstantHandle(object constant)
    {
        var handles = GetConstantHandles(constant).FindAll(h => h.IsValid(Position));
        return handles.Count == 0 ? null : handles.Last();
    }

    public string Export()
    {
        return Builder.ToString();
    }

    private void Reindex()
    {
        Position = 0;

        foreach (var instruction in Instructions)
        {
            instruction.Position = Position++;
        }

        IsReindexingNeeded = false;
    }
}