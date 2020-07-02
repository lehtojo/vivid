using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Component
{
    public abstract void Negate();
    
    public virtual Component? Add(Component other)
    {
        return null;
    }
    
    public virtual Component? Subtract(Component other)
    {
        return null;
    }
    
    public virtual Component? Multiply(Component other)
    {
        return null;
    }
    
    public virtual Component? Divide(Component other)
    {
        return null;
    }
    
    public static Component? operator +(Component left, Component right)
    {
        return left.Add(right);
    }
    
    public static Component? operator -(Component left, Component right)
    {
        return left.Subtract(right);
    }
    
    public static Component? operator *(Component left, Component right)
    {
        return left.Multiply(right);
    }
    
    public static Component? operator /(Component left, Component right)
    {
        return left.Divide(right);
    }
}

public class NumberComponent : Component
{
    public object Value { get; private set; }

    public NumberComponent(object value)
    {
        Value = value;
    }

    public override void Negate()
    {
        if (Value is long multiplier)
        {
            Value = -multiplier;
        }
        else
        {
            Value = -(double)Value;
        }
    }

    public override Component? Add(Component other)
    {
        if (other is NumberComponent number_component)
        {
            return new NumberComponent(Numbers.Add(Value, number_component.Value));
        }

        return null;
    }

    public override Component? Subtract(Component other)
    {
        if (other is NumberComponent number_component)
        {
            return new NumberComponent(Numbers.Subtract(Value, number_component.Value));
        }

        return null;
    }

    public override Component? Multiply(Component other)
    {
        return other switch
        {
            NumberComponent number_component => new NumberComponent(Numbers.Multiply(Value, number_component.Value)),
            VariableComponent variable_component => new VariableComponent(variable_component.Variable, 
                Numbers.Multiply(Value, variable_component.Multiplier)),
            _ => null
        };
    }
    
    public override Component? Divide(Component other)
    {
        return other switch
        {
            NumberComponent number_component => new NumberComponent(Numbers.Divide(Value, number_component.Value)),
            VariableComponent variable_component => new VariableComponent(variable_component.Variable, 
                Numbers.Divide(Value, variable_component.Multiplier)),
            _ => null
        };
    }

    public override bool Equals(object? other)
    {
        return other is NumberComponent component && Equals(component.Value, Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }
}

public class VariableComponent : Component
{
    public object Multiplier { get; private set;  }
    public int Order { get; }
    public Variable Variable { get; }

    public VariableComponent(Variable variable, object? multiplier = null, int order = 1)
    {
        Multiplier = multiplier ?? 1L;
        Variable = variable;
        Order = order;
    }
    
    public override void Negate()
    {
        if (Multiplier is long multiplier)
        {
            Multiplier = -multiplier;
        }
        else
        {
            Multiplier = -(double)Multiplier;
        }
    }
    
    public override Component? Add(Component other)
    {
        if (other is VariableComponent variable_component && Equals(Variable, variable_component.Variable) && Equals(Order, variable_component.Order))
        {
            return new VariableComponent(Variable, Numbers.Add(Multiplier, variable_component.Multiplier));
        }

        return null;
    }
    
    public override Component? Subtract(Component other)
    {
        if (other is VariableComponent variable_component && Equals(Variable, variable_component.Variable) && Equals(Order, variable_component.Order))
        {
            return new VariableComponent(Variable, Numbers.Subtract(Multiplier, variable_component.Multiplier));
        }

        return null;
    }
    
    public override Component? Multiply(Component other)
    {
        return other switch
        {
            VariableComponent variable_component when Equals(Variable, variable_component.Variable) =>
            new VariableComponent(Variable, Numbers.Multiply(Multiplier, variable_component.Multiplier), Order + variable_component.Order),
            NumberComponent number_component => new VariableComponent(Variable, Numbers.Multiply(Multiplier, number_component.Value), Order),
            _ => null
        };
    }
    
    public override Component? Divide(Component other)
    {
        if (other is VariableComponent variable_component && Equals(Variable, variable_component.Variable))
        {
            var order = Order - variable_component.Order;
            var multiplier = Numbers.Divide(Multiplier, variable_component.Multiplier);
            
            if (order == 0)
            {
                return new NumberComponent(multiplier);
            }

            return new VariableComponent(Variable, multiplier, order);
        }

        if (other is NumberComponent number_component)
        {
            return new VariableComponent(Variable, Numbers.Divide(Multiplier, number_component.Value), Order);
        }

        return null;
    }
    
    public override bool Equals(object? other)
    {
        return other is VariableComponent component && Equals(component.Multiplier, Multiplier) && Equals(component.Variable, Variable);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Multiplier, Variable);
    }
}

