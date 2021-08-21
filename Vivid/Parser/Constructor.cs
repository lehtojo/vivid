using System.Collections.Generic;
using System;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position? start, Position? end)
	{
		return new Constructor(context, Modifier.DEFAULT, start, end, true);
	}

	public Constructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}

	/// <summary>
	/// Adds the member variable initializations to the specified constructor implementation
	/// </summary>
	public void AddMemberInitializations(FunctionImplementation implementation)
	{
		var root = implementation.Node!;
		var parent = (Type)Parent!;

		for (var i = parent.Initialization.Length - 1; i >= 0; i--)
		{
			var initialization = parent.Initialization[i].Clone();

			// Skip initializations of static and constant variables
			if (initialization.Is(Operators.ASSIGN))
			{
				var edited = Analyzer.GetEdited(initialization);

				if (edited.Is(NodeType.VARIABLE))
				{
					var variable = edited.To<VariableNode>().Variable;
					if (variable.IsStatic || variable.IsConstant) continue;
				}
			}

			// Find all member accesses, which do not use the self pointer but require it
			var self = Common.GetSelfPointer(implementation, null);
			var member_accessors = initialization.FindAll(i => Analysis.IsSelfPointerRequired(i));

			// Add self pointer to all member accessors
			foreach (var member in member_accessors)
			{
				member.Replace(new LinkNode(self.Clone(), member.Clone()));
			}

			root.Insert(root.First, initialization);
		}
	}

	/// <summary>
	/// Adds the member variable initializations for all existing implementations.
	/// NOTE: This should not be executed twice, as it will cause duplicate initializations.
	/// </summary>
	public void AddMemberInitializations()
	{
		foreach (var implementation in Implementations)
		{
			AddMemberInitializations(implementation);
		}
	}

	public override FunctionImplementation Implement(IEnumerable<Type> types)
	{
		// Implement the constructor and then add the parent type initializations to the beginning of the function body
		var implementation = base.Implement(types);
		implementation.ReturnType = Primitives.CreateUnit();
		AddMemberInitializations(implementation);
		return implementation;
	}
}

public class Destructor : Function
{
	public bool IsDefault { get; private set; }

	public static Destructor Empty(Context context, Position? start, Position? end)
	{
		return new Destructor(context, Modifier.DEFAULT, start, end, true);
	}

	public Destructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.DEINIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}

	public override FunctionImplementation Implement(IEnumerable<Type> types)
	{
		var implementation = base.Implement(types);
		implementation.ReturnType = Primitives.CreateUnit();
		return implementation;
	}
}