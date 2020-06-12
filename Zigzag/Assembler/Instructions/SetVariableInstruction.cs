using System;

public class SetVariableInstruction : Instruction
{
   public Variable Variable { get; private set; }
   public Result Value { get; private set; }

   public SetVariableInstruction(Unit unit, Variable variable, Result value) : base(unit)
   {
      Variable = variable;
      Value = value;
      Result.Format = Value.Format;
   }

   public override Result? GetDestinationDependency()
   {
      throw new ApplicationException("Tried to redirect Set-Variable-Instruction");
   }

   public override void OnSimulate()
   {
      Unit.Scope!.Variables[Variable] = Value;
   }

   public override void OnBuild() {}

   public override InstructionType GetInstructionType()
   {
      return InstructionType.SET_VARIABLE;
   }

   public override Result[] GetResultReferences()
   {
      return new Result[] { Result, Value };
   }
}