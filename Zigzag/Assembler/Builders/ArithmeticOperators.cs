using System;
using System.Globalization;

public static class ArithmeticOperators
{
   public static Result Build(Unit unit, IncrementNode node)
   {
      return BuildIncrementOperation(unit, node);
   }

   public static Result Build(Unit unit, DecrementNode node)
   {
      return BuildDecrementOperation(unit, node);
   }

   public static Result BuildNot(Unit unit, NotNode node)
   {
      return SingleParameterInstruction.Not(unit, References.Get(unit, node.Object)).Execute();
   }

   public static Result BuildNegate(Unit unit, NegateNode node)
   {
      return SingleParameterInstruction.Negate(unit, References.Get(unit, node.Object)).Execute();
   }

   public static Result Build(Unit unit, OperatorNode node)
   {
      var operation = node.Operator;

      if (Equals(operation, Operators.ADD))
      {
         return BuildAdditionOperator(unit, node);
      }
      if (Equals(operation, Operators.SUBTRACT))
      {
         return BuildSubtractionOperator(unit, node);
      }
      if (Equals(operation, Operators.MULTIPLY))
      {
         return BuildMultiplicationOperator(unit, node);
      }
      if (Equals(operation, Operators.DIVIDE))
      {
         return BuildDivisionOperator(unit, false, node);
      }
      if (Equals(operation, Operators.MODULUS))
      {
         return BuildDivisionOperator(unit, true, node);
      }
      if (Equals(operation, Operators.ASSIGN_ADD))
      {
         return BuildAdditionOperator(unit, node, true);
      }
      if (Equals(operation, Operators.ASSIGN_SUBTRACT))
      {
         return BuildSubtractionOperator(unit, node, true);
      }
      if (Equals(operation, Operators.ASSIGN_MULTIPLY))
      {
         return BuildMultiplicationOperator(unit, node, true);
      }
      if (Equals(operation, Operators.ASSIGN_DIVIDE))
      {
         return BuildDivisionOperator(unit, false, node, true);
      }
      if (Equals(operation, Operators.ASSIGN_MODULUS))
      {
         return BuildDivisionOperator(unit, true, node, true);
      }
      if (Equals(operation, Operators.ASSIGN))
      {
         return BuildAssignOperator(unit, node);
      }
      if (Equals(operation, Operators.BITWISE_AND) || Equals(operation, Operators.BITWISE_XOR) || Equals(operation, Operators.BITWISE_OR))
      {
         return BuildBitwiseOperator(unit, node);
      }
      if (Equals(operation, Operators.ASSIGN_AND) || Equals(operation, Operators.ASSIGN_XOR) || Equals(operation, Operators.ASSIGN_OR))
      {
         return BuildBitwiseOperator(unit, node, true);
      }

      throw new ArgumentException("Node not implemented yet");
   }

   private static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
   {
      var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
      var right = References.Get(unit, operation.Right);

      var number_type = operation.GetType()!.To<Number>().Type;

      return new AdditionInstruction(unit, left, right, number_type, assigns).Execute();
   }

