using System;

public class LockStateInstruction : Instruction
{
   public Register Register { get; private set; }
   public bool IsLocked { get; private set; }

   public static LockStateInstruction Lock(Unit unit, Register register)
   {
      return new LockStateInstruction(unit, register, true);
   }

   public static LockStateInstruction Unlock(Unit unit, Register register)
   {
      return new LockStateInstruction(unit, register, false);
   }

   private LockStateInstruction(Unit unit, Register register, bool locked) : base(unit)
   {
      Register = register;
      IsLocked = locked;
      Description = (IsLocked ? "Lock" : "Unlock") + $" '{register}'";
   }

   public override void OnSimulate()
   {
      Register.IsLocked = IsLocked;
   }

   public override void OnBuild()
   {
      Register.IsLocked = IsLocked;
   }

   public override Result GetDestinationDependency()
   {
      throw new InvalidOperationException("Tried to redirect Set-Register-Lock-State-Instruction");
   }

   public override InstructionType GetInstructionType() 
   {
      return InstructionType.LOCK_STATE;
   }

   public override Result[] GetResultReferences()
   {
      return new[] { Result };
   }
}