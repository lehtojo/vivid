using System;
using System.Globalization;

public class DivisionInstruction : DualParameterInstruction
{
   private const string SIGNED_INTEGER_DIVISION_INSTRUCTION = "idiv";
   private const string UNSIGNED_INTEGER_DIVISION_INSTRUCTION = "div";

   private const string SINGLE_PRECISION_DIVISION_INSTRUCTION = "divss";
   private const string DOUBLE_PRECISION_DIVISION_INSTRUCTION = "divsd";

   private const string DIVIDE_BY_TWO_INSTRUCTION = "sar";

   public bool IsModulus { get; private set; }
   public bool Assigns { get; private set; }

   public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format)
   {
      IsModulus = modulus;

      if (Assigns = assigns)
      {
         Result.Metadata = First.Metadata;
      }
   }

   private static long GetDivisorReciprocal(long divisor)
   {
      if (divisor == 1)
      {
         return 1;
      }

      var fraction = long.Parse((1.0 / divisor).ToString(CultureInfo.InvariantCulture).Split('.')[1], CultureInfo.InvariantCulture);
      var threshold = (long)Math.Pow(10, Math.Floor(Math.Log10(fraction)) + 1);
      var result = (long)0;

      for (var i = 0; i < 64; i++)
      {
         fraction *= 2;

         if (fraction >= threshold)
         {
            result |= 1L << (63 - i);
            fraction -= threshold;
         }
      }

      return result;
   }

   private Result CorrectNominatorLocation()
   {
      var register = Unit.GetNominatorRegister();
      var location = new RegisterHandle(register);

      if (!First.Value.Equals(location))
      {
         Memory.ClearRegister(Unit, location.Register);

         return new MoveInstruction(Unit, new Result(location, First.Format), First)
         {
            Type = Assigns ? MoveType.RELOCATE : MoveType.COPY

         }.Execute();
      }

      return First;
   }

   private void BuildModulus(Result denominator)
   {
      var destination = new RegisterHandle(Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!);

      Build(
         SIGNED_INTEGER_DIVISION_INSTRUCTION,
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
            new Result(destination, Assembler.Format),
            ParameterFlag.WRITE_ACCESS | ParameterFlag.DESTINATION | ParameterFlag.HIDDEN,
            HandleType.REGISTER
         )
      );
   }

   private void BuildDivision(Result denominator)
   {
      Build(
         SIGNED_INTEGER_DIVISION_INSTRUCTION,
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

   public override void OnSimulate()
   {
      if (Assigns && First.Metadata.IsPrimarilyVariable)
      {
			Unit.Set(First.Metadata.Variable, Result);
      }
   }

   private class ConstantDivision
   {
      public Result Other;
      public long Constant;

      public ConstantDivision(Result other, Result constant)
      {
         Other = other;
         Constant = (long)constant.Value.To<ConstantHandle>().Value;
      }
   }

   private ConstantDivision? TryGetConstantDivision()
   {
      if (Second.IsConstant)
      {
         return new ConstantDivision(First, Second);
      }
      else
      {
         return null;
      }
   }

   private static bool IsPowerOfTwo(long x)
   {
      return (x & (x - 1)) == 0;
   }
   
   public override void OnBuild()
   {
      // Handle decimal division separately
      if (Result.Format.IsDecimal())
      {
         var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_DIVISION_INSTRUCTION : DOUBLE_PRECISION_DIVISION_INSTRUCTION;
         var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

         Build(
            instruction,
            Assembler.Size,
            new InstructionParameter(
               First,
               flags,
               HandleType.MEDIA_REGISTER
            ),
            new InstructionParameter(
               Second,
               ParameterFlag.NONE,
               HandleType.MEDIA_REGISTER,
               HandleType.MEMORY
            )
         );

         return;
      }

      if (!IsModulus)
      {
         var division = TryGetConstantDivision();

         if (division != null && IsPowerOfTwo(division.Constant))
         {
            var count = new ConstantHandle((long)Math.Log2(division.Constant));

            Build(
               DIVIDE_BY_TWO_INSTRUCTION,
               new InstructionParameter(
                  division.Other,
                  ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE),
                  HandleType.REGISTER
               ),
               new InstructionParameter(
                  new Result(count, Assembler.Format),
                  ParameterFlag.NONE,
                  HandleType.CONSTANT
               )
            );

            return;
         }
      }

      var denominator = CorrectNominatorLocation();
      var remainder = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!;

      // Clear the remainder register
      Memory.Zero(Unit, remainder);

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