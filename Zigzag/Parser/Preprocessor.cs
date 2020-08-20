using System;
using System.Globalization;

public static class Preprocessor
{
   private static object? GetValue(Node node)
   {
      return node.GetNodeType() switch
      {
         NodeType.NUMBER_NODE => node.To<NumberNode>().Value,
         NodeType.STRING_NODE => node.To<StringNode>().Text,
         NodeType.OPERATOR_NODE => EvaluateOperator(node.To<OperatorNode>()),
         _ => null
      };
   }

   private static object? TryGetValue(Node node)
   {
      return node.GetNodeType() switch
      {
         NodeType.NUMBER_NODE => node.To<NumberNode>().Value,
         NodeType.STRING_NODE => node.To<StringNode>().Text,
         NodeType.OPERATOR_NODE => TryEvaluateOperator(node.To<OperatorNode>()),
         _ => null
      };
   }

   public static bool? TryEvaluateOperator(OperatorNode comparison)
   {
      var left = TryGetValue(comparison.Left);
      var right = TryGetValue(comparison.Right);

      if (left == null || right == null)
      {
         return null;
      }

      if (comparison.Operator == Operators.EQUALS)
      {
         return Equals(left, right);
      }
      if (comparison.Operator == Operators.NOT_EQUALS)
      {
         return !Equals(left, right);
      }

      if (comparison.Operator == Operators.AND)
      {
         return Convert.ToInt64(left, CultureInfo.InvariantCulture) != 0 && 
                  Convert.ToInt64(right, CultureInfo.InvariantCulture) != 0;
      }
      if (comparison.Operator == Operators.OR)
      {
         return Convert.ToInt64(left, CultureInfo.InvariantCulture) != 0 || 
                  Convert.ToInt64(right, CultureInfo.InvariantCulture) != 0;
      }

      // The following comparisons need the left and right side values to be comparable
      if (!(left is IComparable x && right is IComparable y))
      {
         return null;
      }

      if (comparison.Operator == Operators.GREATER_THAN)
      {
         return x.CompareTo(y) > 0;
      }
      if (comparison.Operator == Operators.LESS_THAN)
      {
         return x.CompareTo(y) < 0;
      }
      if (comparison.Operator == Operators.GREATER_OR_EQUAL)
      {
         return x.CompareTo(y) >= 0;
      }
      if (comparison.Operator == Operators.LESS_OR_EQUAL)
      {
         return x.CompareTo(y) <= 0;
      }

      return null;
   }

   public static bool? EvaluateOperator(OperatorNode comparison)
   {
      var left = GetValue(comparison.Left);
      var right = GetValue(comparison.Right);

      if (left == null || right == null)
      {
         throw new ArgumentException("Could not resolve a comparison operand");
      }

      if (comparison.Operator == Operators.EQUALS)
      {
         return Equals(left, right);
      }
      if (comparison.Operator == Operators.NOT_EQUALS)
      {
         return !Equals(left, right);
      }

      if (comparison.Operator == Operators.AND)
      {
         return Convert.ToInt64(left, CultureInfo.InvariantCulture) != 0 && 
                  Convert.ToInt64(right, CultureInfo.InvariantCulture) != 0;
      }
      if (comparison.Operator == Operators.OR)
      {
         return Convert.ToInt64(left, CultureInfo.InvariantCulture) != 0 || 
                  Convert.ToInt64(right, CultureInfo.InvariantCulture) != 0;
      }

      // The following comparisons need the left and right side values to be comparable
      if (!(left is IComparable x && right is IComparable y))
      {
         throw new ArgumentException("One of the comparison operands was not comparable");
      }

      if (comparison.Operator == Operators.GREATER_THAN)
      {
         return x.CompareTo(y) > 0;
      }
      if (comparison.Operator == Operators.LESS_THAN)
      {
         return x.CompareTo(y) < 0;
      }
      if (comparison.Operator == Operators.GREATER_OR_EQUAL)
      {
         return x.CompareTo(y) >= 0;
      }
      if (comparison.Operator == Operators.LESS_OR_EQUAL)
      {
         return x.CompareTo(y) <= 0;
      }

      throw new ArgumentException("Unsupported comparison");
   }

   private static Context? Evaluate(IfNode conditional_node)
   {
      var value = Convert.ToInt64(GetValue(conditional_node.Condition), CultureInfo.InvariantCulture);

      if (value != 0)
      {
         // Evaluate the body of the if-statement
         EvaluateNode(conditional_node.Context, conditional_node.Body);

         return conditional_node.Context;
      }

      var successor = conditional_node.Successor;

      if (successor != null)
      {
         if (successor.Is(NodeType.ELSE_NODE))
         {
            var node = successor.To<ElseNode>();

            // Evaluate the body of the else-statement
            EvaluateNode(node.Context, node.Body);

            return node.Context;
         }

         return Evaluate((IfNode)successor);
      }

      return null;
   }

   private static void EvaluateNode(Context context, Node root)
   {
      foreach (var iterator in root)
      {
         if (iterator.Is(NodeType.IF_NODE))
         {
            var conditional_node = iterator.To<IfNode>();
            var result = Evaluate(conditional_node);

            if (result != null)
            {
               context.Merge(result);
            }
         }
      }
   }

   public static Status Evaluate(Context context, Node root)
   {
      // Apply constants and such
      Analyzer.Analyze(context);

      try 
      {
         EvaluateNode(context, root);
         return Status.OK;
      }
      catch (Exception e)
      {
         return Status.Error(e.Message);
      }
   }
}