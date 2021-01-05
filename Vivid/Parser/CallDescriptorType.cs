using System.Collections.Generic;
using System.Linq;
using System;

public class CallDescriptorType : Type
{
	public Type? Self { get; private set; }
	public List<Type?> Parameters { get; }
	public Type? ReturnType { get; }

	public CallDescriptorType(List<Type?> parameters, Type? return_type) : base(string.Empty, Modifier.PUBLIC)
	{
		Parameters = parameters;
		ReturnType = return_type;
	}

	public CallDescriptorType(Type self, List<Type?> parameters, Type? return_type) : base(string.Empty, Modifier.PUBLIC)
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

	public override Type? GetOffsetType()
	{
		return global::Types.LINK;
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

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Self);
		hash.Add(Parameters);
		hash.Add(ReturnType);
		return hash.ToHashCode();
	}
	
	public override string ToString()
	{
		return $"({string.Join(", ", Parameters.Select(p => p?.ToString() ?? "_").ToArray())}) => {ReturnType?.ToString() ?? "_"}";
	}
}