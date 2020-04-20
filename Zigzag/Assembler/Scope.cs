using System.Collections.Generic;
using System.Linq;
using System;

public struct VariableLoad
{
    public Variable Variable;
    public Result Reference;
}

public class Scope : IDisposable
{
    private List<VariableLoad> OuterLoads { get; set; } = new List<VariableLoad>();
    private HashSet<Variable> StartLoads { get; set; } = new HashSet<Variable>();
    private HashSet<Variable> EndLoads { get; set; } = new HashSet<Variable>();

    public Dictionary<Variable, List<Result>> Variables = new Dictionary<Variable, List<Result>>();
    public Dictionary<object, List<Result>> Constants = new Dictionary<object, List<Result>>();

    public List<Variable> ActiveVariables { get; set; } = new List<Variable>();

    public Unit? Unit { get; private set; }
    public Scope? Outer { get; private set; } = null;

    public Scope(Unit unit, IEnumerable<Variable>? active_variables = null)
    {
        ActiveVariables = active_variables?.ToList() ?? new List<Variable>();

        Enter(unit);
    }

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

    private bool ContainsPredictableVariable(Register register, out Variable? variable)
    {
        var handle = register.Handle;

        if (handle != null && handle.Metadata.PrimaryAttribute is VariableAttribute attribute && attribute.Variable.IsPredictable)
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

    public void Enter(Unit unit)
    {
        Unit = unit;

        // Save the outer scope so that this scope can be exited later
        if (unit.Scope != this)
        {
            Outer = unit.Scope;
        }

        // Look for variables in registers to protect during this scope
        /*foreach (var register in Unit.NonReservedRegisters)
        {
            if (ContainsPredictableVariable(register, out Variable? variable) && !ActiveVariables.Contains(variable!))
            {
                ActiveVariables.Add(variable!);
            }
        }*/

        // Detect new variables to load
        if (OuterLoads.Count != ActiveVariables.Count)
        {
            foreach (var variable in ActiveVariables)
            {
                if (!OuterLoads.Exists(l => l.Variable == variable))
                {
                    var reference = References.GetVariable(Unit, variable, AccessMode.READ);
                    var load = new VariableLoad { Variable = variable, Reference = reference };

                    OuterLoads.Add(load);
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
                var outer_reference = OuterLoads[i].Reference;

                if (outer_reference != null)
                {
                    var variable_handles = GetVariableHandles(unit, variable);

                    if (variable_handles.Count == 0)
                    {
                        var first_local = new Result(outer_reference.Value);
                        var variable_attribute = outer_reference.Metadata[variable];

                        first_local.Metadata.Attach(variable_attribute);

                        variable_handles.Add(first_local);
                    }
                    else
                    {
                        var first = variable_handles[0];
                        first.Value = outer_reference.Value;
                    }
                }

                if (!StartLoads.Contains(variable))
                {
                    // The current variable is an active one so it must stay protected during the whole scope
                    References.GetVariable(unit, variable, AccessMode.READ);

                    StartLoads.Add(variable);
                }
            }

            // Since a new scope has begun the current register variables must be reconnected
            foreach (var register in Unit.NonReservedRegisters)
            {
                if (ContainsPredictableVariable(register, out Variable? variable))
                {
                    var handles = GetVariableHandles(Unit, variable!);

                    if (handles.Count == 0)
                    {
                        register.Reset();
                        continue;
                    }

                    var current = handles.First();
                    current.Value = new RegisterHandle(register);
                    current.Metadata.Attach(register.Handle!.Metadata.SecondaryAttributes);

                    register.Handle = current;
                }
            }
        }
    }

    public int GetCurrentVariableVersion(Unit unit, Variable variable)
    {
        var global = Outer?.GetCurrentVariableVersion(unit, variable) ?? 0;
        var local = Math.Max(GetVariableHandles(unit, variable).FindLastIndex(h => h.IsValid(unit.Position)), 0);

        return global + local;
    }

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
                var version = GetCurrentVariableVersion(unit, variable);

                current.Metadata.Attach(new VariableAttribute(variable, version));

                Unit!.Cache(variable, current, true);

                return Variables[variable];
            }

            return handles;
        }
    }

    public List<Result> GetConstantHandles(object constant)
    {
        if (Constants.TryGetValue(constant, out List<Result>? elements))
        {
            if (elements == null)
            {
                throw new Exception("Constant reference list was null");
            }

            return elements;
        }
        else
        {
            var handles = new List<Result>();
            Constants.Add(constant, handles);

            return handles;
        }
    }

    public Result? GetCurrentVariableHandle(Unit unit, Variable variable)
    {
        var handles = GetVariableHandles(unit, variable).FindAll(h => h.IsValid(unit.Position));
        return handles.Count == 0 ? null : handles.Last();
    }

    public Result? GetCurrentConstantHandle(Unit unit, object constant)
    {
        var handles = GetConstantHandles(constant).FindAll(h => h.IsValid(unit.Position));
        return handles.Count == 0 ? null : handles.Last();
    }

    public void Exit()
    {
        if (Unit == null) throw new ApplicationException("Unit was not assigned to a scope or the scope was never entered");

        foreach (var variable in ActiveVariables)
        {
            if (!EndLoads.Contains(variable))
            {
                // The current variable is an active one so it must stay protected during the whole scope
                References.GetVariable(Unit, variable, AccessMode.READ);

                EndLoads.Add(variable);
            }
        }

        // Exit to the outer scope
        Unit.Scope = Outer ?? this;
    }

    public void Dispose()
    {
        if (Unit == null) throw new ApplicationException("Unit was not assigned to a scope, make sure the unit is given through the constructor");

        Exit();
    }
}