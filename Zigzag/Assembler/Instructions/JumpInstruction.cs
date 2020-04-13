using System.Collections.Generic;

public class JumpInstruction : Instruction
{
    private static readonly Dictionary<ComparisonOperator, string> Instructions = new Dictionary<ComparisonOperator, string>();

    static JumpInstruction()
    {
        Instructions.Add(Operators.GREATER_THAN, "jg");
        Instructions.Add(Operators.GREATER_OR_EQUAL, "jge");
        Instructions.Add(Operators.LESS_THAN, "jl");
        Instructions.Add(Operators.LESS_OR_EQUAL, "jle");
        Instructions.Add(Operators.EQUALS, "je");
        Instructions.Add(Operators.NOT_EQUALS, "jne");
    }

    public Label Label { get; private set; }
    public ComparisonOperator? Comparator { get; private set; }

    public JumpInstruction(Unit unit, Label label) : base(unit)
    {
        Label = label;
        Comparator = null;
    }

    public JumpInstruction(Unit unit, Result comparison, ComparisonOperator comparator, bool invert, Label label) : base(unit)
    {
        Result.Join(comparison);

        Label = label;
        Comparator = invert ? comparator.Counterpart : comparator;    
    }

    public override void Build()
    {
        var instruction = Comparator == null ? "jmp" : Instructions[Comparator];
        Build($"{instruction} {Label.GetName()}");
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.JUMP;
    }

    public override Result? GetDestinationDependency()
    {
        return null;
    }
}