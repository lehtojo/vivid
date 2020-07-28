using System.Linq;
using System;

public static class Lambdas
{
   public static Result Build(Unit unit, LambdaNode node)
   {
      if (!node.Lambda.Implementations.Any())
      {
         throw new ApplicationException("Missing implementation for lambda");
      }

      var implementation = node.Lambda.Implementations.First();
      var root = implementation.Node ?? throw new ApplicationException("Missing implementation for lambda");

      var captured_variables = Scope.GetAllNonLocalVariables(new Node[] { root }, implementation).ToList();
      var captured_member_variables = captured_variables.Where(v => v.IsMember).ToList();

      // Remove all member variables since they all share the same memory address which will be taken into account later
      captured_member_variables.ForEach(v => captured_variables.Remove(v));

      // Required memory is equal to memory required to store all the captured non-member variables, the function pointer and optionally the this pointer whose sizes are depedent on the chosen platform
      var required_memory = (long)captured_variables.Sum(v => v.Type!.ReferenceSize) + 
         (captured_member_variables.Count > 0 ? 2 : 1) * Assembler.Size.Bytes;

      // Allocate a memory structure which stores the lambda
      var lambda = Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, Types.LINK, new NumberNode(Assembler.Format, required_memory));

      // Store the function pointer first
      var function_pointer_location = new Result(new MemoryHandle(unit, lambda, 0), Assembler.Format);
      var function_pointer = new Result(new DataSectionHandle(node.Lambda.GetFullname(), true), Assembler.Format);

      unit.Append(new MoveInstruction(unit, function_pointer_location, function_pointer));

      var position = Assembler.Size.Bytes;

      // Should the current this pointer be captured as well?
      if (captured_member_variables.Any())
      {
         var source = References.GetVariable(unit, unit.Self!, AccessMode.READ);
         var destination = new Result(new MemoryHandle(unit, lambda, position), Assembler.Format);

         unit.Append(new MoveInstruction(unit, destination, source));

         position += Assembler.Size.Bytes;
      }

      // Store each captured variable
      foreach (var captured_variable in captured_variables)
      {
         var source = References.GetVariable(unit, captured_variable, AccessMode.READ);
         var destination = new Result(new MemoryHandle(unit, lambda, position), Assembler.Format);

         unit.Append(new MoveInstruction(unit, destination, source));

         position += captured_variable.Type!.ReferenceSize;
      }

      return lambda;
   }
}