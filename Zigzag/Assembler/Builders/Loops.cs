using System.Collections.Generic;
using System.Linq;
using System;

public class VariableUsageInfo
{
	public Variable Variable { get; }
	public Result? Reference { get; set; }
	public int Usages { get; }

	public VariableUsageInfo(Variable variable, int usages)
	{
		Variable = variable;
		Usages = usages;
	}
}

public static class Loops
{
   /// <summary>
   /// Returns whether the given variable is a local variable
   /// </summary>
   private static bool IsNonLocalVariable(Variable variable, params Context[] local_contexts)
   {
      return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
   }

   /// <summary>
   /// Analyzes how many times each variable in the given node tree is used and sorts the result as well
   /// </summary>
   private static Dictionary<Variable, int> GetNonLocalVariableUsageCount(Unit unit, Node root, params Context[] local_contexts)
   {
      var variables = new Dictionary<Variable, int>();
      var iterator = root.First;

      while (iterator != null)
      {
         if (iterator is VariableNode node && IsNonLocalVariable(node.Variable, local_contexts))
         {
            if (node.Variable.IsPredictable)
            {
               variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
            }
            else if (!node.Parent?.Is(NodeType.LINK_NODE) ?? false)
            {
               if (unit.Self == null)
               {
                  throw new ApplicationException("Detected an use of the this pointer but it was missing");
               }

               variables[unit.Self] = variables.GetValueOrDefault(unit.Self, 0) + 1;
            }
         }
         else
         {
            foreach (var usage in GetNonLocalVariableUsageCount(unit, iterator, local_contexts))
            {
               variables[usage.Key] = variables.GetValueOrDefault(usage.Key, 0) + usage.Value;
            }
         }

         iterator = iterator.Next;
      }

      return variables;
   }

   public static Result BuildControlInstruction(Unit unit, LoopControlNode node)
   {
      if (node.Loop == null)
      {
         throw new ApplicationException("Loop control instruction was not inside a loop");
      }

      if (node.Instruction == Keywords.STOP)
      {
         var exit = node.Loop.Exit ?? throw new NotImplementedException("Forever loops don't have exit labels yet");

         var symmetry_start = unit.Loops[node.Loop] ?? throw new ApplicationException("Loop was not registered to unit");
         var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

         // Restore the state after the body
         symmetry_end.Append();

         // Restore the state after the body
         unit.Append(symmetry_end);

         return new JumpInstruction(unit, exit).Execute();
      }
      else
      {
         throw new NotImplementedException("Loop control instruction not implemented yet");
      }
   }

   /// <summary>
   /// Returns info about variable usage in the given loop
   /// </summary>
   private static List<VariableUsageInfo> GetAllVariableUsages(Unit unit, LoopNode node)
   {
      // Get all non-local variables in the loop and their number of usages
      var variables = GetNonLocalVariableUsageCount(unit, node, node.StepsContext, node.BodyContext)
                     .Select(i => new VariableUsageInfo(i.Key, i.Value)).ToList();

      // Sort the variables based on their number of usages (most used variable first)
      variables.Sort((a, b) => -a.Usages.CompareTo(b.Usages));

      return variables;
   }

   /// <summary>
   /// Returns whether the given loop contains functions
   /// </summary>
   private static bool ContainsFunction(LoopNode node)
   {
      return node.Find(n => n.Is(NodeType.FUNCTION_NODE)) != null;
   }

   /// <summary>
   /// Tries to move most used loop variables into registers
   /// </summary>
   private static void CacheLoopVariables(Unit unit, LoopNode node)
   {
      var variables = GetAllVariableUsages(unit, node);

      // If the loop contains at least one function, the variables should be cached into non-volatile registers
      // (Otherwise there would be a lot of register moves trying to save the cached variables)
      var non_volatile_mode = ContainsFunction(node);

      unit.Append(new CacheVariablesInstruction(unit, node, variables, non_volatile_mode));
   }

   private static Result BuildForeverLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
   {
      var active_variables = Scope.GetAllActiveVariablesForScope(unit, new Node[] { loop }, loop.BodyContext.Parent!, loop.BodyContext);

      var state = unit.GetState(unit.Position);
      var result = (Result?)null;

      using (var scope = new Scope(unit, active_variables))
      {
         // Append the label where the loop will start
         unit.Append(start);

         var symmetry_start = new SymmetryStartInstruction(unit, active_variables);
         unit.Append(symmetry_start);

         // Register loop to the unit
         unit.Loops.Add(loop, symmetry_start);

         // Build the loop body
         result = Builders.Build(unit, loop.Body);

         var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

         // Restore the state after the body
         symmetry_end.Append();

         // Restore the state after the body
         unit.Append(symmetry_end);
      }

      unit.Set(state);

      return result;
   }

