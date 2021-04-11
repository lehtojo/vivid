using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionType : UnresolvedType
{
	public Type? Self { get; private set; }
	public List<Type?> Parameters { get; }
	public Type? ReturnType { get; }

	public FunctionType(List<Type?> parameters, Type? return_type) : base(string.Empty)
	{
		Parameters = parameters;
		ReturnType = return_type;
	}

	public FunctionType(Type self, List<Type?> parameters, Type? return_type) : base(string.Empty)
	{
		Self = self;
		Parameters = parameters;
		ReturnType = return_type;
	}

	public override Node? Resolve(Context context)
	{
		var resolved = Parameters.Select(i => (i == null || !i.IsUnresolved) ? null : Resolver.Resolve(context, i)).ToArray();

		for (var i = 0; i < resolved.Length; i++)
		{
			var iterator = resolved[i];

			if (iterator != null)
			{
				Parameters[i] = iterator;
			}
		}

		return null;
	}

	public override bool IsResolved()
	{
		return Parameters.All(i => i != null && !i.IsUnresolved);
	}

	public override Type? GetOffsetType()
	{
		return global::Link.GetVariant(global::Types.U64);
	}

	public override bool Equals(object? other)
	{
		if (other is not FunctionType type || Parameters.Count != type.Parameters.Count)
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
		return $"({string.Join(", ", Parameters.Select(p => p?.ToString() ?? "_").ToArray())}) -> {ReturnType?.ToString() ?? "_"}";
	}
}