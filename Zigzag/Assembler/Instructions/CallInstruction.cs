using System;
using System.Linq;
using System.Collections.Generic;

public class CallInstruction : Instruction
{
   public string Function { get; private set; }
   public CallingConvention Convention { get; private set; }
   public List<Instruction> ParameterInstructions { get; set; } = new List<Instruction>();
   public bool IsParameterInstructionListExtracted => IsBuilt;

   public CallInstruction(Unit unit, string function, CallingConvention convention, Type return_type) : base(unit)
   {
      Function = function;
      Convention = convention;
      Result.Format = return_type?.Format ?? Assembler.Format;
   }

   /// <summary>
   /// Iterates through the volatile registers and ensures that they don't contain any important values which are needed later
   /// </summary>
   private void ValidateEvacuation()
   {
      foreach (var register in Unit.VolatileRegisters)
      {
         if (!register.IsAvailable(Position + 1))
         {
            throw new ApplicationException("Detected failure in register evacuation");
         }
      }
   }

   private void ExecuteParameterInstructions()
   {
      Unit.Append(Memory.Relocate(Unit, ParameterInstructions.Select(i => i.To<MoveInstruction>()).ToList()));
      Unit.Append(new EvacuateInstruction(Unit, this));
   }

   public override void OnBuild()
   {
      ExecuteParameterInstructions();

      // Validate evacuation since it's very important to be correct
      ValidateEvacuation();

      Build($"call {Function}");

      // After a call all volatile registers might be changed
      Unit.VolatileRegisters.ForEach(r => r.Reset());

      // Returns value is always in the following handle
      var register = Result.Format.IsDecimal() ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();
      var source = new RegisterHandle(register);

      if (Result.IsEmpty)
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

         var move = new MoveInstruction(Unit, Result, new Result(source, Result.Format));

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
      // If this call follows the x64 calling convention, the parameter instructions' source values must be referenced so that they aren't overriden before this call
      if (!IsParameterInstructionListExtracted && Convention == CallingConvention.X64)
      {
          return ParameterInstructions
              .Where(i => i.Type == InstructionType.MOVE)
              .Select(i => i.To<DualParameterInstruction>().Second)
              .Concat(new Result[] { Result }).ToArray();
      }

      return new Result[] { Result };
   }
}