   private static Result BuildLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
   {
      var active_variables = Scope.GetAllActiveVariablesForScope(unit, new Node[] { loop }, loop.BodyContext.Parent!, loop.BodyContext);

      var state = unit.GetState(unit.Position);
      var result = (Result?)null;

      using (var scope = new Scope(unit, active_variables))
      {
         // Append the label where the loop will start
         unit.Append(start);

         var symmetry_start = new SymmetryStartInstruction(unit, active_variables);
         unit.Append(symmetry_start);

         // Register loop to the unit
         unit.Loops.Add(loop, symmetry_start);

         // Build the loop body
         result = Builders.Build(unit, loop.Body);

         if (!loop.IsForeverLoop)
         {
            // Build the loop action
            Builders.Build(unit, loop.Action);
         }

			var conditional_jump = (Instruction?)null;

         if (loop.Condition is OperatorNode end_condition)
         {
            var left = References.Get(unit, end_condition.Left);
            var right = References.Get(unit, end_condition.Right);

            // Compare the two operands
            unit.Append(new CompareInstruction(unit, left, right));

            // Jump to the start based on the comparison
            conditional_jump = new JumpInstruction(unit, (ComparisonOperator)end_condition.Operator, false, start.Label);
         }
			else
			{
				throw new ApplicationException("Loop type not implemented");
			}

         scope.AppendFinalizers = false;

         var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

         // Restore the state after the body
         symmetry_end.Append();

         // Restore the state after the body
         unit.Append(symmetry_end);

			unit.Append(conditional_jump);
      }

      unit.Set(state);

      return result;
   }

   private static Result BuildForeverLoop(Unit unit, LoopNode node)
   {
      var start = unit.GetNextLabel();

      // Get the current state of the unit for later recovery
      var recovery = new SaveStateInstruction(unit);
      unit.Append(recovery);

      // Initialize the loop
      CacheLoopVariables(unit, node);

      Scope.PrepareConditionallyChangingConstants(unit, node, node.StepsContext, node.BodyContext);
      unit.Append(new BranchInstruction(unit, new Node[] { node.Body }));

      // Build the loop body
      var result = BuildForeverLoopBody(unit, node, new LabelInstruction(unit, start));

      // Jump to the start of the loop
      unit.Append(new JumpInstruction(unit, start));

      // Recover the previous state
      unit.Append(new RestoreStateInstruction(unit, recovery));

      return result;
   }

   public static Result Build(Unit unit, LoopNode node)
   {
      if (node.IsForeverLoop)
      {
         return BuildForeverLoop(unit, node);
      }

      // Create the start and end label of the loop
      var start = unit.GetNextLabel();
      var end = unit.GetNextLabel();

      // Register the exit label to the loop for control keywords
      node.Exit = end;

      // Initialize the loop
      Builders.Build(unit, node.Initialization);

      // Try to cache loop variables
      CacheLoopVariables(unit, node);

      Scope.PrepareConditionallyChangingConstants(unit, node, node.BodyContext);
      unit.Append(new BranchInstruction(unit, new Node[] { node.Initialization, node.Condition, node.Action, node.Body }));

      if (node.Condition is OperatorNode start_condition)
      {
         var left = References.Get(unit, start_condition.Left);
         var right = References.Get(unit, start_condition.Right);

         // Compare the two operands
         unit.Append(new CompareInstruction(unit, left, right));

         // Jump to the end based on the comparison
         unit.Append(new JumpInstruction(unit, (ComparisonOperator)start_condition.Operator, true, end));
      }

      // Get the current state of the unit for later recovery
      var recovery = new SaveStateInstruction(unit);
      unit.Append(recovery);

      // Build the loop body
      var result = BuildLoopBody(unit, node, new LabelInstruction(unit, start));

      // Append the label where the loop ends
      unit.Append(new LabelInstruction(unit, end));

      // Recover the previous state
      unit.Append(new RestoreStateInstruction(unit, recovery));

      return result;
   }
}