using System;

public class CallInstruction : Instruction
{
    public string Function { get; private set; }

    public CallInstruction(Unit unit, string function) : base(unit)
    {
        Function = function;
    }

    public void Evacuate()
    {
        Unit.VolatileRegisters.ForEach(source => 
        {
            if (!source.IsAvailable(Position)) 
            {
                var destination = (Handle?)null;
                var register = Unit.GetNextNonVolatileRegister();
                
                if (register != null) 
                {
                    destination = new RegisterHandle(register);
                }
                else
                {
                    throw new NotImplementedException("Stack move required but not implemented");
                }

                var move = new MoveInstruction(Unit, new Result(destination), source.Handle!);
                move.Mode = MoveMode.RELOCATE;

                Unit.Append(move);

                source.Reset();
            }
        });
    }

    public override void Build()
    {
        // Move all values that are later needed to safe registers or to stack
        Evacuate();
        
        Build($"call {Function}");

        // Returns value is always in the following handle
        var register = Unit.GetStandardReturnRegister();
        var source = new RegisterHandle(register);

        if (Result.Empty)
        {
            // The result is not predefined so the result can just hold the standard return register
            Result.Value = source;
            register.Handle = Result;
        }
        else
        {
            var move = new MoveInstruction(Unit, Result, new Result(source));
            
            // Configure the move so that this instruction's result is attached to the destination
            move.Mode = MoveMode.LOAD;

            // The result is predefined so the value from the source handle must be moved to the predefined result
            Unit.Append(move, true);
        }
    }

    public override Result GetDestinationDependency()
    {
        return Result;   
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.CALL;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}