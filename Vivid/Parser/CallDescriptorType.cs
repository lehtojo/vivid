using System.Collections.Generic;
using System.Linq;

public class CallDescriptorType : Type
{
	public Type? Self { get; private set; }
	public List<Type?> Parameters { get; set; }
	public Type? ReturnType { get; }

	public CallDescriptorType(List<Type?> parameters, Type? return_type) : base(string.Empty, AccessModifier.PUBLIC)
	{
		Parameters = parameters;
		ReturnType = return_type;
	}

	public CallDescriptorType(Type self, List<Type?> parameters, Type? return_type) : base(string.Empty, AccessModifier.PUBLIC)
	{
		Self = self;
		Parameters = parameters;
		ReturnType = return_type;
	}

	public override bool IsResolved()
	{
		return true;
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle += 'F';

		if (ReturnType == global::Types.UNIT)
		{
			mangle += 'v';
		}
		else
		{
			mangle += ReturnType!;
		}

		mangle += Parameters!;
		mangle += 'E';
	}

	public override bool Equals(object? other)
	{
		if (!(other is CallDescriptorType type) || Parameters.Count != type.Parameters.Count)
		{
			return false;
		}

		for (var i = 0; i < Parameters.Count; i++)
		{
			if (Resolver.GetSharedType(Parameters[i], type.Parameters[i]) == null)
			{
				return false;
			}
		}

		return ReturnType == type.ReturnType || Resolver.GetSharedType(ReturnType, type.ReturnType) != null;
	}

	public override string ToString()
	{
		return $"({string.Join(", ", Parameters.Select(p => p?.ToString() ?? "_").ToArray())}) => {ReturnType?.ToString() ?? "_"}";
	}
}