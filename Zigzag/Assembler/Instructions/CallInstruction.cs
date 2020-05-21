using System;

public class CallInstruction : Instruction
{
    public string Function { get; private set; }
    public CallingConvention Convention { get; private set; }
    public Instruction[] ParameterInstructions { get; set; } = new Instruction[0];

    public CallInstruction(Unit unit, string function, CallingConvention convention) : base(unit)
    {
        Function = function;
        Convention = convention;
    }

    /// <summary>
    /// Iterates through the volatile registers and ensures that they don't contain any important values which are needed later
    /// </summary>
    private void ValidateEvacuation()
    {
        foreach (var register in Unit.VolatileRegisters)
        {
            if (!register.IsAvailable(Position))
            {
                throw new ApplicationException("Detected failure in register evacuation");
            }
        }
    }

    public override void OnBuild()
    {
        // Validate evacuation since it's very important to be correct
        ValidateEvacuation();

        Build($"call {Function}");

        // After a call all volatile registers might be changed
        Unit.VolatileRegisters.ForEach(r => r.Reset());

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
            // Ensure that the destination register is empty
            if (Result.Value.Type == HandleType.REGISTER)
            {
                Memory.ClearRegister(Unit, Result.Value.To<RegisterHandle>().Register);
            }

            var move = new MoveInstruction(Unit, Result, new Result(source));
            
            // Configure the move so that this instruction's result is attached to the destination
            move.Type = MoveType.LOAD;

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