using System.Collections.Generic;
 
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

    private static void SimulateVariableLoads(Unit unit)
    {
        unit.Simulate(i => 
        {
            if (i is GetVariableInstruction load)
            {
                var handles = unit.GetValidVariableHandles(load.Variable);

                if (handles.Count > 0)
                {
                    load.Connect(handles[0]);
                    //load.Result.EntangleTo(handles[0]);
                    //handles[0].Entangle(load.Result);
                }
                else
                {
                    load.Result.Set(References.CreateVariableHandle(unit, load.Variable));
                    unit.Cache(load.Variable, load.Result, false);
                }
            }
        });
    }

    private static void SimulateConstantLoads(Unit unit)
    {
        unit.Simulate(i => 
        {
            if (i is GetConstantInstruction load)
            {
                var handles = unit.GetValidConstantHandles(load.Value);

                if (handles.Count > 0)
                {
                    load.Connect(handles[0]);
                    //handles[0].Entangle(load.Result);
                }
                else
                {
                    load.Result.Set(References.CreateConstantNumber(unit, load.Value));
                    unit.Cache(load.Value, load.Result);
                }
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
        unit.Simulate(instruction =>
        {
            instruction.Weld();
        });

        unit.Simulate(i =>
        {
            if (i is ReturnInstruction instruction)
            {
                //instruction.Object.Set(new RegisterHandle(unit.GetStandardReturnRegister()));
                //unit.GetStandardReturnRegister().Value = instruction.Object;
                instruction.RedirectTo(new RegisterHandle(unit.GetStandardReturnRegister()));
            }
        });
    }

    public static Unit Channel(Unit unit)
    {
        SimulateMoves(unit);
        SimulateVariableLoads(unit);
        SimulateConstantLoads(unit);
        SimulateLifetimes(unit);
        ConnectReturnStatements(unit);

        return unit;
    }
}

/*

x = a + a
y = b - a

return x * y


r1 = GetVariable a
r2 = GetVariable a
o1 = Add { r1, r2 }

r3 = GetVariable b
r4 = GetVariable a
o2 = Sub { r3, r4 }

r5 = GetVariable x
r6 = GetVariable y
o3 = Mul { r5, r6 }

Return o3
--------------------------
r1 = [rbp+8] => mov rax, [rbp+8]
o1 = Add { r1, r1 } => lea rcx, [rax+rax]

r3 = [rbp+12] => mov rdx, [rbp+12]
r4 = r1 => rax
o2 = Sub { r3, r4 } => sub rdx, rax 

r5 = o1 => rcx
r6 = 21 => rdx
o3 = Mul { r5, r6 } => imul rcx, rdx

Return o3 => mov rax, rcx

mov rax, [rbp+8]
lea rcx, [rax+rax]
mov rdx, [rbp+12]
sub rdx, rax
imul rcx, rdx
mov rax, rcx
-------------------------------------------
r1 = GetVariable a => rcx
o1 = Add { r1, r1 } => lea rax, [rcx+rcx]

r3 = GetVariable b => rdx
r4 = GetVariable a => rcx
o2 = Sub { r3, r4 } => sub rdx, rcx

r5 = GetVariable x => rax
r6 = GetVariable y => rdx
o3 = Mul { r5, r6 } => imul rax, rdx

mov rcx, [rbp+8]
lea rax, [rcx+rcx]
mov rdx, [rbp+12]
sub rdx, rcx
imul rax, rdx

*/