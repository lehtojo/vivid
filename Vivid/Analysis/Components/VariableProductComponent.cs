using System.Collections.Generic;
using System.Linq;

public class VariableProductComponent : Component
{
	public object Coefficient { get; set; } = 0L;
	public List<VariableComponent> Variables { get; private set; } = new List<VariableComponent>();

	public VariableProductComponent(object coefficient, List<VariableComponent> variables)
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
		if (other is not VariableProductComponent product || Variables.Count != product.Variables.Count)
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

		var clone = (VariableProductComponent)Clone();
		clone.Coefficient = Numbers.Add(Coefficient, ((VariableProductComponent)other).Coefficient);

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

		var clone = (VariableProductComponent)Clone();
		clone.Coefficient = Numbers.Subtract(Coefficient, ((VariableProductComponent)other).Coefficient);

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

				var clone = (VariableProductComponent)Clone();
				clone.Coefficient = coefficient;

				return clone;
			}

			case VariableComponent x:
			{
				var coefficient = Numbers.Multiply(Coefficient, x.Coefficient);

				var clone = (VariableProductComponent)Clone();
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

			case VariableProductComponent product:
			{
				var coefficient = Numbers.Multiply(Coefficient, product.Coefficient);

				var clone = (VariableProductComponent)Clone();
				clone.Coefficient = coefficient;

				foreach (var x in product.Variables)
				{
					clone = (VariableProductComponent)clone.Multiply(x)!;
				}

				return clone;
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

		if (other is NumberComponent number && !Numbers.IsZero(number.Value))
		{
			if (Coefficient is long && number.Value is long && !Numbers.IsZero(Numbers.Remainder(Coefficient, number.Value)))
			{
				return null;
			}

			var coefficient = Numbers.Divide(Coefficient, number.Value);

			var clone = (VariableProductComponent)Clone();
			clone.Coefficient = coefficient;

			return clone;
		}

		return null;
	}

	public override Component Clone()
	{
		var clone = (VariableProductComponent)MemberwiseClone();
		clone.Variables = Variables.Select(v => (VariableComponent)v.Clone()).ToList();

		return clone;
	}
}