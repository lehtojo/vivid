using System.Linq;

public static class Oracle
{
    private static void SimulateMoves(Unit unit)
    {
        unit.Simulate(UnitMode.READ_ONLY_MODE, i => 
        {
            if (i is AssignInstruction assign && 
                assign.First.Metadata is Variable variable)
            {
                unit.Cache(variable, assign.Result, true);
                assign.Second.Metadata = variable;
            }
        });
    }

    private static void SimulateLoads(Unit unit)
    {
        unit.Simulate(UnitMode.READ_ONLY_MODE, i => 
        {
            if (i is GetVariableInstruction v)
            {
                var handles = unit.GetValidVariableHandles(v.Variable);

                if (handles.Count > 0)
                {
                    v.Connect(handles[0]);
                }
                else
                {
                    v.SetSource(References.CreateVariableHandle(unit, v.Self, v.Variable));
                    
                    if (v.Variable.Category == VariableCategory.LOCAL ||
                        v.Variable.Category == VariableCategory.PARAMETER)
                    {
                        unit.Cache(v.Variable, v.Result, false);
                    }
                }
            }
            else if (i is GetConstantInstruction c)
            {
                var handles = unit.GetValidConstantHandles(c.Value);

                if (handles.Count > 0)
                {
                    //c.Connect(handles[0]);
                }
                else
                {
                    c.SetSource(References.CreateConstantNumber(unit, c.Value));
                    //unit.Cache(c.Value, c.Result);
                }
            }
            else if (i is GetSelfPointerInstruction s)
            {
                unit.Self!.Use(unit.Position);
            }
        });
    }

    private static void SimulateLifetimes(Unit unit)
    {
        unit.Simulate(UnitMode.READ_ONLY_MODE, instruction =>
        {
            foreach (var handle in instruction.GetResultReferences())
            {
                handle.Use(unit.Position);
            }
        });
    }

    private static void ConnectReturnStatements(Unit unit)
    {
        unit.Simulate(UnitMode.READ_ONLY_MODE, i =>
        {
            if (i is ReturnInstruction instruction)
            {
                instruction.Redirect(new RegisterHandle(unit.GetStandardReturnRegister()));
            }
        });
    }

    public static void SimulateRegisterUsage(Unit unit)
    {
        var functions = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);

        unit.Simulate(UnitMode.READ_ONLY_MODE, instruction => 
        {
            var result = instruction.Result;

            if (functions.Any(f => result.Lifetime.IsActive(f.Position) && result.Lifetime.Start != f.Position && result.Lifetime.End != f.Position) &&
                !(result.Value is RegisterHandle handle && !handle.Register.IsVolatile))
            {
                var register = unit.GetNextNonVolatileRegister();

                if (register != null)
                {
                    instruction.Redirect(new RegisterHandle(register));
                    register.Value = result;
                }
            }
        });
    }

    public static Unit Channel(Unit unit)
    {
        SimulateMoves(unit);
        SimulateLoads(unit);
        SimulateLifetimes(unit);
        ConnectReturnStatements(unit);
        SimulateRegisterUsage(unit);

        return unit;
    }
}