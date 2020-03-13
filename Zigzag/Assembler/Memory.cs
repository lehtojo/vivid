using System;
using System.Collections.Generic;
using System.Text;

public static class Memory
{
    private static Quantum<Handle> ToRegister(Unit unit, Quantum<Handle> handle)
    {
        var register = unit.GetNextRegister();
        var destination = new Quantum<Handle>(new RegisterHandle(register));
        
        unit.Build(new MoveInstruction(destination, handle));

        return destination;
    }

    private static Quantum<Handle> Duplicate(Unit unit, RegisterHandle handle)
    {
        var register = unit.GetNextRegister();
        var destination = new Quantum<Handle>(new RegisterHandle(register));

        unit.Build(new MoveInstruction(destination, new Quantum<Handle>(handle)));

        return destination;
    }

    public static void GetRegisterFor(Unit unit, Quantum<Handle> value)
    {
        var register = unit.GetNextRegister();
        var handle = new RegisterHandle(register);

        if (register.Value != null)
        {
            throw new NotImplementedException("Register values cannot be yet saved");
        }

        register.Value = value;
        value.Set(handle);
    }

    public static Quantum<Handle> Convert(Unit unit, Quantum<Handle> handle, HandleType type, bool writable)
    {
        switch (type)
        {
            case HandleType.REGISTER:
            {
                var register = unit.TryGetCached(handle);
                var dying = handle.Value.IsDying(unit);

                if (register != null)
                {
                    if (writable && !dying)
                    {
                        return Duplicate(unit, register);
                    }
                    else
                    {
                        return new Quantum<Handle>(register);
                    }
                }

                var destination = ToRegister(unit, handle);

                if (writable && !dying)
                {
                    return Duplicate(unit, (RegisterHandle)destination.Value);
                }

                return destination;
            }

            default:
            {
                throw new ArgumentException("Couldn't convert reference to the requested format");
            }
        }
    }
}