using System;
using System.Collections.Generic;
using System.Linq;

public static class Conditionals
{
   /// <summary>
   /// Builds the body of an if-statement or an else-if-statement
   /// </summary>
   public static Result BuildBody(Unit unit, Context local_context, Node body)
   {
      var active_variables = Scope.GetAllActiveVariablesForScope(unit, body, local_context.Parent!, local_context);

      var state = unit.GetState(unit.Position);
      var result = (Result?)null;

      // Since this is a body of some statement is also has a scope
      using (var scope = new Scope(unit, active_variables))
      {
         // Merges all changes that happen in the scope with the outer scope
         var merge = new MergeScopeInstruction(unit, active_variables);

         // Build the body
         result = Builders.Build(unit, body);

         // Restore the state after the body
         unit.Append(merge);
      }

      unit.Set(state);

      return result;
   }

   /// <summary>
   /// Builds an if-statement or an else-if-statement
   /// </summary>
   private static Result Build(Unit unit, IfNode node, Node condition, LabelInstruction end)
   {
      //var left = References.Get(unit, condition.Left);
      //var right = References.Get(unit, condition.Right);

      // Compare the two operands
      //var comparison = new CompareInstruction(unit, left, right).Execute();

      // Set the next label to be the end label if there's no successor since then there wont be any other comparisons
      var interphase = node.Successor == null ? end.Label : unit.GetNextLabel();

      // Jump to the next label based on the comparison
      //unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)condition.Operator, true, interphase));
      BuildCondition(unit, node.Context.Parent!, condition, interphase);

      // Get the current state of the unit for later recovery
      var recovery = new SaveStateInstruction(unit);
      unit.Append(recovery);

      // Build the body of this if-statement
      var result = BuildBody(unit, node.Context, node.Body);

      // Recover the previous state
      unit.Append(new RestoreStateInstruction(unit, recovery));

      // If the if-statement body is executed it must skip the potential successors
      if (node.Successor != null)
      {
         // Skip the next successor from this if-statement's body and add the interphase label
         unit.Append(new JumpInstruction(unit, end.Label));
         unit.Append(new LabelInstruction(unit, interphase));

         // Build the successor
         return Conditionals.Build(unit, node.Successor, end);
      }

      return result;
   }

   private static Result Build(Unit unit, Node node, LabelInstruction end)
   {
      if (node is IfNode if_node)
      {
         return Build(unit, if_node, if_node.Condition, end);
      }
      else if (node is ElseNode else_node)
      {
         // Get the current state of the unit for later recovery
         var recovery = new SaveStateInstruction(unit);
         unit.Append(recovery);

         var result = BuildBody(unit, else_node.Context, node);

         // Recover the previous state
         unit.Append(new RestoreStateInstruction(unit, recovery));

         return result;
      }
      else
      {
         throw new ApplicationException("Successor of an if-statement wasn't an else-if-statement or an else-statement");
      }
   }

   public static Result Start(Unit unit, IfNode node)
   {
      Scope.PrepareConditionallyChangingConstants(unit, node);

      unit.Append(new PrepareForConditionalExecutionInstruction(unit, node.GetAllBranches()));

      var end = new LabelInstruction(unit, unit.GetNextLabel());
      var result = Build(unit, node, end);
      unit.Append(end);

      return result;
   }

   private static void BuildCondition(Unit unit, Context current_context, Node condition, Label failure)
   {
      var success = unit.GetNextLabel();

      var instructions = BuildCondition(unit, condition, success, failure);
      instructions.Add(new LabelInstruction(unit, success));

      // Remove all occurances of the following pattern from the instructions:
      // jmp [Label]
      // [Label]:
      for (var i = instructions.Count - 2; i >= 0; i--)
      {
         if (instructions[i].Is(InstructionType.JUMP) && instructions[i + 1].Is(InstructionType.LABEL))
         {
            var jump = instructions[i].To<JumpInstruction>();
            var label = instructions[i + 1].To<LabelInstruction>();

            if (!jump.IsConditional && jump.Label == label.Label)
            {
               instructions.RemoveAt(i);
            }
         }
      }

      // Replace all occurances of the following pattern in the instructions:
      // [Conditional jump] [Label 1]
      // jmp [Label 2]
      // [Label 1]:
      // =====================================
      // [Inverted conditional jump] [Label 2]
      // [Label 1]:
      for (var i = instructions.Count - 3; i >= 0; i--)
      {
         if (instructions[i].Is(InstructionType.JUMP) &&
            instructions[i + 1].Is(InstructionType.JUMP) &&
            instructions[i + 2].Is(InstructionType.LABEL))
         {
            var conditonal_jump = instructions[i].To<JumpInstruction>();
            var jump = instructions[i + 1].To<JumpInstruction>();
            var label = instructions[i + 2].To<LabelInstruction>();

            if (conditonal_jump.IsConditional && !jump.IsConditional && conditonal_jump.Label == label.Label && jump.Label != label.Label)
            {
               conditonal_jump.Invert();
               conditonal_jump.Label = jump.Label;

               instructions.RemoveAt(i + 1);
            }
         }
      }

      // Remove unused labels
      var labels = instructions.Where(i => i.Is(InstructionType.LABEL)).Select(i => i.To<LabelInstruction>()).ToList();
      var jumps = instructions.Where(i => i.Is(InstructionType.JUMP)).Select(j => j.To<JumpInstruction>());

      foreach (var label in labels)
      {
         // Check if any jump instruction uses the current label
         if (!jumps.Any(j => j.Label == label.Label))
         {
            // Since the label isn't used, it can be removed
            instructions.Remove(label);
         }
      }

      // Append all the instructions to the unit
      foreach (var instruction in instructions)
      {
         if (instruction.Is(InstructionType.PSEUDO_COMPARE))
         {
            instruction.To<PseudoCompareInstruction>().Append(current_context);
         }
         else
         {
            unit.Append(instruction);
         }
      }
   }

