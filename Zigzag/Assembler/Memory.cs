using System;

public static class Memory
{
    private static Result ToRegister(Unit unit, Result handle)
    {
        var register = unit.GetNextRegister();
        var destination = new Result(new RegisterHandle(register));
        
        unit.Build(new MoveInstruction(unit, destination, handle));

        return destination;
    }

    private static Result Duplicate(Unit unit, Result source)
    {
        var register = unit.GetNextRegister();
        var destination = new Result(new RegisterHandle(register));

        unit.Build(new MoveInstruction(unit, destination, new Result(source.Value)));

        return destination;
    }

    public static void GetRegisterFor(Unit unit, Result value)
    {
        var register = unit.GetNextRegister();
        var handle = new RegisterHandle(register);

        if (!register.IsAvailable(unit))
        {
            throw new NotImplementedException("Register values cannot be yet saved");
        }

        register.Value = value;
        value.Set(handle);
    }

    public static Result Convert(Unit unit, Result handle, HandleType type, bool writable)
    {
        switch (type)
        {
            case HandleType.REGISTER:
            {
                var register = unit.TryGetCached(handle);
                var dying = handle.IsDying(unit);

                if (register != null)
                {
                    if (writable && !dying)
                    {
                        return Duplicate(unit, new Result(register));
                    }
                    else
                    {
                        return new Result(register);
                    }
                }

                var destination = ToRegister(unit, handle);

                if (writable && !dying)
                {
                    return Duplicate(unit, destination);
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