   private static Result BuildIncrementOperation(Unit unit, IncrementNode increment)
   {
      var left = References.Get(unit, increment.Object, AccessMode.WRITE);
      var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));

      var number_type = increment.Object.GetType()!.To<Number>().Type;

      return new AdditionInstruction(unit, left, right, number_type, true).Execute();
   }

   private static Result BuildDecrementOperation(Unit unit, DecrementNode decrement)
   {
      var left = References.Get(unit, decrement.Object, AccessMode.WRITE);
      var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));

      var number_type = decrement.Object.GetType()!.To<Number>().Type;

      return new SubtractionInstruction(unit, left, right, number_type, true).Execute();
   }

   private static Result BuildSubtractionOperator(Unit unit, OperatorNode operation, bool assigns = false)
   {
      var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
      var right = References.Get(unit, operation.Right);

      var number_type = operation.GetType()!.To<Number>().Type;

      return new SubtractionInstruction(unit, left, right, number_type, assigns).Execute();
   }

   /// <summary>
   /// Returns whether the node represents a object located in memory
   private static bool IsComplexDestination(Node node)
   {
      return node.Is(NodeType.VARIABLE_NODE) && !node.To<VariableNode>().Variable.IsPredictable ||
               node.Is(NodeType.LINK_NODE) || node.Is(NodeType.OFFSET_NODE);
   }

   private static Result BuildMultiplicationOperator(Unit unit, OperatorNode operation, bool assigns = false)
   {
      var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
      var right = References.Get(unit, operation.Right);

      var number_type = operation.GetType()!.To<Number>().Type;

      var result = new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();

      if (IsComplexDestination(operation.Left) && assigns)
      {
         return new MoveInstruction(unit, left, result).Execute();
      }

      return result;
   }

   private static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
   {
      var number_type = operation.GetType()!.To<Number>().Type;

      if (!modulus && operation.Right.Is(NodeType.NUMBER_NODE) && operation.Right.To<NumberNode>().Value is long divisor && !IsPowerOfTwo(divisor))
      {
         return BuildConstantDivision(unit, operation.Left, divisor);
      }
      
      var right = References.Get(unit, operation.Right);
      var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);

      var result = new DivisionInstruction(unit, modulus, left, right, number_type, assigns).Execute();

      if (IsComplexDestination(operation.Left) && assigns)
      {
         return new MoveInstruction(unit, left, result).Execute();
      }

      return result;
   }

   private static Result BuildAssignOperator(Unit unit, OperatorNode node)
   {
      var left = References.Get(unit, node.Left, AccessMode.WRITE);
      var right = References.Get(unit, node.Right);

      if (node.Left.Is(NodeType.VARIABLE_NODE) && node.Left.To<VariableNode>().Variable.IsPredictable)
      {
         var variable = node.Left.To<VariableNode>().Variable;

         var instruction = new SetVariableInstruction(unit, variable, right);
         instruction.Value.Metadata.Attach(new VariableAttribute(variable));

         return instruction.Execute();
      }

      // Externally used variables need an immediate update 
      return new MoveInstruction(unit, left, right).Execute();
   }

   private static Result BuildBitwiseOperator(Unit unit, OperatorNode operation, bool assigns = false)
   {
      var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
      var right = References.Get(unit, operation.Right);

      var number_type = operation.GetType()!.To<Number>().Type;

      if (operation.Operator == Operators.BITWISE_AND)
      {
         return BitwiseInstruction.And(unit, left, right, number_type, assigns).Execute();
      }
      if (operation.Operator == Operators.BITWISE_XOR)
      {
         return BitwiseInstruction.Xor(unit, left, right, number_type, assigns).Execute();
      }
      if (operation.Operator == Operators.BITWISE_OR)
      {
         return BitwiseInstruction.Or(unit, left, right, number_type, assigns).Execute();
      }

      throw new InvalidOperationException("Tried to build bitwise operation from a node which didn't represent bitwise operation");
   }

   private static bool IsPowerOfTwo(long x)
   {
      return (x & (x - 1)) == 0;
   }

   private static long GetDivisorReciprocal(long divisor)
   {
      if (divisor == 1)
      {
         return 1;
      }

		var fraction = (decimal)1 / divisor;
      var result = (long)0;

      for (var i = 0; i < 64; i++)
      {
         fraction *= 2;

         if (fraction >= 1)
         {
            result |= 1L << (63 - i);
            fraction -= (int)fraction;
         }
      }

      return result + 1;
   }

   private static Result BuildConstantDivision(Unit unit, Node dividend, long divisor)
   {
		var first = References.Get(unit, dividend, AccessMode.READ);

		// Multiply the variable with the divisor's reciprocal
		var reciprocal = new ConstantHandle(GetDivisorReciprocal(divisor));
		var multiplication = new LongMultiplicationInstruction(unit, first, new Result(reciprocal, Assembler.Format), Assembler.Format).Execute();

		// The following offset fixes the result of the division when the result is negative by setting the offset's value to one if the result is negative, otherwise zero
		var offset = BitwiseInstruction.ShiftRight(unit, multiplication, new Result(new ConstantHandle(63L), Assembler.Format), multiplication.Format).Execute();

		// Fix the division by adding the offset to the multiplication
		var addition = new AdditionInstruction(unit, offset, multiplication, multiplication.Format, false).Execute();

		return addition;
   }
}