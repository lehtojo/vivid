using System;

public static class Memory
{
    public static void ClearRegister(Unit unit, Register target)
    {
        if (target.Handle == null)
        {
            return;
        }

        var register = (Register?)null;

        if (target.IsVolatile)
        {
            register = unit.GetNextRegisterWithoutReleasing();
        }
        else
        {
            register = unit.GetNextNonVolatileRegister(false);
        }

        if (register == null)
        {
            unit.Release(target);
            return;
        }

        var destination = new RegisterHandle(register);
        
        var move = new MoveInstruction(unit, new Result(destination), target.Handle);
        move.Type = MoveType.RELOCATE;

        unit.Append(move);
    }
    
    public static Result CopyToRegister(Unit unit, Result result)
    {
        var register = unit.GetNextRegister();
        var destination = new Result(new RegisterHandle(register));
        
        return new MoveInstruction(unit, destination, result).Execute();
    }

    public static Result MoveToRegister(Unit unit, Result result)
    {
        var register = unit.GetNextRegister();
        var destination = new Result(new RegisterHandle(register));

        var move = new MoveInstruction(unit, destination, result);
        move.Type = MoveType.RELOCATE;
        
        return move.Execute();
    }

    public static void GetRegisterFor(Unit unit, Result value)
    {
        var register = unit.GetNextRegister();
        var handle = new RegisterHandle(register);

        if (!register.IsAvailable(unit.Position))
        {
            throw new NotImplementedException("Register values cannot be yet saved");
        }

        register.Handle = value;
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
                var register = unit.TryGetCached(result, !protect);
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

                var destination = MoveToRegister(unit, result);

                if (protect && !dying)
                {
                    return CopyToRegister(unit, destination);
                }

                return destination;
            }

            case HandleType.NONE:
            {
                throw new ApplicationException("Tried to convert none-handle");
            }

            default: return null;
        }
    }
}