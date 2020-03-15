public static class Oracle
{
    private static void SimulateMoves(Unit unit)
    {
        unit.Simulate(i => 
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
        unit.Simulate(i => 
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
                    unit.Cache(v.Variable, v.Result, false);
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
                unit.Self!.AddUsage(unit.Position);
            }
        });
    }

    private static void SimulateLifetimes(Unit unit)
    {
        unit.Simulate(instruction =>
        {
            foreach (var handle in instruction.GetHandles())
            {
                handle.AddUsage(unit.Position);
            }
        });
    }

    private static void ConnectReturnStatements(Unit unit)
    {
        unit.Simulate(i =>
        {
            if (i is ReturnInstruction instruction)
            {
                instruction.Redirect(new RegisterHandle(unit.GetStandardReturnRegister()));
            }
        });
    }

    public static Unit Channel(Unit unit)
    {
        SimulateMoves(unit);
        SimulateLoads(unit);
        SimulateLifetimes(unit);
        ConnectReturnStatements(unit);

        return unit;
    }
}