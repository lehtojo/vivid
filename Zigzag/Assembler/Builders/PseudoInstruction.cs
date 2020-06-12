using System;

public abstract class PseudoInstruction : Instruction
{
   public PseudoInstruction(Unit unit) : base(unit) {}

   public override Result? GetDestinationDependency()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override Result[] GetResultReferences()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override int GetStackOffsetChange()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override void OnBuild()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override void OnPostBuild()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override void OnSimulate()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }

   public override string ToString()
   {
      throw new InvalidOperationException("Tried to build a pseudo-instruction");
   }
}