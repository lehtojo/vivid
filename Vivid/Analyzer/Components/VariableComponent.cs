using System;
using System.Collections.Generic;

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

			return new VariableProductComponent(coefficient, new List<VariableComponent> { this, variable_component });
		}

		if (other is NumberComponent number_component)
		{
			return new VariableComponent(Variable, Numbers.Multiply(Coefficient, number_component.Value), Order);
		}

		if (other is VariableProductComponent product)
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