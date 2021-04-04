using System;

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
			VariableComponent variable_component => new VariableComponent(variable_component.Variable, Numbers.Multiply(Value, variable_component.Coefficient)),
			VariableProductComponent product => product * this,
			_ => null
		};
	}

	public override Component? Divide(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		if (other is NumberComponent number_component && !Numbers.IsZero(number_component.Value))
		{
			return new NumberComponent(Numbers.Divide(Value, number_component.Value));
		}

		return null;
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