using System;

public class LambdaCallNode : Node, IType
{
   public Node Object => Previous!;
   public Node Parameters => this;

   public LambdaCallNode(Node parameters)
   {
      SetParameters(parameters);
   }

   public new Type? GetType()
   {
      var lambda = Object switch
      {
         VariableNode variable => (LambdaType?)variable.GetType(),
         LinkNode link => (LambdaType?)link.GetType(),
         _ => throw new ApplicationException("Invalid lambda call node configuration")
      };

      return lambda?.ReturnType;
   }

   public LambdaCallNode SetParameters(Node parameters)
   {
      var parameter = parameters.First;

      while (parameter != null)
      {
         var next = parameter.Next;
         Add(parameter);
         parameter = next;
      }

      return this;
   }
}