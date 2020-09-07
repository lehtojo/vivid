using System;

public class LoadOnlyIfConstantInstruction : Instruction
{
   private Variable Variable { get; }

   public LoadOnlyIfConstantInstruction(Unit unit, Variable variable) : base(unit)
   {
      Variable = variable;
      Result.Format = Variable.Type!.Format;
   }

   public override void OnBuild()
   {
      var handle = new GetVariableInstruction(Unit, Variable, AccessMode.READ).Execute();

      if (handle == null)
      {
         throw new ApplicationException("Scope tried to edit an external variable which wasn't defined yet");
      }

      if (!handle.IsConstant) return;
      
      var recommendation = handle.GetRecommendation(Unit);
      var media_register = handle.Format.IsDecimal();

      Register? register = null;

      if (recommendation != null)
      {
         register = Memory.Consider(Unit, recommendation, media_register);
      }

      if (register == null)
      {
         register = media_register ? Unit.GetNextMediaRegisterWithoutReleasing()
            : Unit.GetNextRegisterWithoutReleasing();
      }

      Result.Value = register == null ? References.CreateVariableHandle(Unit, Variable) : new RegisterHandle(register);

      Unit.Append(new MoveInstruction(Unit, Result, handle)
      {
         Type = MoveType.RELOCATE
      });
   }

   public override Result? GetDestinationDependency()
   {
      return Result;
   }

   public override InstructionType GetInstructionType()
   {
      return InstructionType.LOAD_ONLY_IF_CONSTANT;
   }

   public override Result[] GetResultReferences()
   {
      return new[] { Result };
   }
}