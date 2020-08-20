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

    public virtual Component Clone()
    {
        return (Component)MemberwiseClone();
    }
}

public class NumberComponent : Component
{
    public object Value { get; private set; }

    public NumberComponent(object value)
    {
        Value = value;

        if (!(Value is long || Value is double))
        {
            throw new ArgumentException("Invalid value passed for number component");
        }
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
            ComplexVariableProduct product => product * this,
            _ => null
        };
    }
    
    public override Component? Divide(Component other)
    {
        return other switch
        {
            NumberComponent number_component => new NumberComponent(Numbers.Divide(Value, number_component.Value)),
            VariableComponent variable_component => new VariableComponent(variable_component.Variable, 
                Numbers.Divide(Value, variable_component.Multiplier), -variable_component.Order),
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
    public object Multiplier { get; set;  }
    public int Order { get; set; }
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
        if (other is VariableComponent x && Equals(Variable, x.Variable) && Equals(Order, x.Order))
        {
            var multiplier = Numbers.Add(Multiplier, x.Multiplier);

            if (Numbers.IsZero(multiplier))
            {
                return new NumberComponent(0L);
            }

            return new VariableComponent(Variable, multiplier);
        }

        return null;
    }
    
    public override Component? Subtract(Component other)
    {
        if (other is VariableComponent x && Equals(Variable, x.Variable) && Equals(Order, x.Order))
        {
            var multiplier = Numbers.Subtract(Multiplier, x.Multiplier);

            if (Numbers.IsZero(multiplier))
            {
                return new NumberComponent(0L);
            }

            return new VariableComponent(Variable, multiplier);
        }

        return null;
    }
    
    public override Component? Multiply(Component other)
    {
        if (other is VariableComponent variable_component)
        {
            if (Equals(Variable, variable_component.Variable))
            {
                return new VariableComponent(
                    Variable, 
                    Numbers.Multiply(Multiplier, variable_component.Multiplier), 
                    Order + variable_component.Order
                );
            }

            var coefficient = Numbers.Multiply(Multiplier, variable_component.Multiplier);
            Multiplier = 1L;
            variable_component.Multiplier = 1L;

            return new ComplexVariableProduct(coefficient, new List<VariableComponent> { this, variable_component });
        }

        if (other is NumberComponent number_component)
        {
            return new VariableComponent(Variable, Numbers.Multiply(Multiplier, number_component.Value), Order);
        }

        if (other is ComplexVariableProduct product)
        {
            return product * this;
        }

        return null;
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

        if (other is NumberComponent number)
        {
            // If neither one of the two multipliers is a decimal number, the dividend must be divisible by the divisor
            if (Multiplier is long && number.Value is long && !Numbers.IsZero(Numbers.Remainder(Multiplier, number.Value)))
            {
                return null;
            }

            return new VariableComponent(Variable, Numbers.Divide(Multiplier, number.Value), Order);
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

public class ComplexVariableProduct : Component
{
    public object Coefficient { get; set; } = 0L;
    public List<VariableComponent> Variables { get; private set; } = new List<VariableComponent>();

    public ComplexVariableProduct(object coefficient, List<VariableComponent> variables)
    {
        Coefficient = coefficient;
        Variables = variables;
    }

    public override void Negate()
    {
        Coefficient = Numbers.Negate(Coefficient);
    }

    private bool Equals(Component other)
    {
        if (!(other is ComplexVariableProduct product) || Variables.Count != product.Variables.Count)
        {
            return false;
        }

        foreach (var x in Variables)
        {
            if (!product.Variables.Exists(v => v.Variable == x.Variable && v.Order == x.Order))
            {
                return false;
            }
        }

        return true;
    }

    public override Component? Add(Component other)
    {
        if (!Equals(other))
        {
            return null;
        }

        var clone = (ComplexVariableProduct)Clone();
        clone.Coefficient = Numbers.Add(Coefficient, ((ComplexVariableProduct)other).Coefficient);

        return clone;
    }

    public override Component? Subtract(Component other)
    {
        if (!Equals(other))
        {
            return null;
        }

        var clone = (ComplexVariableProduct)Clone();
        clone.Coefficient = Numbers.Subtract(Coefficient, ((ComplexVariableProduct)other).Coefficient);

        return clone;
    }

    public override Component? Multiply(Component other)
    {
        switch (other)
        {
            case NumberComponent number:
            {
                var coefficient = Numbers.Multiply(Coefficient, number.Value);

                if (Numbers.IsZero(coefficient))
                {
                    return new NumberComponent(0L);
                }

                var clone = (ComplexVariableProduct)Clone();
                clone.Coefficient = coefficient;

                return clone;
            }

            case VariableComponent x:
            {
                var coefficient = Numbers.Multiply(Coefficient, x.Multiplier);

                var clone = (ComplexVariableProduct)Clone();
                clone.Coefficient = coefficient;

                var a = clone.Variables.Find(v => v.Variable == x.Variable);

                if (a != null)
                {
                    a.Order += x.Order;

                    if (a.Order == 0)
                    {
                        Variables.Remove(a);
                    }
                }
                else
                {
                    x = (VariableComponent)x.Clone();
                    x.Multiplier = 1L;

                    clone.Variables.Add(x);
                }

                return clone;
            }

            case ComplexVariableProduct product:
            {
                var coefficient = Numbers.Multiply(Coefficient, product.Coefficient);

                var clone = (ComplexVariableProduct)Clone();
                clone.Coefficient = coefficient;

                foreach (var x in product.Variables)
                {
                    clone = (ComplexVariableProduct)clone.Multiply(x)!;
                }
                
                return this;
            }

            default: return null;
        }
    }

    public override Component? Divide(Component other)
    {
        if (other is NumberComponent number)
        {
            if (Coefficient is long && number.Value is long && !Numbers.IsZero(Numbers.Remainder(Coefficient, number.Value)))
            {
                return null;
            }

            var coefficient = Numbers.Divide(Coefficient, number.Value);

            var clone = (ComplexVariableProduct)Clone();
            clone.Coefficient = coefficient;

            return clone;
        }

        return null;
    }

    public override Component Clone()
    {
        var clone = (ComplexVariableProduct)MemberwiseClone();
        clone.Variables = Variables.Select(v => (VariableComponent)v.Clone()).ToList();

        return clone;
    }
}

public class ComplexComponent : Component
{
    public Node Node { get; private set; }
    public bool IsNegative { get; private set; }

    public override void Negate()
    {
        IsNegative = !IsNegative;
    }

    public ComplexComponent(Node node)
    {
        Node = node;
    }

    public override Component Clone()
    {
        var clone = (ComplexComponent)MemberwiseClone();
        clone.Node = Node.Clone();

        return clone;
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
                if (Numbers.IsZero(number_component.Value))
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
                if (Numbers.IsZero(variable_component.Multiplier))
                {
                    continue;
                }

                var node = GetOrderedVariable(variable_component.Variable, variable_component.Order);
                bool is_multiplier_negative;

                // When the multiplier is exactly one (double), the multiplier can be ignored, meaning the inaccuracy of the comparison is expected
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (variable_component.Multiplier is double b && Math.Abs(b) != 1.0)
                {
                    is_multiplier_negative = b < 0.0;

                    if (Math.Abs(b) != 1.0)
                    {
                        node = new OperatorNode(Operators.MULTIPLY).SetOperands(
                            node, 
                            new NumberNode(Format.DECIMAL, Math.Abs(b))
                        );
                    }
                }
                else 
                {
                    is_multiplier_negative = (long)variable_component.Multiplier < 0;

                    if (Math.Abs((long)variable_component.Multiplier) != 1L)
                    {
                        node = new OperatorNode(Operators.MULTIPLY).SetOperands(
                            node,
                            new NumberNode(Assembler.Format, Math.Abs((long)variable_component.Multiplier))
                        );
                    }   
                }
            
                result = new OperatorNode(is_multiplier_negative ? Operators.SUBTRACT : Operators.ADD).SetOperands(result, node);
            }

            if (component is ComplexComponent complex_component)
            {
                result = new OperatorNode(complex_component.IsNegative ? Operators.SUBTRACT : Operators.ADD).SetOperands(
                    result,
                    complex_component.Node
                );
            }

            if (component is ComplexVariableProduct product)
            {
                var is_negative = product.Coefficient is long a && a < 0L || product.Coefficient is double b && b < 0.0;

                if (is_negative)
                {
                    product.Negate();
                }

                var other = Recreate(product);

                result = is_negative 
                    ? new OperatorNode(Operators.SUBTRACT).SetOperands(result, other) 
                    : new OperatorNode(Operators.ADD).SetOperands(result, other);
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

        for (var i = 1; i < Math.Abs(order); i++)
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
            
            return !Numbers.Equals(variable_component.Multiplier, 1L) ? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Assembler.Format, variable_component.Multiplier)) : result;
        }

        if (component is ComplexComponent complex_component)
        {
            return complex_component.IsNegative ? new NegateNode(complex_component.Node) : complex_component.Node;
        }

        if (component is ComplexVariableProduct product)
        {
            var result = GetOrderedVariable(product.Variables.First().Variable, product.Variables.First().Order);

            for (var i = 1; i < product.Variables.Count; i++)
            {
                var variable = product.Variables[i];

                result = new OperatorNode(Operators.MULTIPLY)
                    .SetOperands(result, GetOrderedVariable(variable.Variable, variable.Order));
            }

            return !Numbers.Equals(product.Coefficient, 1L) 
                ? new OperatorNode(Operators.MULTIPLY)
                    .SetOperands(result, new NumberNode(Assembler.Format, product.Coefficient)) 
                : result;
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
    /// Returns whether the specified node is primitive that is whether it contains only operators, numbers, parameter- or local variables
    /// </summary>
    /// <returns>True if the definition is primitive, otherwise false</returns>
    private static bool IsPrimitive(Node node)
    {
        return node.Find(n => !(n.Is(NodeType.NUMBER_NODE) || n.Is(NodeType.OPERATOR_NODE) || n.Is(NodeType.VARIABLE_NODE) && n.To<VariableNode>().Variable.IsPredictable)) == null;
    }

    private static List<Node> GetReferences(Node root, Variable variable)
    {
        return root.FindAll(n => n.Is(NodeType.VARIABLE_NODE))
            .Where(v => v.To<VariableNode>().Variable == variable)
            .ToList();
    }

    private static List<Node> GetEdits(List<Node> references)
    {
        return references
            .Where(v => Analyzer.IsEdited(v.To<VariableNode>())).ToList();
    }

    private static Node? GetBranch(Node node)
    {
        return node.FindParent(p => p.Is(NodeType.LOOP_NODE, NodeType.IF_NODE, NodeType.ELSE_IF_NODE, NodeType.ELSE_NODE));
    }

    public static List<Node> GetBlacklist(Node node)
    {
        var blacklist = new List<Node>();
        var branch = node;

        while ((branch = GetBranch(branch!)) != null)
        {
            if (branch is IfNode x)
            {
                blacklist.AddRange(x.GetBranches().Where(b => b != branch));
            }
            else if (branch is ElseIfNode y)
            {
                blacklist.AddRange(y.GetRoot().GetBranches().Where(b => b != branch));
            }
            else if (branch is ElseNode z)
            {
                blacklist.AddRange(z.GetRoot().GetBranches().Where(b => b != branch));
            }
        }

        return blacklist;
    }

    private static List<Edit> GetPastEdits(Node node, IEnumerable<Edit> edits)
    {
        // Get a blacklist which describes which sections of the node tree have not been executed in the past
        var blacklist = GetBlacklist(node);

        return edits.Reverse()
            .SkipWhile(e => !e.Node.IsBefore(node))
            .Where(e => !blacklist.Any(i => e.Node.IsUnder(i)))
            .ToList();
    }

    private class Edit
    {
        public Node Node { get; set; }
        public List<Node> Dependencies { get; private set; } = new List<Node>();
        public bool Required => Dependencies.Any();

        public Edit(Node node)
        {
            Node = node;
        }

        public void AddDependency(Node dependency)
        {
            if (!Dependencies.Contains(dependency))
            {
                Dependencies.Add(dependency);
            }
        }

        public void RemoveDependency(Node dependency)
        {
            if (!Dependencies.Remove(dependency))
            {
                throw new ApplicationException("Tried to remove edit depedency but it was not registered");
            }
        }

        public Node GetRoot()
        {
            return Node.FindParent(p => p.Is(NodeType.INCREMENT_NODE, NodeType.DECREMENT_NODE) || 
                p.Is(NodeType.OPERATOR_NODE) && p.To<OperatorNode>().Operator.Type == OperatorType.ACTION)
                ?? throw new ApplicationException("Could not find the root of a edit");
        }
    }

    /// <summary>
    /// Toggles all edits which are encountered in the specified node tree and returns whether the edits are bypassable
    /// </summary>
    /// <returns>
    /// Returns whether the specified node tree is executable without encountering the specified edits
    /// </returns>
    private static bool Toggle(Node node, Node dependency, List<Edit> edits)
    {
        var edit = edits.Find(e => e.Node == node);

        if (edit != null)
        {
            edit.AddDependency(dependency);

            if (edit.Node.Is(Operators.ASSIGN))
            {
                return false;
            }
        }

        // If the specified node tree doesn't contain any of the edits, it must be penetrable
        if (node.Find(n => edits.Any(e => e.Node == n)) == null)
        {
            return true;
        }

        if (node.Is(NodeType.IF_NODE))
        {
            var branches = node.To<IfNode>().GetBranches();
            edits.Where(e => branches.Any(b => e.Node.IsUnder(b))).ForEach(e => e.AddDependency(dependency));
        }
        else if (node.Is(NodeType.LOOP_NODE))
        {
            if (node.To<LoopNode>().IsForeverLoop && !Toggle(node.To<LoopNode>().Body, dependency, edits))
            {
                return false;
            }
            else
            {
                // Register all the edits inside the loop
                edits.Where(e => e.Node.IsUnder(node)).ForEach(e => e.AddDependency(dependency));

                // If the initialization contains an edit, it means it's not bypassable
                if (!Toggle(node.To<LoopNode>().Initialization, dependency, edits))
                {
                    return false;
                }
            }
        }
        else if (!node.Is(NodeType.ELSE_IF_NODE, NodeType.ELSE_NODE))
        {
            var iterator = node.Last;

            while (iterator != null)
            {
                if (!Toggle(iterator, dependency, edits))
                {
                    return false;
                }

                iterator = iterator.Previous;
            }
        }

        return true;
    }

    private static Edit? ToggleRelevantEdits(Node node, Node dependency, List<Edit> edits)
    {
        var iterator = (Node?)node;
        var past = GetPastEdits(node, edits);

        while (iterator != null)
        {
            if (!Toggle(iterator, dependency, past))
            {
                return past.FirstOrDefault();
            }

            if (iterator.Previous == null)
            {
                if (iterator.Parent == null)
                {
                    return past.FirstOrDefault();
                }

                iterator = iterator.Parent!;

                switch (iterator.GetNodeType())
                {
                    case NodeType.LOOP_NODE:
                    {
                        //Toggle(iterator, dependency, edits);

                        // All edits which are inside the current loop are needed by the specified node
                        edits.Where(e => e.Node.IsUnder(iterator)).ForEach(e => e.AddDependency(dependency));

                        iterator = iterator.Previous;
                        break;
                    }

                    case NodeType.IF_NODE:
                    {
                        iterator = iterator.Previous;
                        break;
                    }

                    case NodeType.ELSE_IF_NODE:
                    {
                        iterator = iterator.To<ElseIfNode>().GetRoot().Previous;
                        break;
                    }

                    case NodeType.ELSE_NODE:
                    {
                        iterator = iterator.To<ElseNode>().GetRoot().Previous;
                        break;
                    }

                    case NodeType.IMPLEMENTATION_NODE:
                    {
                        return past.FirstOrDefault();
                    }

                    default: break;
                }
            }
            else
            {
                iterator = iterator.Previous;
            }
        }

        return past.FirstOrDefault();
    }

    private static bool IsBranched(Node read, Node edit)
    {
        var x = read.FindParent(p => p is IContext && !(p.Is(NodeType.LOOP_NODE) && p.To<LoopNode>().IsForeverLoop)) ?? throw new ApplicationException("Analysis executed outside of a context");
        var y = edit.FindParent(p => p is IContext && !(p.Is(NodeType.LOOP_NODE) && p.To<LoopNode>().IsForeverLoop)) ?? throw new ApplicationException("Analysis executed outside of a context");

        return x != y && !x.IsUnder(y);
    }

    private static bool IsAssignable(Node read, Edit edit, List<Edit> edits)
    {
        var root = edit.GetRoot();

        if (!root!.Is(NodeType.OPERATOR_NODE) || root!.To<OperatorNode>().Operator != Operators.ASSIGN)
        {
            /// TODO: Assignment value is possible to calculate here sometimes
            return false;
        }

        if (!IsPrimitive(root) || IsBranched(read, edit.Node))
        {
            return false;
        }

        var loop = read.FindParent(p => p.Is(NodeType.LOOP_NODE));

        if (loop == null)
        {
            return true;
        }

        if (!loop.To<LoopNode>().IsForeverLoop && read.IsUnder(loop.To<LoopNode>().Condition))
        {
            return false;
        }

        return !(!edit.Node.IsUnder(loop) && edits.Where(e => e != edit).Any(e => e.Node.IsUnder(loop)));
    }

    private static Node AssignVariable(Node node, Variable variable)
    {
        var root = node.Clone();

        var reads = GetReferences(root, variable);
        var edits = GetEdits(reads).Select(e => new Edit(e)).ToList();

        reads.RemoveAll(r => edits.Any(e => e.Node == r));

        foreach (var read in reads)
        {
            // Register all of the past edits which are needed by the current reference
            // Retrieve the latest edit which concerns the current reference
            var edit = ToggleRelevantEdits(read, read, edits);

            if (edit != null && IsAssignable(read, edit, edits))
            {
                var assignment = edit.GetRoot();

                edit.RemoveDependency(read);

                var components = CollectComponents(assignment.Last!);
                var simplified = Recreate(components);

                read.Replace(simplified);
            }
        }

        // Remove all edits which are not needed or have been assigned completely
        edits.Where(e => !e.Required).ForEach(e => e.GetRoot().Remove());

        return root;
    }

    private static bool OptimizeComparisons(Node root)
    {
        var comparisons = root.FindAll(n => n.Is(NodeType.OPERATOR_NODE) && n.To<OperatorNode>().Operator.Type == OperatorType.COMPARISON);
        var precomputed = false;

        foreach (var comparison in comparisons)
        {
            var left = CollectComponents(comparison.First!);
            var right = CollectComponents(comparison.Last!);

            var i = 0;
            var j = 0;

            while (left.Any() && right.Any() && i < left.Count)
            {
                if (j >= right.Count)
                {
                    i++;
                    j = 0;
                    continue;
                }

                var x = left[i];
                var y = right[j];

                if (x is ComplexComponent)
                {
                    i++;
                    j = 0;
                    continue;
                }
                else if (y is ComplexComponent)
                {
                    j++;
                    continue;
                }

                var s = x - y;

                if (s != null)
                {
                    left.RemoveAt(i);
                    right.RemoveAt(j);

                    left.Insert(i, s);

                    j = 0;
                }
                else
                {
                    j++;
                }
            }

            if (!left.Any())
            {
                left.Add(new NumberComponent(0L));
            }

            if (!right.Any())
            {
                right.Add(new NumberComponent(0L));
            }

            left = Simplify(left);
            right = Simplify(right);

            comparison.First!.Replace(Recreate(left));
            comparison.Last!.Replace(Recreate(right));
        
            var evaluation = Preprocessor.TryEvaluateOperator(comparison.To<OperatorNode>());

            if (evaluation != null)
            {
                comparison.Replace(new NumberNode(Parser.Size.ToFormat(), (bool)evaluation ? 1L : 0L));
                precomputed = true;
            }
        }

        return precomputed;
    }

    private static bool UnwrapStatements(Node root)
    {
        var iterator = root.First;
        var unwrapped = false;

        while (iterator != null)
        {
            if (iterator.Is(NodeType.IF_NODE))
            {
                var statement = iterator.To<IfNode>();

                if (statement.Condition.Is(NodeType.NUMBER_NODE))
                {
                    var successors = statement.GetSuccessors();

                    if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
                    {
                        // Disconnect all the successors
                        successors.ForEach(s => s.Remove());

                        iterator = statement.Next;

                        // Replace the conditional statement with the body
                        statement.ReplaceWithChildren(statement.Body);

                        unwrapped = true;
                    }
                    else
                    {
                        if (statement.Successor == null)
                        {
                            iterator = statement.Next;
                            statement.Remove();
                            continue;
                        }

                        if (statement.Successor.Is(NodeType.ELSE_NODE))
                        {
                            iterator = statement.Successor.Next;

                            // Replace the conditional statement with the body of the successor
                            statement.ReplaceWithChildren(statement.Successor.To<ElseNode>().Body);

                            unwrapped = true;
                            continue;
                        }

                        var successor = statement.Successor.To<ElseIfNode>();

                        var replacement = new IfNode(successor.Context, successor.Condition, successor.Body);
                        iterator = replacement;

                        successor.Remove();

                        statement.Replace(replacement);

                        unwrapped = true;
                    }

                    continue;
                }
            }
            else if (iterator.Is(NodeType.LOOP_NODE))
            {
                var statement = iterator.To<LoopNode>();

                if (!statement.IsForeverLoop)
                {
                    if (!statement.Condition.Is(NodeType.NUMBER_NODE))
                    {
                        iterator = statement.Next;

                        if (TryUnwrapLoop(statement))
                        {
                            unwrapped = true;
                        }

                        continue;
                    }

                    if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
                    {
                        statement.Parent!.Insert(statement, statement.Initialization);

                        var replacement = new LoopNode(statement.StepsContext, statement.BodyContext, null, statement.Body);
                        statement.Parent!.Insert(statement, replacement);

                        iterator = replacement.Next;
                    }
                    else
                    {
                        statement.Replace(statement.Initialization);
                        iterator = statement.Initialization;
                    }

                    unwrapped = true;
                    continue;
                }
            }

            iterator = iterator.Next;
        }

        return unwrapped;
    }

    private class LoopUnwrapDescriptor
    {
        public Variable Iterator { get; set; }
        public long Steps { get; set; }
        public List<Component> Start { get; set; }
        public List<Component> Step { get; set; }

        public LoopUnwrapDescriptor(Variable iterator, long steps, List<Component> start, List<Component> step)
        {
            Iterator = iterator;
            Steps = steps;
            Start = start;
            Step = step;
        }
    }

    private static LoopUnwrapDescriptor? TryGetLoopUnwrapDescriptor(LoopNode loop)
    {
        // First, ensure that the condition contains a comparison operator and that it's primitive.
        // Examples:
        // i < 10
        // i == 0
        // 0 < 10 * a + 10 - x
        var condition = loop.Condition;
        
        if (condition.Is(NodeType.NORMAL) || 
            !condition.Is(NodeType.OPERATOR_NODE) || 
            condition.To<OperatorNode>().Operator.Type != OperatorType.COMPARISON ||
            !IsPrimitive(condition))
        {
            return null;
        }

        // Ensure that the initialization is empty or it contains a definition of an integer variable
        var initialization = loop.Initialization;

        if (initialization.Is(NodeType.NORMAL))
        {
            Console.WriteLine("Analysis encountered an empty loop initialization which canceled the attempt of unwrapping the loop");
            return null;
        }
        
        if (!initialization.Is(Operators.ASSIGN) || !initialization.First!.Is(NodeType.VARIABLE_NODE))
        {
            return null;
        }

        // Make sure the variable is predictable and it's an integer
        var variable = initialization.First!.To<VariableNode>().Variable;

        if (!variable.IsPredictable || 
            !(initialization.First.To<VariableNode>().Variable.Type is Number) ||
            !initialization.Last!.Is(NodeType.NUMBER_NODE))
        {
            return null;
        }

        var start_value = initialization.Last.To<NumberNode>().Value;

        var action = loop.Action;

        if (action.Is(NodeType.NORMAL))
        {
            return null;
        }

        var step_value = new List<Component>();

        if (action.Is(NodeType.INCREMENT_NODE))
        {
            var statement = action.To<IncrementNode>();

            if (!statement.Object.Is(variable))
            {
                return null;
            }

            step_value.Add(new NumberComponent(1L));
        }
        else if (action.Is(NodeType.DECREMENT_NODE))
        {
            var statement = action.To<IncrementNode>();

            if (!statement.Object.Is(variable))
            {
                return null;
            }

            step_value.Add(new NumberComponent(-1L));
        }
        else if (action.Is(NodeType.OPERATOR_NODE))
        {
            var statement = action.To<OperatorNode>();
            
            if (!statement.Left.Is(variable))
            {
                return null;
            }

            if (statement.Operator == Operators.ASSIGN_ADD)
            {
                step_value = CollectComponents(statement.Right);
            }
            else if (statement.Operator == Operators.ASSIGN_SUBTRACT)
            {
                step_value = Negate(CollectComponents(statement.Right));
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
        
        // Try to rewrite the condition so that the initialized variable is on the left side of the comparison
        // Example:
        // 0 < 10 * a + 10 - x => x < 10 * a + 10
        var left = CollectComponents(condition.First!);

        // Abort the optimization if the comparison contains complex variable components
        // Examples (x is the iterator variable):
        // x^2 < 10
        // x < ax + 10
        if (left.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
            c is ComplexVariableProduct y && y.Variables.Exists(i => i.Variable == variable)))
        {
            return null;
        }

        var right = CollectComponents(condition.Last!);

        if (right.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
            c is ComplexVariableProduct y && y.Variables.Exists(i => i.Variable == variable)))
        {
            return null;
        }

        // Ensure that the condition contains atleast one initialization variable
        if (!left.Concat(right).Any(c => c is VariableComponent x && x.Variable == variable))
        {
            return null;
        }

        // Move all other than initialization variables to the right hand side
        for (var i = left.Count - 1; i >= 0; i--)
        {
            var x = left[i];

            if (x is VariableComponent a && a.Variable == variable)
            {
                continue;
            }

            x.Negate();

            right.Add(x);
            left.RemoveAt(i);
        }

        // Move all initialization variables to the left hand side
        for (var i = right.Count - 1; i >= 0; i--)
        {
            var x = right[i];

            if (!(x is VariableComponent a) || a.Variable != variable)
            {
                continue;
            }

            x.Negate();
            
            left.Add(x);
            right.RemoveAt(i);
        }
        
        // Substract the starting value from the right hand side of the condition
        var range = SimplifySubtraction(right, new List<Component> { new NumberComponent(start_value) });
        var result = SimplifyDivision(range, step_value);

        if (result != null)
        {
            if (result.Count != 1)
            {
                return null;
            }

            if (result.First() is NumberComponent steps)
            {
                if (steps.Value is double)
                {
                    Console.WriteLine("Loop can not be unwrapped since the amount of steps is expressed in decimals?");
                    return null;
                }

                return new LoopUnwrapDescriptor(variable, (long)steps.Value, new List<Component> { new NumberComponent(start_value) }, step_value);
            }

            // If the amount of steps is not a constant, it means the length of the loop varies, therefore the loop can not be unwrapped
            return null;
        }

        Console.WriteLine("Encountered possible complex loop increment value division, please implement");
        return null;
    }

    public static bool TryUnwrapLoop(LoopNode loop)
    {
        var descriptor = TryGetLoopUnwrapDescriptor(loop);

        if (descriptor == null)
        {
            return false;
        }

        loop.Insert(loop.Initialization);

        for (var i = 0; i < descriptor.Steps; i++)
        {
            loop.InsertChildren(loop.Body.Clone());
            loop.Insert(loop.Action.Clone());
        }

        loop.Remove();
        return true;
    }

    private static bool RemoveUnreachableStatements(Node root)
    {
        var return_statements = root.FindAll(n => n.Is(NodeType.RETURN_NODE));
        var removed = false;

        for (var i = return_statements.Count - 1; i >= 0; i--)
        {
            var return_statement = return_statements[i];

            // Remove all statements which are after the return statement in its scope
            var iterator = return_statement.Parent!.Last;

            while (iterator != return_statement)
            {
                var previous = iterator!.Previous;
                iterator.Remove();
                iterator = previous;
                removed = true;
            }
        }
        
        return removed;
    }

    private static long GetCost(Node node)
    {
        var result = 0L;
        var iterator = node.First;

        while (iterator != null)
        {
            if (iterator.Is(NodeType.OPERATOR_NODE))
            {
                var operation = iterator.To<OperatorNode>().Operator;

                if (operation == Operators.ADD || operation == Operators.SUBTRACT)
                {
                    result++;
                }
                else if (operation == Operators.MULTIPLY)
                {
                    result += 5;
                }
                else if (operation == Operators.DIVIDE)
                {
                    result += 70;
                }
                else if (operation.Type == OperatorType.COMPARISON)
                {
                    result++;
                }
            }

            result += GetCost(iterator);

            iterator = iterator.Next;
        }

        return result;
    }

    /// <summary>
    /// Tries to optimize the specified node tree which is described by the specified context
    /// </summary>
    private static Node Optimize(Node node, FunctionImplementation context)
    {
        var minimum_cost_snapshot = node;
        var minimum_cost = GetCost(node);

        var snapshot = node;

        Start:

        foreach (var variable in context.Locals.Concat(context.Parameters))
        {
            // Assign the definitions of the current variable
            snapshot = AssignVariable(snapshot, variable);

            // Try to optimize all comparisons found in the current snapshot
            if (OptimizeComparisons(snapshot))
            {
                goto Start;
            }

            // Try to unwrap conditional statements whose outcome have been resolved
            if (UnwrapStatements(snapshot))
            {
                goto Start;
            }

            // Removes all statements which are not reachable
            if (RemoveUnreachableStatements(snapshot))
            {
                goto Start;
            }

            // Now, since the variable is assigned, try to simplify the code
            var expressions = new List<Node>();

            foreach (var operation in FindTopLevelOperators(snapshot))
            {
                if (operation.Operator.Type == OperatorType.ACTION)
                {
                    expressions.Add(operation.Last!);
                }
                else
                {
                    expressions.Add(operation);
                }
            }

            foreach (var expression in expressions)
            {
                var components = CollectComponents(expression);
                var simplified = Recreate(components);
            
                // Replace the value with the simplified version
                expression.Replace(simplified);
            }

            // Calculate the complexity of the current snapshot
            var cost = GetCost(snapshot);

            if (cost < minimum_cost)
            {
                // Since the current snapshot is less complex it should be used
                minimum_cost_snapshot = snapshot;
                minimum_cost = cost;
            }
        }

        return minimum_cost_snapshot;
    }
    
    /// <summary>
    /// Returns all operator nodes which are first encounter when decending from the specified node
    /// </summary>
    private static List<OperatorNode> FindTopLevelOperators(Node node)
    {
        if (node.Is(NodeType.OPERATOR_NODE) && node.To<OperatorNode>().Operator.Type == OperatorType.CLASSIC)
        {
            return new List<OperatorNode> { (OperatorNode)node };
        }

        var operators = new List<OperatorNode>();
        var child = node.First;

        while (child != null)
        {
            if (child.Is(NodeType.OPERATOR_NODE))
            {
                var operation = child.To<OperatorNode>();
                
                if (operation.Operator.Type != OperatorType.CLASSIC)
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
    
    public static void Analyze(Context context)
    {
        foreach (var type in context.Types.Values)
        {
            Analyze(type);
        }
        
        foreach (var implementation in context.GetImplementedFunctions())
        {
            implementation.Node = Optimize(implementation.Node!, implementation);
        }
    }
}