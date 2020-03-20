using System;

public static class Memory
{
    public static Result CopyToRegister(Unit unit, Result source)
    {
        var register = unit.GetNextRegister();
        var destination = new Result(new RegisterHandle(register));
        
        unit.Build(new MoveInstruction(unit, destination, source));

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

    public static Result Convert(Unit unit, Result result, bool move, params HandleType[] types)
    {
        return Convert(unit, result, types, move, false);
    }

    public static Result Convert(Unit unit, Result result, HandleType[] types, bool move, bool writable)
    {
        foreach (var type in types)
        {
            if (result.Value.Type == type)
            {
                return result;
            }

            var converted = TryConvert(unit, result, type, writable);

            if (converted != null)
            {
                if (move)
                {
                    result.Set(converted.Value);
                }

                return converted;
            }
        }

        throw new ArgumentException("Couldn't convert reference to the requested format");
    }

    private static Result? TryConvert(Unit unit, Result result, HandleType type, bool writable)
    {
        switch (type)
        {
            case HandleType.REGISTER:
            {
                var register = unit.TryGetCached(result);
                var dying = result.IsDying(unit.Position);

                if (register != null)
                {
                    if (writable && !dying)
                    {
                        return CopyToRegister(unit, new Result(register));
                    }
                    else
                    {
                        return new Result(register);
                    }
                }

                var destination = CopyToRegister(unit, result);

                if (writable && !dying)
                {
                    return CopyToRegister(unit, destination);
                }

                return destination;
            }

            default: return null;
        }
    }
}