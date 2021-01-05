using System;

public static class Links
{
	public static Result GetMemberFunctionCall(Unit unit, FunctionNode function, Node self_node, Type self_type)
	{
		// Static functions can not access any instance data
		if (function.Function.IsStatic)
		{
			return Calls.Build(unit, function);
		}

		// Retrieve the context where the function is defined
		var function_context = function.Function.Metadata!.GetTypeParent()!;
		var self = References.Get(unit, self_node);

		// If the function is not defined inside the type of the self pointer, it means it must have been defined in its supertypes, therefore casting is needed
		if (function_context != self_type)
		{
			self = Casts.Cast(unit, self, self_type, function_context);
		}

		return Calls.Build(unit, self, function);
	}

	public static Result Build(Unit unit, LinkNode node, AccessMode mode)
	{
		var self_type = node.Object.GetType();

		return node.Member switch
		{
			VariableNode member when member.Variable.Category == VariableCategory.GLOBAL => References.GetVariable(unit,
				member.Variable, mode),

			VariableNode member => new GetObjectPointerInstruction(unit, member.Variable, References.Get(unit, node.Object), member.Variable.GetAlignment(self_type) ?? throw new ApplicationException("Member variable was not aligned"), mode).Execute(),

			FunctionNode function => GetMemberFunctionCall(unit, function, node.Object, self_type),

			_ => throw new NotImplementedException("Unsupported member node")
		};
	}
}