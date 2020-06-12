using System;

public class LoadOnlyIfConstantInstruction : Instruction
{
   private Variable Variable { get; set; }

   public LoadOnlyIfConstantInstruction(Unit unit, Variable variable) : base(unit)
   {
      Variable = variable;
      Result.Format = Variable.Type!.Format;
   }

   public override void OnBuild()
   {
      var handle = Unit.GetCurrentVariableHandle(Variable);

      if (handle == null)
      {
         throw new ApplicationException("Scope tried to edit an external variable which wasn't defined yet");
      }

      if (handle.IsConstant)
      {
         // Decide the destination if it isn't predefined
         if (Result.IsEmpty)
         {
            var register = Unit.GetNextRegisterWithoutReleasing();
            
            if (register == null)
            {
               // Couldn't find an available register so the constant variable must be moved into memory
               Result.Value = References.CreateVariableHandle(Unit, null, Variable);
            }
            else
            {
               Result.Value = new RegisterHandle(register);
            }
         }

         Unit.Append(new MoveInstruction(Unit, Result, handle)
         {
            Type = MoveType.RELOCATE
         });
      }
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
      return new Result[] { Result };
   }
}