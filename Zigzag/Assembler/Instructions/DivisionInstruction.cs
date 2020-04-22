using System;

public class DivisionInstruction : DualParameterInstruction
{
    public bool IsModulus { get; private set; }
    public bool Assigns { get; private set; }

    public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, bool assigns) : base(unit, first, second)
    {
        IsModulus = modulus;

        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    private Result CorrectDenominatorLocation()
    {
        var register = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.DENOMINATOR)) ?? throw new ApplicationException("Architecture didn't have denominator register");
        var location = new RegisterHandle(register);

        if (!First.Value.Equals(location))
        {
            Memory.ClearRegister(Unit, location.Register);

            var move = new MoveInstruction(Unit, new Result(location), First);
            move.Type = MoveType.COPY;

            return move.Execute();
        }

        return First;
    }

    private void ClearRemainderRegister(Register register)
    {
        if (!register.IsAvailable(Unit.Position))
        {
            Memory.ClearRegister(Unit, register);
        }
    }

    private void BuildModulus(Result denominator)
    {
        var destination = new RegisterHandle(Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!);

        Build(
            "idiv",
            Assembler.Size,
            new InstructionParameter(
                denominator,
                ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                ParameterFlag.NONE,
                HandleType.REGISTER,
                HandleType.MEMORY
            ),
            new InstructionParameter(
                new Result(destination),
                ParameterFlag.WRITE_ACCESS | ParameterFlag.DESTINATION | ParameterFlag.HIDDEN,
                HandleType.REGISTER
            )
        );
    }

    private void BuildDivision(Result denominator)
    {
        Build(
            "idiv",
            Assembler.Size,
            new InstructionParameter(
                denominator,
                ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                ParameterFlag.NONE,
                HandleType.REGISTER,
                HandleType.MEMORY
            )
        );
    }

    public override void Build()
    {
        var denominator = CorrectDenominatorLocation();
        var remainder = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!;

        ClearRemainderRegister(remainder);

        using (new RegisterLock(remainder))
        {
            if (IsModulus)
            {
                BuildModulus(denominator);
            }
            else
            {
                BuildDivision(denominator);
            }
        }
    }

    public override Result GetDestinationDependency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.DIVISION;
    }
}