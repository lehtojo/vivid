using System.Collections.Generic;
using System.Linq;
using System;

public class VariableLoad
{
   public Variable Variable { get; private set; }
   public Result Reference { get; private set; }

   public VariableLoad(Variable variable, Result reference)
   {
      Variable = variable;
      Reference = reference;
   }
}

public sealed class Scope : IDisposable
{
   /// <summary>
   /// Returns whether the given variable is a non-local variable
   /// </summary>
   private static bool IsNonLocalVariable(Variable variable, Context[] local_contexts)
   {
      return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
   }

   /// <summary>
   /// Returns all variables in the given node tree that are not declared in the given local context
   /// </summary>
   public static IEnumerable<Variable> GetAllNonLocalVariables(Node[] roots, params Context[] local_contexts)
   {
      return roots.SelectMany(r => r.FindAll(n => n.Is(NodeType.VARIABLE_NODE))
               .Select(n => n.To<VariableNode>().Variable)
               .Where(v => v.IsPredictable && IsNonLocalVariable(v, local_contexts)))
               .Distinct();
   }

   /// <summary>
   /// Retrieves all variables that are visible to the specified context and are inside the current function
   /// </summary>
   private static List<Variable> GetAllContextVariables(Context? current)
   {
      var variables = new List<Variable>();

      while (current != null && !current.IsType)
      {
         variables.AddRange(current.Variables.Values);
         current = current.Parent;
      }

      return variables;
   }

   /// <summary>
   /// Returns all variables in the given context that are used before the current instruction position
   /// </summary>
   private static IEnumerable<Variable> GetAllActiveContextVariables(Unit unit, Context context)
   {
      return GetAllContextVariables(context).Where(v => unit.Instructions.Exists(i =>
         (i is GetVariableInstruction x && x.Variable == v) ||
         (i is RequireVariablesInstruction r && r.Variables.Contains(v))
      ));
   }

