using System.Collections.Generic;
using System.Linq;
using System;

public class Scope : IDisposable
{
    /// <summary>
    /// Returns whether the given variable is a local variable
    /// </summary>
    private static bool IsNonLocalVariable(Variable variable, Context[] local_contexts)
    {
        return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
    }

    /// <summary>
    /// Returns all variables in the given node tree that are not declared in the given local context
    /// </summary>
    private static IEnumerable<Variable> GetAllNonLocalVariables(Node root, params Context[] local_contexts)
    {
        return root.FindAll(n => n.Is(NodeType.VARIABLE_NODE))
                    .Select(n => n.To<VariableNode>().Variable)
                    .Where(v => v.IsPredictable && IsNonLocalVariable(v, local_contexts))
                    .Distinct();
    }

    /// <summary>
    /// Returns all variables in the given context that are used before the current instruction position
    /// </summary>
    private static IEnumerable<Variable> GetAllActiveContextVariables(Unit unit, Context context)
    {
        return context.Variables.Values.Where(v => unit.Instructions.Exists(i => i is GetVariableInstruction x && x.Variable == v));
    }

    /// </summary>
    /// Returns all variables that the scope must take care of
    /// </summary>
    public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Node root, Context current_context, params Context[] scope_contexts)
    {
        return GetAllActiveContextVariables(unit, current_context)
                .Concat(GetAllNonLocalVariables(root, scope_contexts))
                .Concat(unit.Scope!.ActiveVariables)
                .Distinct();
    }

    private struct VariableLoad
    {
        public Variable Variable;
        public Result Reference;
    }

    private List<VariableLoad> Loads { get; set; } = new List<VariableLoad>();
    private HashSet<Variable> Initializers { get; set; } = new HashSet<Variable>();
    private HashSet<Variable> Finalizers { get; set; } = new HashSet<Variable>();

    public Dictionary<Variable, List<Result>> Variables = new Dictionary<Variable, List<Result>>();
    public Dictionary<object, List<Result>> Constants = new Dictionary<object, List<Result>>();

    public List<Variable> ActiveVariables { get; set; } = new List<Variable>();

    public Unit? Unit { get; private set; }
    public Scope? Outer { get; private set; } = null;

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
        var handles = GetVariableHandles(Unit!, variable);
        var current = handles.Count > 0 ? handles.Last() : null;

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
    private bool ContainsPredictableVariable(Register register, out Variable? variable)
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
    /// Switches the current scope to this scope
    /// </summary>
    public void Enter(Unit unit)
    {
        Unit = unit;

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
                    var load = new VariableLoad { Variable = variable, Reference = reference };

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
                var outer_handle = Loads[i].Reference;

                if (outer_handle != null)
                {
                    var handles = GetVariableHandles(unit, variable);

                    if (handles.Count == 0)
                    {
                        var first_local = new Result(outer_handle.Value);
                        var variable_attribute = outer_handle.Metadata[variable];

                        first_local.Metadata.Attach(variable_attribute);

                        handles.Add(first_local);
                    }
                    else
                    {
                        // Connect the first variable handle to the outer handle since the first handle can be considered as a 'loader' of the variable
                        handles.First().Value = outer_handle.Value;
                    }
                }

                if (!Initializers.Contains(variable))
                {
                    // The current variable is an active one so it must stay protected during the whole scope
                    References.GetVariable(unit, variable, AccessMode.READ);

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
                        var handles = GetVariableHandles(Unit, variable!);

                        if (handles.Count == 0)
                        {
                            // Since the variable in the register is unknown it must not continue to the scope
                            register.Reset();
                            continue;
                        }

                        var current = handles.First();
                        current.Value = new RegisterHandle(register);
                        current.Metadata.Attach(register.Handle!.Metadata.Secondary);

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
    }

    /// <summary>
    /// Returns all handles to the given variable
    /// </summary>
    public List<Result> GetVariableHandles(Unit unit, Variable variable)
    {
        // First check if the variable handle list already exists
        if (Variables.TryGetValue(variable, out List<Result>? elements))
        {
            return elements;
        }
        else
        {
            var source = Outer?.GetCurrentVariableHandle(Unit!, variable);

            var handles = new List<Result>();
            Variables.Add(variable, handles);

            if (source != null)
            {
                // Create a reference to the outer reference and configure it so that this scope cannot change the outer reference
                var current = new Result();

                current.Metadata.Attach(new VariableAttribute(variable));

                Unit!.Cache(variable, current, true);

                return Variables[variable];
            }

            return handles;
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

    /// <summary>
    /// Returns the current handle to the given variable
    /// </summary>
    public Result? GetCurrentVariableHandle(Unit unit, Variable variable)
    {
        var handles = GetVariableHandles(unit, variable).FindAll(h => h.IsValid(unit.Position));
        return handles.Count == 0 ? null : handles.Last();
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

        foreach (var variable in ActiveVariables)
        {
            if (!Finalizers.Contains(variable))
            {
                // The current variable is an active one so it must stay protected during the whole scope lifetime
                References.GetVariable(Unit, variable, AccessMode.READ);

                Finalizers.Add(variable);
            }
        }

        // Exit to the outer scope
        Unit.Scope = Outer ?? this;
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