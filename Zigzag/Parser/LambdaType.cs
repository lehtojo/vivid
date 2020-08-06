using System.Collections.Generic;

public class LambdaType : Type
{
   public List<Type?> Parameters { get; }
   public Type? ReturnType { get; }

   public LambdaType(List<Type?> parameters, Type? return_type) : base(string.Empty, AccessModifier.PUBLIC)
   {
      Parameters = parameters;
      ReturnType = return_type;
   }
}