   private class PseudoCompareInstruction : PseudoInstruction
   {
		public Node Comparison { get; private set; }
      public Node Left => Comparison.First!;
      public Node Right => Comparison.Last!;
      public ComparisonOperator Operator { get; private set; }
      public Label Success { get; private set; }
      public Label Failure { get; private set; }

      public PseudoCompareInstruction(Unit unit, Node comparison, ComparisonOperator operation, Label success, Label failure) : base(unit)
      {
         Comparison = comparison;
         Operator = operation;
         Success = success;
         Failure = failure;
      }

      public void Append(Context current_context)
      {
         // Get the current state of the unit for later recovery
         var recovery = new SaveStateInstruction(Unit);
         Unit.Append(recovery);

         var active_variables = Scope.GetAllActiveVariablesForScope(Unit, Comparison, current_context);

         var state = Unit.GetState(Unit.Position);

         // Since this is a body of some statement is also has a scope
         using (var scope = new Scope(Unit, active_variables))
         {
            // Merges all changes that happen in the scope with the outer scope
            var merge = new MergeScopeInstruction(Unit, active_variables);

            // Build the body
            var left = References.Get(Unit, Left);
            var right = References.Get(Unit, Right);

            // Compare the two operands
            Unit.Append(new CompareInstruction(Unit, left, right));

            // Restore the state after the body
            Unit.Append(merge);
         }

         Unit.Set(state);

         // Recover the previous state
         Unit.Append(new RestoreStateInstruction(Unit, recovery));
      }

      public override InstructionType GetInstructionType()
      {
         return InstructionType.PSEUDO_COMPARE;
      }
   }

   private static List<Instruction> BuildCondition(Unit unit, Node condition, Label success, Label failure)
   {
      if (condition.Is(NodeType.OPERATOR_NODE))
      {
         var operation = condition.To<OperatorNode>();

         if (operation.Operator.Type == OperatorType.LOGIC)
         {
            return BuildLogicalCondition(unit, operation, success, failure);
         }
         else if (operation.Operator.Type == OperatorType.COMPARISON)
         {
            return BuildComparison(unit, operation, success, failure);
         }
         else
         {
            throw new ApplicationException("Unsupported operator encountered while building a conditional statement");
         }
      }
      else if (condition.Is(NodeType.CONTENT_NODE))
      {
         return BuildCondition(unit, condition.First ?? throw new ApplicationException("Encountered an empty parenthesis while building a condition"), success, failure);
      }
      else
      {
         throw new ApplicationException("Comparing to zero in conditional statements is not yet automatic");
      }
   }

   private static List<Instruction> BuildComparison(Unit unit, OperatorNode condition, Label success, Label failure)
   {
      return new List<Instruction>()
      {
         new PseudoCompareInstruction
         (
            unit,
            condition,
            (ComparisonOperator)condition.Operator,
            success,
            failure
         ),
         new JumpInstruction(unit, (ComparisonOperator)condition.Operator, false, success),
         new JumpInstruction(unit, failure)
      };
   }

   private static List<Instruction> BuildLogicalCondition(Unit unit, OperatorNode condition, Label success, Label failure)
   {
      var instructions = new List<Instruction>();
      var interphase = unit.GetNextLabel();

      if (condition.Operator == Operators.AND)
      {
         instructions.AddRange(BuildCondition(unit, condition.Left, interphase, failure));
         instructions.Add(new LabelInstruction(unit, interphase));
         instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
      }
      else if (condition.Operator == Operators.OR)
      {
         instructions.AddRange(BuildCondition(unit, condition.Left, success, interphase));
         instructions.Add(new LabelInstruction(unit, interphase));
         instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
      }
      else
      {
         throw new ApplicationException("Unsupported logical operator encountered while building a conditional statement");
      }

      return instructions;
   }
}