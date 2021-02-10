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
		var self_type = node.Left.GetType();

		if (node.Right.Is(NodeType.VARIABLE))
		{
			var variable = node.Right.To<VariableNode>().Variable;

			if (variable.Category == VariableCategory.GLOBAL)
			{
				return References.GetVariable(unit, variable, mode);
			}

			var start = References.Get(unit, node.Left);
			var alignment = variable.GetAlignment(self_type) ?? throw new ApplicationException("Member variable was not aligned");

			return new GetObjectPointerInstruction(unit, variable, start, alignment, mode).Execute();
		}

		if (!node.Right.Is(NodeType.FUNCTION))
		{
			throw new NotImplementedException("Unsupported member node");
		}

		return GetMemberFunctionCall(unit, node.Right.To<FunctionNode>(), node.Left, self_type);
	}
}