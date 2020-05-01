using System.Linq;

public static class Oracle
{
    private static void DifferentiateDependencies(Unit unit, Result result)
    {
        // Get all variable dependencies
        var dependencies = result.Metadata.Secondary
            .Where(a => a.Type == AttributeType.VARIABLE)
            .Select(a => (VariableAttribute)a);

        // Ensure there is dependencies
        if (dependencies.Count() == 0)
        {
            return;
        }

        // Duplicate the current result and share it between the dependencies
        var duplicate = new DuplicateInstruction(unit, result).Execute();

        foreach (var dependency in dependencies)
        {
            // Redirect the dependency to the result of the duplication
            unit.Cache(dependency.Variable, duplicate, true);

            duplicate.Metadata.Attach(new VariableAttribute(dependency.Variable));
        }

        // Attach the primary attribute since it's still valid
        duplicate.Metadata.Attach(result.Metadata.Primary!);
    }

    public static void DifferentiateTarget(Unit unit, Result result, Variable target)
    {
        // Duplicate the result and give it to the target
        var duplicate = new DuplicateInstruction(unit, result).Execute();

        // Redirect the dependency to the target
        unit.Cache(target, duplicate, true);

        duplicate.Metadata.Attach(new VariableAttribute(target));
    }

    /// <summary>
    /// Resolves all write dependencies in the given result
    /// </summary>
    private static void Resolve(Unit unit, Result result, Variable target)
    {
        var destination = (VariableAttribute?)result.Metadata.Primary;

        if (destination == null)
        {
            return;
        }

        if (destination.Variable == target)
        {
            DifferentiateDependencies(unit, result);
        }
    }

    private static void SimulateMoves(Unit unit, Instruction i)
    {
        if (i.Type == InstructionType.ASSIGN)
        {
            var instruction = (DualParameterInstruction)i;
            var destination = instruction.First;
            var source = instruction.Second;

            // Check if the destination is a variable
            if (destination.Metadata.Primary is VariableAttribute attribute && attribute.Variable.IsPredictable)
            {
                unit.Cache(attribute.Variable, instruction.Result, true);


                // The source value now contains the new value of the destination
                source.Metadata.Attach(new VariableAttribute(attribute.Variable));
            }
        }
    }

    private static bool IsPropertyOf(Variable expected, Result result)
    {
        return result.Metadata.Primary is VariableAttribute attribute && attribute.Variable == expected;
    }

    private static void SimulateLoads(Unit unit, Instruction instruction)
    {
        if (instruction is GetVariableInstruction v)
        {
            var handle = unit.GetCurrentVariableHandle(v.Variable);

            if (handle != null && (v.Mode == AccessMode.READ || IsPropertyOf(v.Variable, handle)))
            {
                v.Connect(handle);
            }
            else
            {
                v.Configure(References.CreateVariableHandle(unit, v.Self, v.Variable), new VariableAttribute(v.Variable));

                if (v.Mode != AccessMode.WRITE && v.Variable.IsPredictable)
                {
                    unit.Cache(v.Variable, v.Result, false);
                }
            }

            if (v.Mode == AccessMode.WRITE)
            {
                Resolve(unit, v.Result, v.Variable);
            }
        }
        else if (instruction is GetConstantInstruction c)
        {
            var handle = unit.GetCurrentConstantHandle(c.Value);

            c.Configure(References.CreateConstantNumber(unit, c.Value));
        }
    }

    private static void SimulateCaching(Unit unit)
    {
        unit.Simulate(UnitPhase.APPEND_MODE, i =>
        {
            SimulateMoves(unit, i);
            SimulateLoads(unit, i);
        });
    }

    public static void SimulateLifetimes(Unit unit)
    {
        unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
        {
            foreach (var result in instruction.GetAllUsedResults())
            {
                result.Lifetime.Reset();
            }
        });

        unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
        {
            foreach (var result in instruction.GetAllUsedResults())
            {
                result.Use(unit.Position);
            }
        });
    }

    private static void ConnectReturnStatements(Unit unit)
    {
        var functions = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);

        unit.Simulate(UnitPhase.READ_ONLY_MODE, i =>
        {
            if (i is ReturnInstruction instruction)
            {
                var return_register = unit.GetStandardReturnRegister();

                var start = instruction.GetRedirectionRoot().Position;
                var end = unit.Position;

                if ((return_register.Handle == null || !return_register.Handle.Lifetime.IsIntersecting(start, end)) && !functions.Any(f => f.Result.Lifetime.IsIntersecting(start, end)))
                {
                    instruction.Redirect(new RegisterHandle(return_register));
                }
            }
        });
    }

    public static void SimulateRegisterUsage(Unit unit)
    {
        var functions = unit.Instructions.FindAll(i => i.Type == InstructionType.CALL);

        unit.Simulate(UnitPhase.READ_ONLY_MODE, instruction =>
        {
            var result = instruction.Result;

            if (functions.Any(f => result.Lifetime.IsActive(f.Position) && result.Lifetime.Start != f.Position && result.Lifetime.End != f.Position) &&
                !(result.Value is RegisterHandle handle && !handle.Register.IsVolatile))
            {
                var start = instruction.GetRedirectionRoot().Position;
                var end = unit.Position;

                // Make sure redirection root is not in range
                var register = unit.GetNextNonVolatileRegister(start, end);

                if (register != null)
                {
                    instruction.Redirect(new RegisterHandle(register));
                    register.Handle = result;
                }
            }
        });
    }

    public static Unit Channel(Unit unit)
    {
        if (unit.Optimize)
        {
            SimulateCaching(unit);
        }

        SimulateLifetimes(unit);

        if (unit.Optimize)
        {
            ConnectReturnStatements(unit);
            SimulateRegisterUsage(unit);
        }

        return unit;
    }
}