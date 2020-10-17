using System;
using System.Collections.Generic;
using System.Linq;

public class VariableReferenceDescriptor
{
	public List<Node> Reads { get; }
	public List<Node> Writes { get; }

	public VariableReferenceDescriptor(List<Node> reads, List<Node> writes)
	{
		Writes = writes;
		Reads = reads;
	}
}

public class VariableEqualityComparer : EqualityComparer<Variable>
{
	public override bool Equals(Variable? a, Variable? b)
	{
		return a == b;
	}

	public override int GetHashCode(Variable? a) => 0;
}

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
		if (Value is long coefficient)
		{
			Value = -coefficient;
		}
		else
		{
			Value = -(double)Value;
		}
	}

	public override Component? Add(Component other)
	{
		if (Numbers.IsZero(this))
		{
			return other.Clone();
		}
		else if (Numbers.IsZero(other))
		{
			return Clone();
		}

		if (other is NumberComponent number_component)
		{
			return new NumberComponent(Numbers.Add(Value, number_component.Value));
		}

		return null;
	}

	public override Component? Subtract(Component other)
	{
		if (Numbers.IsZero(other))
		{
			return Clone();
		}
		else if (Numbers.IsZero(Value))
		{
			var clone = other.Clone();
			clone.Negate();

			return clone;
		}

		if (other is NumberComponent number_component)
		{
			return new NumberComponent(Numbers.Subtract(Value, number_component.Value));
		}

		return null;
	}

	public override Component? Multiply(Component other)
	{
		if (Numbers.IsZero(this) || Numbers.IsZero(other))
		{
			return new NumberComponent(0L);
		}
		else if (Numbers.IsOne(this))
		{
			return other.Clone();
		}
		else if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return other switch
		{
			NumberComponent number_component => new NumberComponent(Numbers.Multiply(Value, number_component.Value)),
			VariableComponent variable_component => new VariableComponent(variable_component.Variable,
				Numbers.Multiply(Value, variable_component.Coefficient)),
			ComplexVariableProduct product => product * this,
			_ => null
		};
	}

	public override Component? Divide(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return other switch
		{
			NumberComponent number_component => new NumberComponent(Numbers.Divide(Value, number_component.Value)),
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
	public object Coefficient { get; set; }
	public int Order { get; set; }
	public Variable Variable { get; }

	public VariableComponent(Variable variable, object? coefficient = null, int order = 1)
	{
		Coefficient = coefficient ?? 1L;
		Variable = variable;
		Order = order;
	}

	public override void Negate()
	{
		if (Coefficient is long coefficient)
		{
			Coefficient = -coefficient;
		}
		else
		{
			Coefficient = -(double)Coefficient;
		}
	}

	public override Component? Add(Component other)
	{
		if (Numbers.IsZero(other))
		{
			return Clone();
		}

		if (other is VariableComponent x && Equals(Variable, x.Variable) && Equals(Order, x.Order))
		{
			var coefficient = Numbers.Add(Coefficient, x.Coefficient);

			if (Numbers.IsZero(coefficient))
			{
				return new NumberComponent(0L);
			}

			return new VariableComponent(Variable, coefficient);
		}

		return null;
	}

	public override Component? Subtract(Component other)
	{
		if (Numbers.IsZero(other))
		{
			return Clone();
		}

		if (other is VariableComponent x && Equals(Variable, x.Variable) && Equals(Order, x.Order))
		{
			var coefficient = Numbers.Subtract(Coefficient, x.Coefficient);

			if (Numbers.IsZero(coefficient))
			{
				return new NumberComponent(0L);
			}

			return new VariableComponent(Variable, coefficient);
		}

		return null;
	}

	public override Component? Multiply(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}
		else if (Numbers.IsZero(other))
		{
			return new NumberComponent(0L);
		}

		if (other is VariableComponent variable_component)
		{
			if (Equals(Variable, variable_component.Variable))
			{
				return new VariableComponent(
					Variable,
					Numbers.Multiply(Coefficient, variable_component.Coefficient),
					Order + variable_component.Order
				);
			}

			var coefficient = Numbers.Multiply(Coefficient, variable_component.Coefficient);
			Coefficient = 1L;
			variable_component.Coefficient = 1L;

			return new ComplexVariableProduct(coefficient, new List<VariableComponent> { this, variable_component });
		}

		if (other is NumberComponent number_component)
		{
			return new VariableComponent(Variable, Numbers.Multiply(Coefficient, number_component.Value), Order);
		}

		if (other is ComplexVariableProduct product)
		{
			return product * this;
		}

		return null;
	}

	public override Component? Divide(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		if (other is VariableComponent variable_component && Equals(Variable, variable_component.Variable))
		{
			var order = Order - variable_component.Order;
			var coefficient = Numbers.Divide(Coefficient, variable_component.Coefficient);

			if (order == 0)
			{
				return new NumberComponent(coefficient);
			}

			// Ensure that the coefficient supports fractions
			if (coefficient is double || Numbers.IsZero(Numbers.Remainder(Coefficient, variable_component.Coefficient)))
			{
				return new VariableComponent(Variable, coefficient, order);
			}
		}

		if (other is NumberComponent number)
		{
			// If neither one of the two coefficients is a decimal number, the dividend must be divisible by the divisor
			if (Coefficient is long && number.Value is long && !Numbers.IsZero(Numbers.Remainder(Coefficient, number.Value)))
			{
				return null;
			}

			return new VariableComponent(Variable, Numbers.Divide(Coefficient, number.Value), Order);
		}

		return null;
	}

	public override bool Equals(object? other)
	{
		return other is VariableComponent component && Equals(component.Coefficient, Coefficient) && Equals(component.Variable, Variable);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Coefficient, Variable);
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
		if (Numbers.IsZero(other))
		{
			return Clone();
		}

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
		if (Numbers.IsZero(other))
		{
			return Clone();
		}

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
		if (Numbers.IsOne(other))
		{
			return Clone();
		}
		else if (Numbers.IsZero(other))
		{
			return new NumberComponent(0L);
		}

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
				var coefficient = Numbers.Multiply(Coefficient, x.Coefficient);

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
					x.Coefficient = 1L;

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
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

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

	public override Component? Add(Component other)
	{
		return Numbers.IsZero(other) ? Clone() : null;
	}

	public override Component? Subtract(Component other)
	{
		return Numbers.IsZero(other) ? Clone() : null;
	}

	public override Component? Multiply(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return Numbers.IsZero(other) ? new NumberComponent(0L) : null;
	}

	public override Component? Divide(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return null;
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
	public static bool IsMathematicalOptimizationEnabled { get; set; }  = true;

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

				result = is_negative
					? new OperatorNode(Operators.SUBTRACT).SetOperands(result, number.Negate())
					: new OperatorNode(Operators.ADD).SetOperands(result, number);
			}

			if (component is VariableComponent variable_component)
			{
				// When the coefficient is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
				if (Numbers.IsZero(variable_component.Coefficient))
				{
					continue;
				}

				var node = GetOrderedVariable(variable_component.Variable, variable_component.Order);
				bool is_coefficient_negative;

				// When the coefficient is exactly one (double), the coefficient can be ignored, meaning the inaccuracy of the comparison is expected
				if (variable_component.Coefficient is double b)
				{
					is_coefficient_negative = b < 0.0;

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
					is_coefficient_negative = (long)variable_component.Coefficient < 0;

					if (Math.Abs((long)variable_component.Coefficient) != 1L)
					{
						node = new OperatorNode(Operators.MULTIPLY).SetOperands(
							node,
							new NumberNode(Assembler.Format, Math.Abs((long)variable_component.Coefficient))
						);
					}
				}

				result = new OperatorNode(is_coefficient_negative ? Operators.SUBTRACT : Operators.ADD).SetOperands(result, node);
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
			// When the coefficient is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
			if (variable_component.Coefficient is double a && a == 0.0)
			{
				return new NumberNode(Format.DECIMAL, 0.0);
			}

			if (variable_component.Coefficient  is long b && b == 0.0)
			{
				return new NumberNode(Assembler.Format, 0L);
			}

			var result = GetOrderedVariable(variable_component.Variable, variable_component.Order);

			// When the coefficient is exactly one (double), the coefficient can be ignored, meaning the inaccuracy of the comparison is expected
			if (variable_component.Coefficient is double c)
			{
				if (c == 1.0)
				{
					return result;
				}

				return new OperatorNode(Operators.MULTIPLY)
					.SetOperands(result, new NumberNode(Format.DECIMAL, c));
			}

			return !Numbers.IsOne(variable_component.Coefficient)
				? new OperatorNode(Operators.MULTIPLY)
					.SetOperands(result, new NumberNode(Assembler.Format, variable_component.Coefficient))
				: result;
		}

		if (component is ComplexComponent complex_component)
		{
			return complex_component.IsNegative
				? new NegateNode(complex_component.Node)
				: complex_component.Node;
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

		if (node.Is(NodeType.NUMBER))
		{
			result.Add(new NumberComponent(node.To<NumberNode>().Value));
		}
		else if (node.Is(NodeType.VARIABLE))
		{
			result.Add(new VariableComponent(node.To<VariableNode>().Variable));
		}
		else if (node.Is(NodeType.OPERATOR))
		{
			result.AddRange(CollectComponents(node.To<OperatorNode>()));
		}
		else if (node.Is(NodeType.CONTENT))
		{
			if (!Equals(node.First, null))
			{
				result.AddRange(CollectComponents(node.First));
			}
		}
		else if (node.Is(NodeType.NEGATE))
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
		return node.Find(n => !(n.Is(NodeType.NUMBER) || n.Is(NodeType.OPERATOR) || n.Is(NodeType.VARIABLE) && n.To<VariableNode>().Variable.IsPredictable)) == null;
	}

	private static List<Node> GetReferences(Node root, Variable variable)
	{
		return root.FindAll(n => n.Is(NodeType.VARIABLE))
			.Where(v => v.To<VariableNode>().Variable == variable)
			.ToList();
	}

	private static List<Node> GetEdits(List<Node> references)
	{
		return references
			.Where(v => Analyzer.IsEdited(v.To<VariableNode>())).ToList();
	}

	public static VariableReferenceDescriptor GetVariableReferenceDescriptor(Node root, Variable variable)
	{
		var reads = GetReferences(root, variable);
		var writes = GetEdits(reads);

		for (var i = 0; i < writes.Count; i++)
		{
			for (var j = 0; j < reads.Count; j++)
			{
				if (reads[j] == writes[i] && !writes[i].Parent!.Is(NodeType.INCREMENT, NodeType.DECREMENT))
				{
					reads.RemoveAt(j);
					break;
				}
			}
		}

		return new VariableReferenceDescriptor(reads, writes);
	}

	private static Node? GetBranch(Node node)
	{
		return node.FindParent(p => p.Is(NodeType.LOOP, NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE));
	}

	public static List<Node> GetBlacklist(Node node)
	{
		var blacklist = new List<Node>();
		var branch = node;

		while ((branch = GetBranch(branch!)) != null)
		{
			if (branch is IfNode x)
			{
				blacklist.AddRange(x.GetBranches().Where(b => b != x));
			}
			else if (branch is ElseIfNode y)
			{
				blacklist.AddRange(y.GetRoot().GetBranches().Where(b => b != y));
			}
			else if (branch is ElseNode z)
			{
				blacklist.AddRange(z.GetRoot().GetBranches().Where(b => b != z));
			}
		}

		return blacklist;
	}

	/// <summary>
	/// Returns whether the specified variable will be used in the future starting from the specified node perspective
	/// </summary>
	public static bool IsUsedLater(Variable variable, Node perspective)
	{
		// Get a blacklist which describes which sections of the node tree have not been executed in the past or won't be executed in the future
		var blacklist = GetBlacklist(perspective);

		// If any of the references is placed after the specified perspective, the variable is needed
		if (variable.References.Any(i => !blacklist.Any(j => i.IsUnder(j)) && i.IsAfter(perspective)))
		{
			return true;
		}

		return perspective.FindParent(i => i.Is(NodeType.LOOP)) != null;
	}

	private static List<Edit> GetPastEdits(Node reference, IEnumerable<Edit> edits)
	{
		// Get a blacklist which describes which sections of the node tree have not been executed in the past or won't be executed in the future
		var blacklist = GetBlacklist(reference);

		return edits.Reverse()
			.SkipWhile(e => !e.GetRoot().IsBefore(reference)) // Take while the edits are before the specified reference
			.Where(e => !blacklist.Any(i => e.Node.IsUnder(i))) // Filter out all the edits which are not in the execution paths that lead to the specified reference
			.ToList();
	}

	private class Edit
	{
		public Variable Variable { get; set; }
		public Node Node { get; set; }
		public List<Node> Dependencies { get; private set; } = new List<Node>();
		public bool Required => Dependencies.Any();

		public Edit(Variable variable, Node node)
		{
			Variable = variable;
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
			return Node.FindParent(p => p.Is(NodeType.INCREMENT, NodeType.DECREMENT) ||
				p.Is(NodeType.OPERATOR) && p.To<OperatorNode>().Operator.Type == OperatorType.ACTION)
				?? throw new ApplicationException("Could not find the root of a edit");
		}

		public Variable[] GetVariableDependencies()
		{
			var root = GetRoot();

			if (root.Is(NodeType.INCREMENT, NodeType.DECREMENT))
			{
				return Array.Empty<Variable>();
			}

			return root.FindAll(i => i.Is(NodeType.VARIABLE))
				.Select(i => i.To<VariableNode>().Variable)
				.Distinct()
				.Where(i => i != Variable)
				.ToArray();
		}
	}

	/// <summary>
	/// Toggles all edits which are encountered in the specified node tree and returns whether the edits are bypassable
	/// </summary>
	/// <returns>
	/// Returns whether the specified node tree is executable without encountering the specified edits
	/// </returns>
	private static bool Register(Node node, Node dependency, List<Edit> edits)
	{
		var edit = edits.Find(e => e.GetRoot() == node);

		if (edit != null)
		{
			edit.AddDependency(dependency);

			if (edit.GetRoot().Is(Operators.ASSIGN))
			{
				return false;
			}
		}

		// If the specified node tree doesn't contain any of the edits, it must be penetrable
		if (node.Find(n => edits.Any(e => e.Node == n)) == null)
		{
			return true;
		}

		if (node.Is(NodeType.IF))
		{
			var branches = node.To<IfNode>().GetBranches();
			edits.Where(e => branches.Any(b => e.Node.IsUnder(b))).ForEach(e => e.AddDependency(dependency));
		}
		else if (node.Is(NodeType.LOOP))
		{
			if (node.To<LoopNode>().IsForeverLoop && !Register(node.To<LoopNode>().Body, dependency, edits))
			{
				return false;
			}
			else
			{
				// Register all the edits inside the loop
				edits.Where(e => e.Node.IsUnder(node)).ForEach(e => e.AddDependency(dependency));

				// If the initialization contains an edit, it means it's not bypassable
				if (!Register(node.To<LoopNode>().Initialization, dependency, edits))
				{
					return false;
				}
			}
		}
		else if (!node.Is(NodeType.ELSE_IF, NodeType.ELSE))
		{
			var iterator = node.Last;

			while (iterator != null)
			{
				if (!Register(iterator, dependency, edits))
				{
					return false;
				}

				iterator = iterator.Previous;
			}
		}

		return true;
	}

	private static Node? StepOutside(Node? iterator, List<Edit> edits, Node dependency)
	{
		if (iterator == null)
		{
			return null;
		}

		switch (iterator.GetNodeType())
		{
			case NodeType.LOOP:
			{
				// All edits which are inside the current loop are needed by the dependency node
				edits.Where(e => e.Node.IsUnder(iterator)).ForEach(e => e.AddDependency(dependency));

				if (iterator.Previous != null)
				{
					return iterator.Previous;
				}

				return iterator.Previous ?? StepOutside(iterator.Parent, edits, dependency);
			}

			case NodeType.IF:
			{
				return iterator.Previous ?? StepOutside(iterator.Parent, edits, dependency);
			}

			case NodeType.ELSE_IF:
			{
				iterator = iterator.To<ElseIfNode>().GetRoot();

				return iterator.Previous ?? StepOutside(iterator.Parent, edits, dependency);
			}

			case NodeType.ELSE:
			{
				iterator = iterator.To<ElseNode>().GetRoot();

				return iterator.Previous ?? StepOutside(iterator.Parent, edits, dependency);
			}

			case NodeType.CONTEXT:
			case NodeType.NORMAL:
			{
				return StepOutside(iterator.Parent, edits, dependency);
			}

			case NodeType.IMPLEMENTATION:
			{
				return null;
			}

			default:
			{
				return iterator;
			}
		}
	}

	/// <summary>
	/// Iterates backwards starting from the specified node and registers all edits as needed which may affect the specified node
	/// </summary>
	/// <returns>Returns the edit which is closest to the specified node</returns>
	private static Edit? RegisterSignificantEdits(Node node, Node dependency, List<Edit> edits)
	{
		var iterator = (Node?)node;
		var past = GetPastEdits(node, edits);

		while (iterator != null)
		{
			if (!Register(iterator, dependency, past))
			{
				return past.FirstOrDefault();
			}

			if (iterator.Previous == null)
			{
				iterator = StepOutside(iterator.Parent, edits, dependency);
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
		var x = read.FindParent(p => p is IContext && !(p.Is(NodeType.LOOP) && p.To<LoopNode>().IsForeverLoop)) ?? throw new ApplicationException("Analysis executed outside of a context");
		var y = edit.FindParent(p => p is IContext && !(p.Is(NodeType.LOOP) && p.To<LoopNode>().IsForeverLoop)) ?? throw new ApplicationException("Analysis executed outside of a context");

		return x != y && !x.IsUnder(y);
	}

	private static bool IsAssignable(Node read, Edit edit, List<Edit> edits, Dictionary<Variable, VariableReferenceDescriptor> descriptors)
	{
		var root = edit.GetRoot()!;

		if (!root.Is(Operators.ASSIGN))
		{
			/// TODO: Assignment value is possible to calculate here sometimes
			return false;
		}

		// Skip edits which have function calls or they are branched from the perspective of the read
		if (!IsPrimitive(root) || IsBranched(read, edit.Node))
		{
			return false;
		}

		// Collect all variables in the value of the edit
		var dependencies = edit.GetVariableDependencies();

		// If any of the depedency variables is between the edit and the read, the edit can not be assigned
		if (dependencies.SelectMany(i => descriptors.GetValueOrDefault(i)?.Writes ?? new List<Node>()).Any(i => i.IsBetween(edit.Node, read)))
		{
			return false;
		}

		var loop = read.FindParent(p => p.Is(NodeType.LOOP));

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

	/// <summary>
	/// Tries to simplify the specified node
	/// </summary>
	private static Node GetSimplifiedValue(Node value)
	{
		var components = CollectComponents(value);
		var simplified = Recreate(components);

		return simplified;
	}

	private static Node AssignVariable(FunctionImplementation context, Node node, Variable variable, out Dictionary<Variable, VariableReferenceDescriptor> descriptors, bool clone = true)
	{
		var root = clone ? node.Clone() : node;

		// Retrieve descriptors for each of the variables in the context
		descriptors = new Dictionary<Variable, VariableReferenceDescriptor>(context.Locals.Concat(context.Parameters).Select(
			v => new KeyValuePair<Variable, VariableReferenceDescriptor>(v, GetVariableReferenceDescriptor(root, v))
		), new VariableEqualityComparer());

		var descriptor = descriptors[variable];
		var edits = descriptor.Writes.Select(i => new Edit(variable, i)).ToList();

		foreach (var read in descriptor.Reads)
		{
			// Register all of the past edits which are needed by the current reference
			// Retrieve the latest edit which concerns the current reference
			var edit = RegisterSignificantEdits(read, read, edits);

			// Try to inline the value from the assignment
			if (edit != null && IsAssignable(read, edit, edits, descriptors))
			{
				var assignment = edit.GetRoot();

				edit.RemoveDependency(read);

				// Optimize the value of the assignment is allowed
				var value = IsMathematicalOptimizationEnabled ? GetSimplifiedValue(assignment.Last!) : assignment.Last!.Clone();

				// Replace the reference with the value of the assignment
				read.Replace(value);
			}
		}

		// Remove all edits which are not needed or have been assigned completely
		edits.Where(e => !e.Required).ForEach(e => e.GetRoot().Remove());

		return root;
	}

	private static bool OptimizeComparisons(Node root)
	{
		var comparisons = root.FindAll(n => n.Is(NodeType.OPERATOR) && n.To<OperatorNode>().Operator.Type == OperatorType.COMPARISON);
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

	private static bool UnwrapStatements(Node root, FunctionImplementation context)
	{
		var iterator = root.First;
		var unwrapped = false;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.IF))
			{
				var statement = iterator.To<IfNode>();

				if (statement.Condition.Is(NodeType.NUMBER))
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

						if (statement.Successor.Is(NodeType.ELSE))
						{
							iterator = statement.Successor.Next;

							// Replace the conditional statement with the body of the successor
							statement.ReplaceWithChildren(statement.Successor.To<ElseNode>().Body);

							unwrapped = true;
							continue;
						}

						var successor = statement.Successor.To<ElseIfNode>();

						// Create a conditional statement identical to the successor but as an if-statement
						var replacement = new IfNode(successor.Context);
						successor.ForEach(i => replacement.Add(i));

						iterator = replacement;

						successor.Remove();

						statement.Replace(replacement);

						unwrapped = true;
					}

					continue;
				}
			}
			else if (iterator.Is(NodeType.LOOP))
			{
				var statement = iterator.To<LoopNode>();

				if (!statement.IsForeverLoop)
				{
					if (!statement.Condition.Is(NodeType.NUMBER))
					{
						iterator = statement.Next;

						if (TryUnwrapLoop(root, context, statement))
						{
							unwrapped = true;
						}

						continue;
					}

					if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
					{
						statement.Parent!.Insert(statement, statement.Initialization);

						var replacement = new LoopNode(statement.Context, null, statement.Body);
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

		// Ensure there is only one condition present
		var condition = loop.Condition;

		if (loop.GetConditionInitialization().Any())
		{
			return null;
		}

		if (!condition.Is(NodeType.OPERATOR) ||
			condition.To<OperatorNode>().Operator.Type != OperatorType.COMPARISON ||
			!IsPrimitive(condition))
		{
			return null;
		}

		// Ensure that the initialization is empty or it contains a definition of an integer variable
		var initialization = loop.Initialization;

		if (initialization.IsEmpty || initialization.First != initialization.Last)
		{
			Console.WriteLine("Analysis encountered an empty loop initialization which canceled the attempt of unwrapping the loop");
			return null;
		}

		initialization = initialization.First!;

		if (!initialization.Is(Operators.ASSIGN) || !initialization.First!.Is(NodeType.VARIABLE))
		{
			return null;
		}

		// Make sure the variable is predictable and it's an integer
		var variable = initialization.First!.To<VariableNode>().Variable;

		if (!variable.IsPredictable ||
			!(initialization.First.To<VariableNode>().Variable.Type is Number) ||
			!initialization.Last!.Is(NodeType.NUMBER))
		{
			return null;
		}

		var start_value = initialization.Last.To<NumberNode>().Value;

		// Ensure there is only one action present
		var action = loop.Action;

		if (action.IsEmpty || action.First != action.Last)
		{
			return null;
		}

		action = action.First!;

		var step_value = new List<Component>();

		if (action.Is(NodeType.INCREMENT))
		{
			var statement = action.To<IncrementNode>();

			if (!statement.Object.Is(variable))
			{
				return null;
			}

			step_value.Add(new NumberComponent(1L));
		}
		else if (action.Is(NodeType.DECREMENT))
		{
			var statement = action.To<IncrementNode>();

			if (!statement.Object.Is(variable))
			{
				return null;
			}

			step_value.Add(new NumberComponent(-1L));
		}
		else if (action.Is(NodeType.OPERATOR))
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

	public static bool TryUnwrapLoop(Node root, FunctionImplementation context, LoopNode loop)
	{
		var descriptor = TryGetLoopUnwrapDescriptor(loop);

		if (descriptor == null)
		{
			return false;
		}

		loop.InsertChildren(loop.Initialization.Clone());

		var action = TryRewriteAsAssignOperation(loop.Action.First!) ?? loop.Action.First!.Clone();

		for (var i = 0; i < descriptor.Steps; i++)
		{
			loop.InsertChildren(loop.Body.Clone());
			loop.Insert(action.Clone());
		}

		loop.Remove();

		AssignVariable(context, root, descriptor.Iterator, out Dictionary<Variable, VariableReferenceDescriptor> _, false);
		return true;
	}

	private static Node? TryRewriteAsAssignOperation(Node edit)
	{
		if (IsValueUsed(edit))
		{
			return null;
		}

		switch (edit)
		{
			case IncrementNode increment:
			{

				var destination = increment.Object.Clone().To<VariableNode>();

				return new OperatorNode(Operators.ASSIGN).SetOperands(
					destination,
					new OperatorNode(Operators.ADD).SetOperands(
						destination.Clone(),
						new NumberNode(destination.Variable.Type!.Format, 1L)
					)
				);
			}

			case DecrementNode decrement:
			{

				var destination = decrement.Object.Clone().To<VariableNode>();

				return new OperatorNode(Operators.ASSIGN).SetOperands(
					destination,
					new OperatorNode(Operators.SUBTRACT).SetOperands(
						destination.Clone(),
						new NumberNode(destination.Variable.Type!.Format, 1L)
					)
				);
			}

			case OperatorNode operation:
			{

				if (operation.Operator.Type != OperatorType.ACTION)
				{
					return null;
				}

				var destination = operation.Left.Clone().To<VariableNode>();
				var type = ((ActionOperator)operation.Operator).Operator;

				if (type == null)
				{
					return null;
				}

				return new OperatorNode(Operators.ASSIGN).SetOperands(
					destination,
					new OperatorNode(type).SetOperands(
						destination.Clone(),
						edit.Last!.Clone()
					)
				);
			}
		}

		return null;
	}

	private static Node? TryRewriteAsActionOperation(Node edit)
	{
		if (!edit.Is(NodeType.INCREMENT, NodeType.DECREMENT) || IsValueUsed(edit))
		{
			return null;
		}

		if (edit is IncrementNode increment)
		{
			var destination = increment.Object.Clone();
			var type = destination.TryGetType() ?? throw new ApplicationException("Could not retrieve type from increment node");

			return new OperatorNode(Operators.ASSIGN_ADD).SetOperands(
				destination,
				new NumberNode(type.Format, 1L)
			);
		}

		if (edit is DecrementNode decrement)
		{
			var destination = decrement.Object.Clone();
			var type = destination.TryGetType() ?? throw new ApplicationException("Could not retrieve type from decrement node");

			return new OperatorNode(Operators.ASSIGN_SUBTRACT).SetOperands(
				destination,
				new NumberNode(type.Format, 1L)
			);
		}

		return null;
	}

	private static bool RemoveUnreachableStatements(Node root)
	{
		var return_statements = root.FindAll(n => n.Is(NodeType.RETURN));
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
			if (iterator.Is(NodeType.OPERATOR))
			{
				var operation = iterator.To<OperatorNode>().Operator;

				if (operation == Operators.ADD || operation == Operators.SUBTRACT)
				{
					result += 2;
				}
				else if (operation == Operators.MULTIPLY)
				{
					result += 10;
				}
				else if (operation == Operators.DIVIDE)
				{
					result += 70;
				}
				else if (operation.Type == OperatorType.COMPARISON)
				{
					result++;
				}
				else if (operation.Type == OperatorType.ACTION)
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
	/// Tries to optimize all expressions in the specified node tree
	/// </summary>
	private static void OptimizeAllExpressions(Node root)
	{
		// Find all top level operators
		var expressions = new List<Node>();

		foreach (var operation in FindTopLevelOperators(root))
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
			// Replace the expression with a simplified version
			expression.Replace(GetSimplifiedValue(expression));
		}
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
			snapshot = AssignVariable(context, snapshot, variable, out Dictionary<Variable, VariableReferenceDescriptor> descriptors);

			// Try to optimize all comparisons found in the current snapshot
			if (IsMathematicalOptimizationEnabled && OptimizeComparisons(snapshot))
			{
				goto Start;
			}

			// Try to unwrap conditional statements whose outcome have been resolved
			if (UnwrapStatements(snapshot, context))
			{
				goto Start;
			}

			// Removes all statements which are not reachable
			if (RemoveUnreachableStatements(snapshot))
			{
				goto Start;
			}

			// Now, since the variable is assigned, try to simplify the code
			if (IsMathematicalOptimizationEnabled)
			{
				OptimizeAllExpressions(snapshot);
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
		if (node.Is(NodeType.OPERATOR) && node.To<OperatorNode>().Operator.Type == OperatorType.CLASSIC)
		{
			return new List<OperatorNode> { (OperatorNode)node };
		}

		var operators = new List<OperatorNode>();
		var child = node.First;

		while (child != null)
		{
			if (child.Is(NodeType.OPERATOR))
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

	/// <summary>
	/// Returns a node representing a position where new nodes can be inserted
	/// </summary>
	private static Node GetInsertPosition(Node reference)
	{
		var iterator = reference.Parent!;
		var position = reference;

		while (!(iterator is IContext || iterator.Is(NodeType.NORMAL)))
		{
			position = iterator;
			iterator = iterator.Parent!;
		}

		return position;
	}

	private static List<OperatorNode> FindBooleanValues(Node root)
	{
		var candidates = root.FindAll(i => i.Is(NodeType.OPERATOR) && (i.To<OperatorNode>().Operator.Type == OperatorType.COMPARISON || i.To<OperatorNode>().Operator.Type == OperatorType.LOGIC)).Cast<OperatorNode>();

		return candidates.Where(candidate =>
		{
			var parent = candidate.FindParent(i => !i.Is(NodeType.CONTENT))!;

			if (!parent.Is(
				NodeType.CAST,
				NodeType.CONSTRUCTION,
				NodeType.DECREMENT,
				NodeType.ELSE_IF,
				NodeType.ELSE,
				NodeType.FUNCTION,
				NodeType.INCREMENT,
				NodeType.LINK,
				NodeType.LOOP,
				NodeType.NEGATE,
				NodeType.NOT,
				NodeType.OFFSET,
				NodeType.RETURN
			))
			{
				return false;
			}

			return !(parent is OperatorNode operation && (operation.Operator.Type == OperatorType.COMPARISON || operation.Operator.Type == OperatorType.LOGIC));

		}).ToList();
	}

	private static void OutlineBooleanValues(Node root)
	{
		var instances = FindBooleanValues(root);

		foreach (var instance in instances)
		{
			var position = GetInsertPosition(instance);

			// Declare a hidden variable which represents the result
			var environment = instance.FindContext()?.GetContext() ?? throw new ApplicationException("Could not find the current context");
			var destination = environment.DeclareHidden(Types.BOOL);

			// Initialize the result with value 'false'
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 0L)
			);

			// Replace the operation with the result
			var replacement = new VariableNode(destination);
			instance.Replace(replacement);

			var context = new Context();
			context.Link(environment);

			// The destination is edited inside the following statement
			var edit = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 1L)
			);

			destination.Edits.Add(edit);

			// Create a conditional statement which sets the value of the destination variable to true if the condition is true
			var statement = new IfNode(
			   context,
			   instance,
			   edit
			);

			// Add the statements which implement the boolean value
			position.Insert(statement);
			statement.Insert(initialization);
		}
	}

	private static bool IsValueUsed(Node value)
	{
		return value.Parent!.Is(
			NodeType.CAST,
			NodeType.CONSTRUCTION,
			NodeType.CONTENT,
			NodeType.DECREMENT,
			NodeType.FUNCTION,
			NodeType.INCREMENT,
			NodeType.LINK,
			NodeType.NEGATE,
			NodeType.NOT,
			NodeType.OFFSET,
			NodeType.OPERATOR,
			NodeType.RETURN
		);
	}

	/// <summary>
	/// Rewrites increment and decrement operators as action operations if their values are discard.
	/// Example (value is not discarded):
	/// x = ++i
	/// Before (value is discarded):
	/// loop (i = 0, i < n, i++)
	/// After:
	/// loop (i = 0, i < n, i += 1)
	/// </summary>
	private static void RewriteDiscardedIncrements(Node root)
	{
		var increments = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT));

		foreach (var increment in increments)
		{
			if (IsValueUsed(increment))
			{
				continue;
			}

			var replacement = TryRewriteAsActionOperation(increment) ?? throw new ApplicationException("Could not rewrite increment operation as assign operation");

			increment.Replace(replacement);
		}
	}

	/// <summary>
	/// Removes negation nodes which cancel each other out.
	/// Example: x = -(-a)
	/// </summary>
	private static void RemoveCancellingNegations(Node root)
	{
		if (root is NegateNode x && x.Object is NegateNode y)
		{
			root.Replace(y.Object);
			RemoveCancellingNegations(y.Object);
			return;
		}

		foreach (var iterator in root)
		{
			RemoveCancellingNegations(iterator);
		}
	}

	/// <summary>
	/// Removes not nodes which cancel each other out.
	/// Example: x = !!a
	/// </summary>
	private static void RemoveCancellingNots(Node root)
	{
		if (root is NotNode x && x.Object is NotNode y)
		{
			root.Replace(y.Object);
			RemoveCancellingNegations(y.Object);
			return;
		}

		foreach (var iterator in root)
		{
			RemoveCancellingNots(iterator);
		}
	}

	/// <summary>
	/// Removes redundant parenthesis in the specified node tree
	/// Example: x = x * (((x + 1)))
	/// </summary>
	private static void RemoveRedundantParenthesis(Node root)
	{
		if (root.Is(NodeType.CONTENT) || root.Is(NodeType.LIST))
		{
			foreach (var iterator in root)
			{
				if (iterator is ContentNode parenthesis && parenthesis.Count() == 1)
				{
					iterator.Replace(iterator.First!);
				}
			}
		}

		root.ForEach(RemoveRedundantParenthesis);
	}

	public static void Analyze(Context context)
	{
		foreach (var type in context.Types.Values)
		{
			Analyze(type);
		}

		foreach (var implementation in context.GetImplementedFunctions())
		{
			RemoveRedundantParenthesis(implementation.Node!);
			RemoveCancellingNegations(implementation.Node!);
			RemoveCancellingNots(implementation.Node!);
			OutlineBooleanValues(implementation.Node!);
			RewriteDiscardedIncrements(implementation.Node!);

			implementation.Node = Optimize(implementation.Node!, implementation);

			// Analyze lambdas for example
			Analyze(implementation);
		}
	}

	// TODO: Remove redundant casts and add necessary casts
	// TODO: Remove exploits such as: !(!(!(x)))
}