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
        value.Value = handle;
    }

    public static Result Convert(Unit unit, Result result, bool move, params HandleType[] types)
    {
        return Convert(unit, result, types, move, false);
    }

    public static Result Convert(Unit unit, Result result, HandleType[] types, bool move, bool protect)
    {
        foreach (var type in types)
        {
            if (result.Value.Type == type)
            {
                return result;
            }

            var converted = TryConvert(unit, result, type, protect);

            if (converted != null)
            {
                if (move)
                {
                    result.Value = converted.Value;
                }

                return converted;
            }
        }

        throw new ArgumentException("Couldn't convert reference to the requested format");
    }

    private static Result? TryConvert(Unit unit, Result result, HandleType type, bool protect)
    {
        switch (type)
        {
            case HandleType.REGISTER:
            {
                var register = unit.TryGetCached(result);
                var dying = result.IsExpiring(unit.Position);

                if (register != null)
                {
                    if (protect && !dying)
                    {
                        return CopyToRegister(unit, new Result(register));
                    }
                    else
                    {
                        return new Result(register);
                    }
                }

                var destination = CopyToRegister(unit, result);

                if (protect && !dying)
                {
                    return CopyToRegister(unit, destination);
                }

                return destination;
            }

            default: return null;
        }
    }
}