public class ComplexComponent : Component
{
    public Node Node { get; }
    public bool IsNegative { get; private set; }

    public override void Negate()
    {
        IsNegative = !IsNegative;
    }

    public ComplexComponent(Node node)
    {
        Node = node;
    }
}

public static class Analysis
{
    /// <summary>
    /// Creates a node tree representing the specified components
    /// </summary>
    /// <param name="components">Components representing an expression</param>
    /// <returns>Node tree representing the specified components</returns>
    private static Node Recreate(List<Component> components)
    {
        var result = Recreate(components.First());

        for (var i = 1; i < components.Count; i++)
        {
            var component = components[i];
            
            if (component is NumberComponent number_component)
            {
                if (Equals(number_component.Value, 0L) || Equals(number_component.Value, 0.0))
                {
                    continue;
                }

                var is_negative = number_component.Value is long a && a < 0L || number_component.Value is double b && b < 0.0;
                var number = new NumberNode(number_component.Value is long ? Assembler.Format : Format.DECIMAL, number_component.Value);

                result = is_negative ? new OperatorNode(Operators.SUBTRACT).SetOperands(result, number.Negate()) 
                    : new OperatorNode(Operators.ADD).SetOperands(result, number);
            }

            if (component is VariableComponent variable_component)
            {
                // When the multiplier is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
                if (Equals(variable_component.Multiplier, 0L) || Equals(variable_component.Multiplier, 0.0))
                {
                    continue;
                }

                var node = GetOrderedVariable(variable_component.Variable, variable_component.Order);

                // When the multiplier is exactly one (double), the multiplier can be ignored, meaning the inaccuracy of the comparison is expected
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (variable_component.Multiplier is double b && b != 1.0)
                {
                    node = new OperatorNode(Operators.MULTIPLY).SetOperands(
                        node, 
                        new NumberNode(Format.DECIMAL, b)
                    );
                }
                else if ((long) variable_component.Multiplier != 1L)
                {
                    node = new OperatorNode(Operators.MULTIPLY).SetOperands(
                        node,
                        new NumberNode(Assembler.Format, (long) variable_component.Multiplier)
                    );
                }
            
                result = new OperatorNode(Operators.ADD).SetOperands(result, node);
            }

            if (component is ComplexComponent complex_component)
            {
                result = new OperatorNode(complex_component.IsNegative ? Operators.SUBTRACT : Operators.ADD).SetOperands(
                    result,
                    complex_component.Node
                );
            }
        }

        return result;
    }
    
    /// <summary>
    /// Builds a node tree representing a variable with an order
    /// </summary>
    /// <param name="variable">Target variable</param>
    /// <param name="order">Order of the variable</param>
    private static Node GetOrderedVariable(Variable variable, int order)
    {
        if (order == 0)
        {
            return new NumberNode(Assembler.Format, 1L);    
        }
        
        var result = (Node)new VariableNode(variable);

        for (var i = 1; i < (int)Math.Abs(order); i++)
        {
            result = new OperatorNode(Operators.MULTIPLY).SetOperands(result, new VariableNode(variable));
        }

        if (order < 0)
        {
            result = new OperatorNode(Operators.DIVIDE).SetOperands(new NumberNode(Assembler.Format, 1L), result);
        }

        return result;
    }
    
    /// <summary>
    /// Creates a node tree representing the specified component
    /// </summary>
    /// <returns>Node tree representing the specified component</returns>
    private static Node Recreate(Component component)
    {
        if (component is NumberComponent number_component)
        {
            return new NumberNode(Assembler.Format, number_component.Value);
        }

        if (component is VariableComponent variable_component)
        {
            // When the multiplier is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (variable_component.Multiplier is double a && a == 0.0)
            {
                return new NumberNode(Format.DECIMAL, 0.0);
            }
            
            if ((long) variable_component.Multiplier == 0L)
            {
                return new NumberNode(Assembler.Format, 0L);
            }

            var result = GetOrderedVariable(variable_component.Variable, variable_component.Order);

            // When the multiplier is exactly one (double), the multiplier can be ignored, meaning the inaccuracy of the comparison is expected
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (variable_component.Multiplier is double b && b != 1.0)
            {
                return new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Format.DECIMAL, 1.0));
            }
            