   /// </summary>
   /// Returns all variables that the scope must take care of
   /// </summary>
   public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Node[] roots, Context current_context, params Context[] scope_contexts)
   {
      return GetAllActiveContextVariables(unit, current_context)
            .Concat(GetAllNonLocalVariables(roots, scope_contexts))
            .Concat(unit.Scope!.ActiveVariables)
            .Distinct();
   }

   /// </summary>
   /// Returns all variables that the scope must take care of
   /// </summary>
   public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Node root, Context current_context, params Context[] scope_contexts)
   {
      return GetAllActiveContextVariables(unit, current_context)
            .Concat(GetAllNonLocalVariables(new Node[] { root }, scope_contexts))
            .Concat(unit.Scope!.ActiveVariables)
            .Distinct();
   }

   /// </summary>
   /// Returns all variables that the scope must take care of
   /// </summary>
   public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Context current_context)
   {
      return GetAllActiveContextVariables(unit, current_context)
            .Concat(unit.Scope!.ActiveVariables)
            .Distinct();
   }

   public static void PrepareConditionallyChangingConstants(Unit unit, Node root, params Context[] local_contexts)
   {
      // Find all variables inside the root node which are edited
      var edited_variables = GetAllNonLocalVariables(new Node[] { root }, local_contexts).Where(v => v.IsEditedInside(root));

      // All edited variables that are constants must be moved to registers or into memory
      foreach (var variable in edited_variables)
      {
         unit.Append(new LoadOnlyIfConstantInstruction(unit, variable)
         {
            Description = $"Loads the variable '{variable.Name}' into a register or memory if it's a constant"
         });
      }
   }

   public static void PrepareConditionallyChangingConstants(Unit unit, IfNode root)
   {
      var edited_variables = new List<Variable>();
      var iterator = (Node?)root;
      var context = root.Context;

      while (iterator != null)
      {
         // Find all variables inside the root node which are edited
         edited_variables.AddRange(GetAllNonLocalVariables(new Node[] { iterator }, context).Where(v => v.IsEditedInside(iterator)));

         if (iterator.Is(NodeType.ELSE_IF_NODE))
         {
            iterator = root.To<IfNode>().Successor;

            // Retrieve the context of the successor
            if (iterator != null)
            {
               if (iterator.Is(NodeType.ELSE_IF_NODE))
               {
                  context = iterator.To<ElseIfNode>().Context;
               }
               else
               {
                  context = iterator.To<ElseNode>().Context;
               }
            }
         }
         else
         {
            break;
         }
      }

      // Remove duplicate variables
      edited_variables = edited_variables.Distinct().ToList();

      // All edited variables that are constants must be moved to registers or into memory
      foreach (var variable in edited_variables)
      {
         unit.Append(new LoadOnlyIfConstantInstruction(unit, variable));
      }
   }

   private List<VariableLoad> Loads { get; set; } = new List<VariableLoad>();
   private HashSet<Variable> Initializers { get; set; } = new HashSet<Variable>();
   private HashSet<Variable> Finalizers { get; set; } = new HashSet<Variable>();

   public List<Variable> ActiveVariables { get; } = new List<Variable>();
   public Dictionary<Variable, Result> Variables { get; } = new Dictionary<Variable, Result>();
   public Dictionary<Variable, Result> TransitionHandles { get; } = new Dictionary<Variable, Result>();

   public Dictionary<object, List<Result>> Constants { get; } = new Dictionary<object, List<Result>>();

   public Unit? Unit { get; private set; }
   public Scope? Outer { get; private set; } = null;

   public int StackOffset { get; set; } = 0;


   public bool AppendFinalizers { get; set; } = true;

   /// <summary>
   /// Creates a scope with variables that are returned to their original locations once the scope is exited
   /// </summary>
   /// <param name="active_variables">Variables that must not be released</param>
   public Scope(Unit unit, IEnumerable<Variable>? active_variables = null)
   {
      ActiveVariables = active_variables?.ToList() ?? new List<Variable>();
      Enter(unit);
   }

   /// <summary>
   /// Returns whether the variable is used later looking from the current instruction position
   /// </summary>
   public bool IsUsedLater(Variable variable)
   {
      // Try to get the most recently used handle of the variable
      var current = GetCurrentVariableHandle(variable);

      // If there's no handle of the variable, it means that the outer scope doesn't have it either
      if (current == null)
      {
         return false;
      }

      // Check if the handle is used later or if the outer scope uses the variable later
      return current.Lifetime.End > Unit!.Position || (Outer?.IsUsedLater(variable) ?? false);
   }

   /// <summary>
   /// Returns whether the register contains a variable that is either a parameter or a local variable
   /// </summary>
   private static bool ContainsPredictableVariable(Register register, out Variable? variable)
   {
      var handle = register.Handle;

      if (handle != null && handle.Metadata.Primary is VariableAttribute attribute && attribute.Variable.IsPredictable)
      {
         variable = attribute.Variable;
         return true;
      }
      else
      {
         variable = null;
         return false;
      }
   }

   /// <summary>
   /// Sets or creates the handle for the specified variable
   /// </summary>
   private Result SetOrCreateTransitionHandle(Variable variable, Handle handle)
   {
      if (TransitionHandles.TryGetValue(variable, out Result? transition_handle))
      {
         transition_handle.Value = handle;
      }
      else
      {
         transition_handle = new Result(handle, variable.Type!.Format);
         transition_handle.Metadata.Attach(new VariableAttribute(variable));

         TransitionHandles.Add(variable, transition_handle);
      }

      // Update the current handle to the variable
      Variables[variable] = transition_handle;

      return transition_handle;
   }

   /// <summary>
   /// Switches the current scope to this scope
   /// </summary>
   public void Enter(Unit unit)
   {
      Unit = unit;
      StackOffset = Unit.StackOffset;

      // Remove old variable data
      Reset();

      // Save the outer scope so that this scope can be exited later
      if (unit.Scope != this)
      {
         Outer = unit.Scope;
      }

      // Detect new variables to load
      if (Loads.Count != ActiveVariables.Count)
      {
         foreach (var variable in ActiveVariables)
         {
            if (!Loads.Exists(l => l.Variable == variable))
            {
               var reference = References.GetVariable(Unit, variable, AccessMode.READ);
               var instruction = reference.Instruction!;
               var load = new VariableLoad(variable, reference);

               instruction.Description = "Keep variable active until the scope";

               Loads.Add(load);
            }
         }
      }

      // Switch the current unit scope to be this scope
      Unit.Scope = this;

      // Connect to the outer scope if it exists
      if (Outer != null)
      {
         for (var i = 0; i < ActiveVariables.Count; i++)
         {
            var variable = ActiveVariables[i];
            var external_handle = Loads[i].Reference;

            if (external_handle != null)
            {
               SetOrCreateTransitionHandle(variable, external_handle.Value);
            }

            if (!Initializers.Contains(variable))
            {
               // The current variable is an active one so it must stay protected during the whole scope
               References.GetVariable(unit, variable, AccessMode.READ).Instruction!.Description = "Prevent variable override";

               Initializers.Add(variable);
            }
         }

         // Since a new scope has begun the current register variables must be reconnected
         foreach (var register in Unit.NonReservedRegisters)
         {
            if (ContainsPredictableVariable(register, out Variable? variable))
            {
               // Check if the variable in the register matches the latest version that is given by outer loads
               if (Loads.Select(l => l.Reference).Any(r => register.Handle?.Equals(r) ?? false))
               {
                  if (!Variables.ContainsKey(variable!))
                  {
                     // Since the variable in the register is unknown it must not continue to the scope
                     register.Reset();
                     continue;
                  }

                  var current = Variables[variable!];
                  current.Value = new RegisterHandle(register);

                  register.Handle = current;
               }
               else
               {
                  // Since the variable in the register is unknown it must not continue to the scope
                  register.Reset();
               }
            }
         }
      }
      else if (unit.Function.Convention == CallingConvention.X64)
      {
         // Move all parameters to their expected registers since this is the first scope
         var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
         var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

         var register = (Register?)null;

         if (unit.Function.IsMember && !unit.Function.IsConstructor)
         {
            var this_pointer = unit.Function.GetVariable(Function.THIS_POINTER_IDENTIFIER) ?? throw new ApplicationException("This pointer was missing");

            register = standard_parameter_registers.Pop();

            if (register != null)
            {
               register.Handle = SetOrCreateTransitionHandle(this_pointer, new RegisterHandle(register));
            }
            else
            {
               throw new ApplicationException("This pointer shouldn't be in stack (x64 calling convention)");
            }
         }

         foreach (var parameter in unit.Function.Parameters)
         {
            var is_decimal = parameter.Type == Types.DECIMAL;

            // Determine the parameter register
            if (is_decimal)
            {
               register = decimal_parameter_registers.Pop();
            }
            else
            {
               register = standard_parameter_registers.Pop();
            }

            if (register != null)
            {
               register.Handle = SetOrCreateTransitionHandle(parameter, new RegisterHandle(register));
            }
            else
            {
               SetOrCreateTransitionHandle(parameter, References.CreateVariableHandle(Unit, null, parameter));
            }
         }
      }
   }

   /// <summary>
   /// Returns all handles to the given constant
   /// </summary>
   public List<Result> GetConstantHandles(object constant)
   {
      if (Constants.TryGetValue(constant, out List<Result>? elements))
      {
         return elements;
      }
      else
      {
         var handles = new List<Result>();
         Constants.Add(constant, handles);

         return handles;
      }
   }

   public Result? GetCurrentVariableHandle(Variable variable)
   {
      // Only predictable variables are allowed to be cached
      if (!variable.IsPredictable)
      {
         return null;
      }
      
      // First check if the variable handle list already exists
      if (Variables.TryGetValue(variable, out Result? handle))
      {
         return handle;
      }
      else
      {
         var source = Outer?.GetCurrentVariableHandle(variable);

         if (source != null)
         {
            Variables.Add(variable, source);
         }

         return source;
      }
   }

   /// <summary>
   /// Returns the current handle to the given constant
   /// </summary>
   public Result? GetCurrentConstantHandle(Unit unit, object constant)
   {
      var handles = GetConstantHandles(constant).FindAll(h => h.IsValid(unit.Position));
      return handles.Count == 0 ? null : handles.Last();
   }

   /// <summary>
   /// Switches back to the outer scope
   /// </summary>
   public void Exit()
   {
      if (Unit == null) throw new ApplicationException("Unit was not assigned to a scope or the scope was never entered");

      if (AppendFinalizers)
      {
         foreach (var variable in ActiveVariables)
         {
            if (!Finalizers.Contains(variable))
            {
               // The current variable is an active one so it must stay protected during the whole scope lifetime
               References.GetVariable(Unit, variable, AccessMode.READ).Instruction!.Description = "Keep variable active";

               Finalizers.Add(variable);
            }
         }
      }

      // Exit to the outer scope
      Unit.Scope = Outer ?? this;
   }

   public void Reset()
   {
      Variables.Clear();
   }

   /// <summary>
   /// Exits this scope after the using-statement
   /// </summary>
   public void Dispose()
   {
      if (Unit == null) throw new ApplicationException("Unit was not assigned to a scope, make sure the unit is given through the constructor");

      Exit();
   }
}