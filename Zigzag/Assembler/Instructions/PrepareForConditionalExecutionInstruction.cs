using System;

public class PrepareForConditionalExecutionInstruction : Instruction
{
   public Node[] Roots { get; private set; }

   public PrepareForConditionalExecutionInstruction(Unit unit, Node[] roots) : base(unit)
   {
      Roots = roots;
   }

   public override void OnBuild() {}

   public override Result? GetDestinationDependency()
   {
      throw new InvalidOperationException("Tried to redirect Prepare-For-Conditional-Execution-Instruction");
   }

   public override InstructionType GetInstructionType()
   {
      return InstructionType.PREPARE_FOR_CONDITIONAL_EXECUTION;
   }

   public override Result[] GetResultReferences()
   {
      return new Result[] { Result };
   }
}