            return (long) variable_component.Multiplier != 1L ? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Assembler.Format, variable_component.Multiplier)) : result;
        }

        if (component is ComplexComponent complex_component)
        {
            return complex_component.IsNegative ? new NegateNode(complex_component.Node) : complex_component.Node;
        }
        
        throw new NotImplementedException("Unsupported component encountered while recreating");
    }
    
    /// <summary>
    /// Negates the all the specified components using their internal negation method
    /// </summary>
    /// <param name="components">Components to negate</param>
    /// <returns>The specified components</returns>
    private static List<Component> Negate(List<Component> components)
    {
        components.ForEach(c => c.Negate());
        return components;
    }
    
    private static List<Component> CollectComponents(Node node)
    {
        var result = new List<Component>();
        
        if (node.Is(NodeType.NUMBER_NODE))
        {
            result.Add(new NumberComponent(node.To<NumberNode>().Value));
        }
        else if (node.Is(NodeType.VARIABLE_NODE))
        {
            result.Add(new VariableComponent(node.To<VariableNode>().Variable));
        }
        else if (node.Is(NodeType.OPERATOR_NODE))
        {
            result.AddRange(CollectComponents(node.To<OperatorNode>()));
        }
        else if (node.Is(NodeType.CONTENT_NODE))
        {
            if (!Equals(node.First, null))
            {
                result.AddRange(CollectComponents(node.First));
            }
        }
        else if (node.Is(NodeType.NEGATE_NODE))
        {
            result.AddRange(Negate(CollectComponents(node.First!)));
        }
        else
        {
            result.Add(new ComplexComponent(node));
        }

        return result;
    }
    
    private static List<Component> CollectComponents(OperatorNode node)
    {
        var left_components = CollectComponents(node.Left);
        var right_components = CollectComponents(node.Right);

        if (Equals(node.Operator, Operators.ADD))
        {
            return SimplifyAddition(left_components, right_components);
        }
        
        if (Equals(node.Operator, Operators.SUBTRACT))
        {
            return SimplifySubtraction(left_components, right_components);
        }
        
        if (Equals(node.Operator, Operators.MULTIPLY))
        {
            return SimplifyMultiplication(left_components, right_components);
        }
        
        if (Equals(node.Operator, Operators.DIVIDE))
        {
            return SimplifyDivision(left_components, right_components);
        }

        return new List<Component>
        {
            new ComplexComponent(
                new OperatorNode(node.Operator).SetOperands(
                    Recreate(left_components), Recreate(right_components)
                )
            )
        };
    }

    /// <summary>
    /// Tries to simplify the specified components
    /// </summary>
    /// <param name="components">Components to simplify</param>
    /// <returns>A simplified version of the components</returns>
    private static List<Component> Simplify(List<Component> components)
    {
        if (components.Count <= 1)
        {
            return components;
        }

        for (var i = 0; i < components.Count; i++)
        {
            var current = components[i];
            
            // Start iterating from the next component
            for (var j = i + 1; j < components.Count;)
            {
                var result = current + components[j];

                // Move to the next component if the two components could not be added together
                if (result == null)
                {
                    j++;
                    continue;
                }
                
                // Remove the components added together
                components.RemoveAt(j);
                components.RemoveAt(i);
                
                // Apply the changes
                components.Insert(i, result);
                current = result;
            }
        }

        return components;
    }
    
    /// <summary>
    /// Simplifies the addition between the specified operands
    /// </summary>
    /// <param name="left_components">Components of the left hand side</param>
    /// <param name="right_components">Components of the right hand side</param>
    /// <returns>Simplified version of the expression</returns>
    private static List<Component> SimplifyAddition(List<Component> left_components, List<Component> right_components)
    {
        return Simplify(left_components.Concat(right_components).ToList());
    }
    
    /// <summary>
    /// Simplifies the subtraction between the specified operands
    /// </summary>
    /// <param name="left_components">Components of the left hand side</param>
    /// <param name="right_components">Components of the right hand side</param>
    /// <returns>Simplified version of the expression</returns>
    private static List<Component> SimplifySubtraction(List<Component> left_components, List<Component> right_components)
    {
        Negate(right_components);

        return SimplifyAddition(left_components, right_components);
    }

    /// <summary>
    /// Simplifies the multiplication between the specified operands
    /// </summary>
    /// <param name="left_components">Components of the left hand side</param>
    /// <param name="right_components">Components of the right hand side</param>
    /// <returns>Simplified version of the expression</returns>
    private static List<Component> SimplifyMultiplication(List<Component> left_components,
        List<Component> right_components)
    {
        var components = new List<Component>();

        foreach (var left_component in left_components)
        {
            foreach (var right_component in right_components)
            {
                var result = left_component * right_component ?? new ComplexComponent(
                    new OperatorNode(Operators.MULTIPLY)
                        .SetOperands(Recreate(left_component), Recreate(right_component))
                );

                components.Add(result);
            }
        }

        return Simplify(components);
    }
    
    /// <summary>
    /// Simplifies the division between the specified operands
    /// </summary>
    /// <param name="left_components">Components of the left hand side</param>
    /// <param name="right_components">Components of the right hand side</param>
    /// <returns>Simplified version of the expression</returns>
    private static List<Component> SimplifyDivision(List<Component> left_components,
        List<Component> right_components)
    {
        if (left_components.Count == 1 && right_components.Count == 1)
        {
            var result = left_components.First() / right_components.First();

            if (result != null)
            {
                return new List<Component> { result };
            }
        }

        return new List<Component>
        {
            new ComplexComponent(
                new OperatorNode(Operators.DIVIDE)
                    .SetOperands(Recreate(left_components), Recreate(right_components))
            )
        };
    }
    
    /// <summary>
    /// Returns whether the definition's value (right side) only consists of numbers or parameters
    /// </summary>
    /// <param name="definition">Variable definition (assign-operator)</param>
    /// <returns>True if the definition is primitive, otherwise false</returns>
    private static bool IsPrimitiveDefinition(OperatorNode definition)
    {
        return definition.Right.Find(n =>
            !(n.Is(NodeType.NUMBER_NODE) || n.Is(NodeType.VARIABLE_NODE) && n.To<VariableNode>().Variable.IsParameter)) == null;
    }
    
    private static void AssignVariableDefinitions(Context context)
    {
        foreach (var variable in context.Variables.Values)
        {
            var reads = variable.Reads;
            var edits = variable.Edits;

            while (edits.Count > 0)
            {
                var edit = edits.First().Parent!;

                if (!edit.Is(NodeType.OPERATOR_NODE) || edit.To<OperatorNode>().Operator != Operators.ASSIGN)
                {
                    continue;
                }
                
                // Simplify the value of the edit
                var components = CollectComponents(edit.Last!);
                var simplified = Recreate(components);

                // Try to get the next edit of the variable
                var next_edit = edits.Count > 1 ? edits[1] : null;
                
                // Find all usages of the new value before the next edit
                var usages = reads.TakeWhile(r => next_edit == null || r.IsBefore(next_edit)).ToArray();
                
                // Replace the usages with the simplified value
                usages.ForEach(u => u.Replace(simplified));
                
                // Remove the replaced usages
                usages.ForEach(u => reads.Remove(u));
                edits.RemoveAt(0);

                // Since this edit is assigned to its usages, it can be removed
                edit.Remove();
            }
        }
    }
    
    private static List<OperatorNode> FindTopLevelOperators(Node node)
    {
        var operators = new List<OperatorNode>();
        var child = node.First;

        while (child != null)
        {
            if (child.Is(NodeType.OPERATOR_NODE))
            {
                var operation = child.To<OperatorNode>();
                
                if (operation.Operator.Type == OperatorType.ACTION ||
                    operation.Operator.Type == OperatorType.INDEPENDENT ||
                    operation.Operator.Type == OperatorType.LOGIC)
                {
                    operators.AddRange(FindTopLevelOperators(operation.Left));
                    operators.AddRange(FindTopLevelOperators(operation.Right));
                }
                else
                {
                    operators.Add(operation);
                }
            }
            else
            {
                operators.AddRange(FindTopLevelOperators(child));
            }
            
            child = child.Next;
        }

        return operators;
    }
    
    /// <summary>
    /// Analyzes the specified node tree
    /// </summary>
    /// <param name="node">Node tree to analyze</param>
    private static void Analyze(Context context, Node node)
    {
        /*AssignVariableDefinitions(context);

        var expressions = new List<Node>();

        foreach (var operation in FindTopLevelOperators(node))
        {
            if (operation.Operator.Type == OperatorType.ACTION)
            {
                expressions.Add(operation.Last!);
            }
        }
        
        foreach (var expression in expressions)
        {
            var components = CollectComponents(expression);
            var simplified = Recreate(components);
            
            // Replace the value with the simplified version
            expression.Replace(simplified);
        }*/

        //var assigns = node.FindAll(n => n.Is(NodeType.OPERATOR_NODE) && 
        //                                Equals(n.To<OperatorNode>().Operator, Operators.ASSIGN) || n.Is(NodeType.RETURN_NODE));
        var expressions = FindTopLevelOperators(node);

        foreach (var assign in expressions)
        {
            var components = CollectComponents(assign);
            var simplified = Recreate(components);
            
            // Replace the value with the simplified version
            assign.Replace(simplified);
        }
    }

    public static void Analyze(Context context)
    {
        foreach (var type in context.Types.Values)
        {
            Analyze(type);
        }
        
        foreach (var implementation in context.GetImplementedFunctions())
        {
            Analyze(implementation, implementation.Node!);
        }
    }
}