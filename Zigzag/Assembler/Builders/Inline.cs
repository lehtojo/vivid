using System;

public static class Inline
{
   public static Result Build(Unit unit, InlineNode node)
   {
      var result = new Result();
		
		Builders.Build(unit, node.Body);

      if (node.Result != null)
      {
         result = References.Get(unit, node.Result);
      }

      return result;
   }
}
