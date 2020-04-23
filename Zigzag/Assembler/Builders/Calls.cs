using System;

public static class Calls
{
    public static Result Build(Unit unit, FunctionNode node)
    {
        return Build(unit, null, node.Parameters, node.Function!);
    }

    public static Result Build(Unit unit, Result self, FunctionNode node)
    {
        return Build(unit, self, node.Parameters, node.Function!);
    }
    
    public static Result Build(Unit unit, Node? parameters, FunctionImplementation implementation)
    {
        return Build(unit, null, parameters, implementation);
    }

    private static bool IsSelfCall(FunctionImplementation current, FunctionImplementation other)
    {
        return !other.IsConstructor && current.IsMember && other.IsMember && current.GetTypeParent() == other.GetTypeParent();
    }

    private static Size GetParameterSize(FunctionImplementation implementation, int current)
    {
        var mode = implementation.ParameterTypes[current].GetSize();

        // Stack doesn't accept bytes
        return mode == Size.BYTE ? Size.WORD : mode;
    }

    public static Result Build(Unit unit, Result? self, Node? parameters, FunctionImplementation implementation)
    {
        var metadata = implementation.Metadata!;
        var parameter = parameters?.Last;
        var parameter_count = 0;

        while (parameter != null)
        {
            var handle = References.Get(unit, parameter);
            unit.Append(new PushInstruction(unit, handle, GetParameterSize(implementation, parameter_count++)));
            
            parameter = parameter.Previous;
        }

        if (self == null && IsSelfCall(unit.Function, implementation))
        {
            self = new GetVariableInstruction(unit, null, unit.Self!, AccessMode.READ).Execute();
        }

        if (self != null)
        {
            unit.Append(new PushInstruction(unit, self, Assembler.Size));
            parameter_count++;
        }

        var result = new CallInstruction(unit, metadata.GetFullname()).Execute();

        // Remove the passed parameters from the stack
        StackMemoryInstruction.Shrink(unit, parameter_count * Assembler.Size.Bytes, implementation.IsResponsible).Execute();

        return result;
    }

    public static Result Build(Unit unit, Function function, params Node[] parameters)
    {
        for (int i = parameters.Length - 1; i >= 0 ; i--)
        {
            var handle = References.Get(unit, parameters[i]);

            var size = ((IType)parameters[i]).GetType()?.GetSize() ?? throw new ApplicationException("Couldn't get type of a parameter");
            size = size == Size.BYTE ? Size.WORD : size; // Stack doesn't accept bytes

            unit.Append(new PushInstruction(unit, handle, size));
        }
        
        var result = new CallInstruction(unit, function.GetFullname()).Execute();

        // Remove the passed parameters from the stack
        StackMemoryInstruction.Shrink(unit, parameters.Length * Assembler.Size.Bytes, function.IsResponsible).Execute();

        return result;
    }
}